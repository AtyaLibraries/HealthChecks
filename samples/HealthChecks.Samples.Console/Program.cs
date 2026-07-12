// <copyright file="Program.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>

using Atya.Web.HealthChecks;
using Atya.Web.HealthChecks.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddAtyaWebHealthChecks(healthChecks =>
{
    healthChecks.AddCheck(
        "self",
        () => HealthCheckResult.Healthy("Process is running."),
        tags: [AtyaHealthCheckTags.Live]);
    healthChecks.AddCheck(
        "database",
        () => HealthCheckResult.Healthy("Database dependency is reachable."),
        tags: [AtyaHealthCheckTags.Ready]);
});

await using WebApplication app = builder.Build();

app.MapAtyaWebHealthChecks();

string[] mappedEndpoints = ((IEndpointRouteBuilder)app).DataSources
    .SelectMany(dataSource => dataSource.Endpoints)
    .OfType<RouteEndpoint>()
    .Select(endpoint => endpoint.RoutePattern.RawText)
    .Where(pattern => pattern is not null)
    .Cast<string>()
    .Order(StringComparer.Ordinal)
    .ToArray();

Console.WriteLine("Atya.Web.HealthChecks mapped endpoints:");
foreach (string endpoint in mappedEndpoints)
{
    Console.WriteLine($"- {endpoint}");
}
