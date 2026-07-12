// <copyright file="AtyaHealthCheckResponseWriterTests.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>

using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Atya.Web.HealthChecks.UnitTests;

public sealed class AtyaHealthCheckResponseWriterTests
{
    [Fact]
    public async Task WriteJsonAsync_Should_Write_Health_Report_As_Json()
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

        await AtyaHealthCheckResponseWriter.WriteJsonAsync(context, report);

        context.Response.ContentType.Should().Be("application/json; charset=utf-8");
        body.Position = 0;
        using JsonDocument document = await JsonDocument.ParseAsync(
            body,
            cancellationToken: TestContext.Current.CancellationToken);

        document.RootElement.GetProperty("status").GetString().Should().Be("Healthy");
        document.RootElement.GetProperty("entries").GetProperty("database").GetProperty("status").GetString()
            .Should().Be("Healthy");
        document.RootElement.GetProperty("entries").GetProperty("database").GetProperty("tags")[0].GetString()
            .Should().Be(AtyaHealthCheckTags.Ready);
        document.RootElement.GetProperty("entries").GetProperty("database").GetProperty("data").GetProperty("region")
            .GetString()
            .Should().Be("eu");
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
}
