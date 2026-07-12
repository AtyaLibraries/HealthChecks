# Atya.Web.HealthChecks

Opinionated ASP.NET Core health-check registration and endpoint helpers for Atya services.

[![NuGet Version](https://img.shields.io/nuget/v/Atya.Web.HealthChecks?style=for-the-badge&logo=nuget&logoColor=white&label=NuGet&color=512BD4)](https://www.nuget.org/packages/Atya.Web.HealthChecks)
[![Downloads](https://img.shields.io/nuget/dt/Atya.Web.HealthChecks?style=for-the-badge&logo=nuget&logoColor=white&label=Downloads&color=512BD4)](https://www.nuget.org/packages/Atya.Web.HealthChecks)
![.NET 10.0](https://img.shields.io/badge/.NET_10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
[![Build](https://img.shields.io/github/actions/workflow/status/AtyaLibraries/HealthChecks/ci.yml?branch=development&style=for-the-badge&logo=githubactions&logoColor=white&label=Build)](https://github.com/AtyaLibraries/HealthChecks/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-512BD4?style=for-the-badge)](LICENSE)

## Overview

`Atya.Web.HealthChecks` is a thin opinion layer over `Microsoft.Extensions.Diagnostics.HealthChecks` and ASP.NET Core endpoint routing. It registers the built-in health-check services, maps the standard Atya endpoint set, and provides a stable JSON response writer for operational probes.

The package does not replace ASP.NET Core health checks. Application-specific probes still use the Microsoft `IHealthChecksBuilder` APIs.

## Installation

```bash
dotnet add package Atya.Web.HealthChecks
```

## Quick Start

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

## Endpoint Contract

`MapAtyaWebHealthChecks()` maps three endpoints by default:

| Endpoint | Predicate | Purpose |
| --- | --- | --- |
| `/health` | all registered checks | Complete health report. |
| `/health/live` | checks tagged `AtyaHealthCheckTags.Live` (`"live"`) | Liveness probe. |
| `/health/ready` | checks tagged `AtyaHealthCheckTags.Ready` (`"ready"`) | Readiness probe. |

Responses use Atya's JSON writer by default and include the overall status, total duration, and per-check entries with status, duration, description, tags, exception message, and data.

## Customization

```csharp
app.MapAtyaWebHealthChecks(options =>
{
    options.HealthPath = "/status";
    options.LivePath = "/status/live";
    options.ReadyPath = "/status/ready";
    options.AllowCachingResponses = false;
    options.UseJsonResponse = true;
});
```

`LivePredicate` and `ReadyPredicate` can be replaced when an application uses different tag conventions.

## Compatibility

Targets `net10.0`.

## Testing

```bash
dotnet test
```

## License

Released under the MIT license. See [LICENSE](LICENSE) for details.
