// <copyright file="EndpointRouteBuilderExtensionsTests.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>

using System.Text;
using System.Text.Json;
using Atya.Web.HealthChecks.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Atya.Web.HealthChecks.UnitTests;

public sealed class EndpointRouteBuilderExtensionsTests
{
    private const string CheckName = "marker-check-name";
    private const string CheckDescription = "marker-description";
    private const string DataKey = "markerDataKey";
    private const string DataValue = "marker-data-value";

    [Fact]
    public void MapAtyaWebHealthChecks_Should_Throw_When_Endpoints_Is_Null()
    {
        var act = () => Extensions.EndpointRouteBuilderExtensions.MapAtyaWebHealthChecks(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void MapAtyaWebHealthChecks_Should_Map_Default_Endpoints()
    {
        using var app = CreateApp();

        app.MapAtyaWebHealthChecks();

        GetRoutePatterns(app).Should().Contain(["/health", "/health/live", "/health/ready"]);
    }

    [Fact]
    public void MapAtyaWebHealthChecks_Should_Map_Configured_Endpoints()
    {
        using var app = CreateApp();

        app.MapAtyaWebHealthChecks(options =>
        {
            options.HealthPath = "/status";
            options.LivePath = "/status/live";
            options.ReadyPath = "/status/ready";
        });

        GetRoutePatterns(app).Should().Contain(["/status", "/status/live", "/status/ready"]);
    }

    [Fact]
    public void MapAtyaWebHealthChecks_Should_Reject_Duplicate_Paths()
    {
        using var app = CreateApp();

        var act = () => app.MapAtyaWebHealthChecks(options => options.LivePath = "/health");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*distinct*");
    }

    [Fact]
    public void MapAtyaWebHealthChecks_Should_Apply_Conventions_To_All_Endpoints()
    {
        using var app = CreateApp();
        var metadata = new TestMetadata();

        app.MapAtyaWebHealthChecks()
            .WithMetadata(metadata);

        GetRouteEndpoints(app)
            .Where(endpoint => endpoint.RoutePattern.RawText?.StartsWith("/health", StringComparison.Ordinal) == true)
            .Should()
            .OnlyContain(endpoint => endpoint.Metadata.GetMetadata<TestMetadata>() == metadata);
    }

    [Fact]
    public void MapAtyaWebHealthChecks_Should_Apply_Final_Conventions_To_All_Endpoints()
    {
        using var app = CreateApp();
        var metadata = new TestMetadata();

        app.MapAtyaWebHealthChecks()
            .Finally(endpointBuilder => endpointBuilder.Metadata.Add(metadata));

        GetRouteEndpoints(app)
            .Where(endpoint => endpoint.RoutePattern.RawText?.StartsWith("/health", StringComparison.Ordinal) == true)
            .Should()
            .OnlyContain(endpoint => endpoint.Metadata.GetMetadata<TestMetadata>() == metadata);
    }

    [Fact]
    public void ConventionBuilder_Should_Throw_When_Convention_Is_Null()
    {
        using var app = CreateApp();
        AtyaHealthChecksEndpointConventionBuilder builder = app.MapAtyaWebHealthChecks();

        Action addAct = () => builder.Add(null!);
        Action finallyAct = () => builder.Finally(null!);

        addAct.Should().Throw<ArgumentNullException>();
        finallyAct.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Regression guard for the mapped endpoints: the default wiring must serve the minimal payload, so a
    /// health check's description, data, and tags cannot leak from an unauthenticated endpoint.
    /// </summary>
    [Theory]
    [InlineData("/health")]
    [InlineData("/health/ready")]
    public async Task MapAtyaWebHealthChecks_Should_Serve_Minimal_Response_By_Default(string path)
    {
        using var app = CreateApp();
        app.MapAtyaWebHealthChecks();

        (string body, string? contentType) = await InvokeEndpointAsync(app, path);

        contentType.Should().Be("application/json; charset=utf-8");
        body.Should().Be("""{"status":"Healthy"}""");
        body.Should().NotContainAny(CheckName, CheckDescription, DataKey, DataValue);
    }

    [Fact]
    public async Task MapAtyaWebHealthChecks_Should_Serve_Detailed_Response_When_Opted_In()
    {
        using var app = CreateApp();
        app.MapAtyaWebHealthChecks(options => options.IncludeDetailedDiagnostics = true);

        (string body, string? contentType) = await InvokeEndpointAsync(app, "/health");

        contentType.Should().Be("application/json; charset=utf-8");
        using JsonDocument document = JsonDocument.Parse(body);
        JsonElement entry = document.RootElement.GetProperty("entries").GetProperty(CheckName);
        entry.GetProperty("description").GetString().Should().Be(CheckDescription);
        entry.GetProperty("data").GetProperty(DataKey).GetString().Should().Be(DataValue);
    }

    [Fact]
    public async Task MapAtyaWebHealthChecks_Should_Serve_Plain_Text_When_Json_Response_Is_Disabled()
    {
        using var app = CreateApp();
        app.MapAtyaWebHealthChecks(options => options.UseJsonResponse = false);

        (string body, string? contentType) = await InvokeEndpointAsync(app, "/health");

        contentType.Should().StartWith("text/plain");
        body.Should().Be("Healthy");
    }

    private static async Task<(string Body, string? ContentType)> InvokeEndpointAsync(
        WebApplication app,
        string path)
    {
        RouteEndpoint endpoint = GetRouteEndpoints(app)
            .Single(candidate => candidate.RoutePattern.RawText == path);

        var context = new DefaultHttpContext { RequestServices = app.Services };
        await using var body = new MemoryStream();
        context.Response.Body = body;

        await endpoint.RequestDelegate!(context);

        return (Encoding.UTF8.GetString(body.ToArray()), context.Response.ContentType);
    }

    private static WebApplication CreateApp()
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
        builder.Services.AddAtyaWebHealthChecks(builder =>
            builder.AddCheck(
                CheckName,
                () => HealthCheckResult.Healthy(
                    CheckDescription,
                    new Dictionary<string, object> { [DataKey] = DataValue }),
                tags: [AtyaHealthCheckTags.Ready]));

        return builder.Build();
    }

    private static string[] GetRoutePatterns(WebApplication app)
    {
        return GetRouteEndpoints(app)
            .Select(endpoint => endpoint.RoutePattern.RawText)
            .Where(pattern => pattern is not null)
            .Cast<string>()
            .ToArray();
    }

    private static RouteEndpoint[] GetRouteEndpoints(WebApplication app)
    {
        return ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(dataSource => dataSource.Endpoints)
            .OfType<RouteEndpoint>()
            .ToArray();
    }

    private sealed class TestMetadata
    {
    }
}
