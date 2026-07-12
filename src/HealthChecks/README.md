# Atya.Web.HealthChecks

Opinionated ASP.NET Core health-check registration and endpoint helpers for Atya services.

## Install

```bash
dotnet add package Atya.Web.HealthChecks
```

## Usage

```csharp
using Atya.Web.HealthChecks;
using Atya.Web.HealthChecks.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddAtyaWebHealthChecks(healthChecks =>
{
    healthChecks.AddCheck(
        "self",
        () => HealthCheckResult.Healthy(),
        tags: [AtyaHealthCheckTags.Live]);
    healthChecks.AddCheck(
        "database",
        () => HealthCheckResult.Healthy(),
        tags: [AtyaHealthCheckTags.Ready]);
});

WebApplication app = builder.Build();
app.MapAtyaWebHealthChecks();
await app.RunAsync();
```

## Contract

`MapAtyaWebHealthChecks()` maps:

| Endpoint | Predicate |
| --- | --- |
| `/health` | all registered checks |
| `/health/live` | checks tagged `AtyaHealthCheckTags.Live` (`"live"`) |
| `/health/ready` | checks tagged `AtyaHealthCheckTags.Ready` (`"ready"`) |

The default response writer returns JSON with the overall status, total duration, and each entry's status, duration, description, tags, exception message, and data.

Endpoint paths, predicates, response caching, and JSON response writing can be customized with `AtyaHealthChecksEndpointOptions`.
