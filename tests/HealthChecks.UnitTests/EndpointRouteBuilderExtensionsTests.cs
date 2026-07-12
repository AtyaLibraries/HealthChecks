// <copyright file="EndpointRouteBuilderExtensionsTests.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>

using Atya.Web.HealthChecks.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Atya.Web.HealthChecks.UnitTests;

public sealed class EndpointRouteBuilderExtensionsTests
{
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

    private static WebApplication CreateApp()
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
        builder.Services.AddAtyaWebHealthChecks(builder =>
            builder.AddCheck("ready", () => HealthCheckResult.Healthy(), tags: [AtyaHealthCheckTags.Ready]));

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
