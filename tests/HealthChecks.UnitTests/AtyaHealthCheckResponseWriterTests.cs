// <copyright file="AtyaHealthCheckResponseWriterTests.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>

using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Atya.Web.HealthChecks.UnitTests;

public sealed class AtyaHealthCheckResponseWriterTests
{
    /// <summary>
    /// Synthetic markers placed in every disclosable field of a health report. The minimal writer must not
    /// emit any of them. They carry no meaning outside these tests.
    /// </summary>
    private const string DescriptionMarker = "marker-description";
    private const string ExceptionMarker = "marker-exception";
    private const string DataKeyMarker = "markerDataKey";
    private const string DataValueMarker = "marker-data-value";
    private const string TagMarker = "marker-tag";
    private const string CheckNameMarker = "marker-check-name";

    [Fact]
    public async Task WriteJsonAsync_Should_Write_Only_Aggregate_Status()
    {
        var context = new DefaultHttpContext();
        await using var body = new MemoryStream();
        context.Response.Body = body;

        await AtyaHealthCheckResponseWriter.WriteJsonAsync(context, CreateDisclosingReport(HealthStatus.Healthy));

        context.Response.ContentType.Should().Be("application/json; charset=utf-8");
        using JsonDocument document = Parse(body);
        document.RootElement.GetProperty("status").GetString().Should().Be("Healthy");
        document.RootElement.EnumerateObject().Select(property => property.Name)
            .Should().Equal("status");
    }

    [Theory]
    [InlineData(HealthStatus.Healthy, "Healthy")]
    [InlineData(HealthStatus.Degraded, "Degraded")]
    [InlineData(HealthStatus.Unhealthy, "Unhealthy")]
    public async Task WriteJsonAsync_Should_Report_Aggregate_Status_For_Every_Health_State(
        HealthStatus status,
        string expected)
    {
        var context = new DefaultHttpContext();
        await using var body = new MemoryStream();
        context.Response.Body = body;

        await AtyaHealthCheckResponseWriter.WriteJsonAsync(context, CreateDisclosingReport(status));

        using JsonDocument document = Parse(body);
        document.RootElement.GetProperty("status").GetString().Should().Be(expected);
    }

    /// <summary>
    /// Regression guard for the minimal-by-default response contract: no per-check diagnostic supplied by an
    /// application health check may reach the response body.
    /// </summary>
    [Fact]
    public async Task WriteJsonAsync_Should_Not_Disclose_Any_Check_Diagnostics()
    {
        var context = new DefaultHttpContext();
        await using var body = new MemoryStream();
        context.Response.Body = body;

        await AtyaHealthCheckResponseWriter.WriteJsonAsync(context, CreateDisclosingReport(HealthStatus.Unhealthy));

        string json = Encoding.UTF8.GetString(body.ToArray());
        json.Should().NotContainAny(
            CheckNameMarker,
            DescriptionMarker,
            ExceptionMarker,
            DataKeyMarker,
            DataValueMarker,
            TagMarker);
        json.Should().NotContainAny("entries", "totalDuration", "duration", "description", "exception", "data", "tags");
    }

    [Fact]
    public async Task WriteJsonAsync_Should_Throw_When_Context_Is_Null()
    {
        var report = new HealthReport(new Dictionary<string, HealthReportEntry>(), TimeSpan.Zero);

        var act = () => AtyaHealthCheckResponseWriter.WriteJsonAsync(null!, report);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task WriteJsonAsync_Should_Throw_When_Report_Is_Null()
    {
        var context = new DefaultHttpContext();

        var act = () => AtyaHealthCheckResponseWriter.WriteJsonAsync(context, null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task WriteDetailedJsonAsync_Should_Write_Health_Report_As_Json()
    {
        var context = new DefaultHttpContext();
        await using var body = new MemoryStream();
        context.Response.Body = body;
        var report = new HealthReport(
            new Dictionary<string, HealthReportEntry>
            {
                ["database"] = new(
                    HealthStatus.Healthy,
                    "Database reachable.",
                    TimeSpan.FromMilliseconds(7),
                    null,
                    new Dictionary<string, object> { ["region"] = "eu" },
                    [AtyaHealthCheckTags.Ready]),
            },
            TimeSpan.FromMilliseconds(11));

        await AtyaHealthCheckResponseWriter.WriteDetailedJsonAsync(context, report);

        context.Response.ContentType.Should().Be("application/json; charset=utf-8");
        using JsonDocument document = Parse(body);

        document.RootElement.GetProperty("status").GetString().Should().Be("Healthy");
        JsonElement entry = document.RootElement.GetProperty("entries").GetProperty("database");
        entry.GetProperty("status").GetString().Should().Be("Healthy");
        entry.GetProperty("description").GetString().Should().Be("Database reachable.");
        entry.GetProperty("tags")[0].GetString().Should().Be(AtyaHealthCheckTags.Ready);
        entry.GetProperty("data").GetProperty("region").GetString().Should().Be("eu");
        entry.GetProperty("exception").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task WriteDetailedJsonAsync_Should_Include_Check_Diagnostics()
    {
        var context = new DefaultHttpContext();
        await using var body = new MemoryStream();
        context.Response.Body = body;

        await AtyaHealthCheckResponseWriter.WriteDetailedJsonAsync(
            context,
            CreateDisclosingReport(HealthStatus.Unhealthy));

        using JsonDocument document = Parse(body);
        JsonElement entry = document.RootElement.GetProperty("entries").GetProperty(CheckNameMarker);
        entry.GetProperty("description").GetString().Should().Be(DescriptionMarker);
        entry.GetProperty("exception").GetString().Should().Be(ExceptionMarker);
        entry.GetProperty("data").GetProperty(DataKeyMarker).GetString().Should().Be(DataValueMarker);
        entry.GetProperty("tags")[0].GetString().Should().Be(TagMarker);
    }

    [Fact]
    public async Task WriteDetailedJsonAsync_Should_Throw_When_Context_Is_Null()
    {
        var report = new HealthReport(new Dictionary<string, HealthReportEntry>(), TimeSpan.Zero);

        var act = () => AtyaHealthCheckResponseWriter.WriteDetailedJsonAsync(null!, report);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task WriteDetailedJsonAsync_Should_Throw_When_Report_Is_Null()
    {
        var context = new DefaultHttpContext();

        var act = () => AtyaHealthCheckResponseWriter.WriteDetailedJsonAsync(context, null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    private static HealthReport CreateDisclosingReport(HealthStatus status)
    {
        return new HealthReport(
            new Dictionary<string, HealthReportEntry>
            {
                [CheckNameMarker] = new(
                    status,
                    DescriptionMarker,
                    TimeSpan.FromMilliseconds(7),
                    new InvalidOperationException(ExceptionMarker),
                    new Dictionary<string, object> { [DataKeyMarker] = DataValueMarker },
                    [TagMarker]),
            },
            TimeSpan.FromMilliseconds(11));
    }

    private static JsonDocument Parse(MemoryStream body)
    {
        return JsonDocument.Parse(body.ToArray());
    }
}
