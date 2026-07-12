// <copyright file="ServiceCollectionExtensions.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>

using Atya.Foundation.Guards;
using Microsoft.Extensions.DependencyInjection;

namespace Atya.Web.HealthChecks.Extensions;

/// <summary>
/// Service collection extensions for Atya health checks.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers ASP.NET Core health checks and applies optional health-check builder configuration.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="configure">An optional callback for adding application health checks.</param>
    /// <returns>The Microsoft health-check builder.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IHealthChecksBuilder AddAtyaWebHealthChecks(
        this IServiceCollection services,
        Action<IHealthChecksBuilder>? configure = null)
    {
        Guard.AgainstNull(services);

        services.AddLogging();
        services.AddOptions();

        IHealthChecksBuilder builder = services.AddHealthChecks();
        configure?.Invoke(builder);

        return builder;
    }
}
