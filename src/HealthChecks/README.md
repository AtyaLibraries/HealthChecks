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

### Response shape

By default every endpoint returns the aggregate status only:

```json
{"status":"Healthy"}
```

Health endpoints are usually unauthenticated, so the default deliberately omits check names, durations,
descriptions, tags, exception messages, and check data. The HTTP status code (`200` healthy, `503`
unhealthy) still carries the signal orchestrators and load balancers need.

Per-check diagnostics are opt-in:

```csharp
app.MapAtyaWebHealthChecks(options => options.IncludeDetailedDiagnostics = true);
```

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0110000",
  "entries": {
    "database": {
      "status": "Healthy",
      "duration": "00:00:00.0070000",
      "description": "Database dependency is reachable.",
      "tags": ["ready"],
      "exception": null,
      "data": { "region": "eu" }
    }
  }
}
```

Enable it only where the endpoints are protected by authorization or are unreachable from untrusted
networks — the payload surfaces whatever your health checks put in their descriptions, exceptions, and
data dictionaries.

The two writers are also usable directly as `HealthCheckOptions.ResponseWriter` values:
`AtyaHealthCheckResponseWriter.WriteJsonAsync` (minimal) and
`AtyaHealthCheckResponseWriter.WriteDetailedJsonAsync` (detailed).

Endpoint paths, predicates, response caching, response detail, and JSON response writing can be
customized with `AtyaHealthChecksEndpointOptions`.
