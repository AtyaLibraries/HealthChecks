// <copyright file="EndpointRouteBuilderExtensions.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>

using Atya.Foundation.Guards;
using Atya.Web.HealthChecks.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Atya.Web.HealthChecks.Extensions;

/// <summary>
/// Endpoint route builder extensions for Atya health-check endpoints.
/// </summary>
public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the standard Atya health endpoints: all checks, liveness checks, and readiness checks.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="configure">An optional callback for customizing endpoint options.</param>
    /// <returns>A convention builder that applies conventions to all mapped health-check endpoints.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="endpoints"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">A configured path is empty, does not start with <c>/</c>, or duplicates another configured path.</exception>
    public static AtyaHealthChecksEndpointConventionBuilder MapAtyaWebHealthChecks(
        this IEndpointRouteBuilder endpoints,
        Action<AtyaHealthChecksEndpointOptions>? configure = null)
    {
        Guard.AgainstNull(endpoints);

        var options = new AtyaHealthChecksEndpointOptions();
        configure?.Invoke(options);
        options.Validate();

        HealthCheckOptions allOptions = CreateHealthCheckOptions(options, _ => true);
        HealthCheckOptions liveOptions = CreateHealthCheckOptions(options, options.LivePredicate);
        HealthCheckOptions readyOptions = CreateHealthCheckOptions(options, options.ReadyPredicate);

        IEndpointConventionBuilder[] builders =
        [
            endpoints.MapHealthChecks(options.HealthPath, allOptions),
            endpoints.MapHealthChecks(options.LivePath, liveOptions),
            endpoints.MapHealthChecks(options.ReadyPath, readyOptions),
        ];

        return new AtyaHealthChecksEndpointConventionBuilder(builders);
    }

    private static HealthCheckOptions CreateHealthCheckOptions(
        AtyaHealthChecksEndpointOptions options,
        Func<HealthCheckRegistration, bool> predicate)
    {
        var healthCheckOptions = new HealthCheckOptions
        {
            AllowCachingResponses = options.AllowCachingResponses,
            Predicate = predicate,
        };

        if (options.UseJsonResponse)
        {
            healthCheckOptions.ResponseWriter = AtyaHealthCheckResponseWriter.WriteJsonAsync;
        }

        return healthCheckOptions;
    }
}
