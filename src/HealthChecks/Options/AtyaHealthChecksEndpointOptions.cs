// <copyright file="AtyaHealthChecksEndpointOptions.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Atya.Web.HealthChecks.Options;

/// <summary>
/// Configures the standard Atya health-check endpoints.
/// </summary>
public sealed class AtyaHealthChecksEndpointOptions
{
    private string _healthPath = "/health";
    private string _livePath = "/health/live";
    private string _readyPath = "/health/ready";

    private Func<HealthCheckRegistration, bool> _livePredicate =
        registration => registration.Tags.Contains(AtyaHealthCheckTags.Live, StringComparer.OrdinalIgnoreCase);

    private Func<HealthCheckRegistration, bool> _readyPredicate =
        registration => registration.Tags.Contains(AtyaHealthCheckTags.Ready, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the endpoint path that runs all registered health checks.
    /// </summary>
    public string HealthPath
    {
        get => _healthPath;
        set => _healthPath = NormalizePath(value, nameof(value));
    }

    /// <summary>
    /// Gets or sets the endpoint path that runs checks selected by <see cref="LivePredicate"/>.
    /// </summary>
    public string LivePath
    {
        get => _livePath;
        set => _livePath = NormalizePath(value, nameof(value));
    }

    /// <summary>
    /// Gets or sets the endpoint path that runs checks selected by <see cref="ReadyPredicate"/>.
    /// </summary>
    public string ReadyPath
    {
        get => _readyPath;
        set => _readyPath = NormalizePath(value, nameof(value));
    }

    /// <summary>
    /// Gets or sets a value indicating whether endpoint responses should use Atya's JSON health report writer.
    /// </summary>
    public bool UseJsonResponse { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether health-check responses may be cached by intermediaries.
    /// </summary>
    public bool AllowCachingResponses { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether endpoint responses include per-check diagnostics.
    /// Defaults to <see langword="false"/>, which returns the aggregate health status only.
    /// </summary>
    /// <remarks>
    /// Enabling this opts in to the detailed payload written by
    /// <see cref="AtyaHealthCheckResponseWriter.WriteDetailedJsonAsync"/>, which discloses check names,
    /// durations, descriptions, tags, exception messages, and check data. Health endpoints are commonly
    /// unauthenticated, so enable this only when the endpoints are protected by authorization or are
    /// unreachable from untrusted networks. Has no effect when <see cref="UseJsonResponse"/> is
    /// <see langword="false"/>.
    /// </remarks>
    public bool IncludeDetailedDiagnostics { get; set; }

    /// <summary>
    /// Gets or sets the predicate used by the liveness endpoint.
    /// </summary>
    /// <exception cref="ArgumentNullException">The assigned value is <see langword="null"/>.</exception>
    public Func<HealthCheckRegistration, bool> LivePredicate
    {
        get => _livePredicate;
        set => _livePredicate = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Gets or sets the predicate used by the readiness endpoint.
    /// </summary>
    /// <exception cref="ArgumentNullException">The assigned value is <see langword="null"/>.</exception>
    public Func<HealthCheckRegistration, bool> ReadyPredicate
    {
        get => _readyPredicate;
        set => _readyPredicate = value ?? throw new ArgumentNullException(nameof(value));
    }

    internal void Validate()
    {
        string[] paths = [HealthPath, LivePath, ReadyPath];
        if (paths.Distinct(StringComparer.OrdinalIgnoreCase).Count() != paths.Length)
        {
            throw new ArgumentException("Health check endpoint paths must be distinct.");
        }
    }

    private static string NormalizePath(string? value, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);

        string path = value.Trim();
        if (!path.StartsWith('/'))
        {
            throw new ArgumentException("Health check endpoint paths must start with '/'.", paramName);
        }

        return path;
    }
}
