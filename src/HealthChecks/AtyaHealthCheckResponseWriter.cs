// <copyright file="AtyaHealthCheckResponseWriter.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>

using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Atya.Web.HealthChecks;

/// <summary>
/// Writes Atya's JSON health-check response payloads.
/// </summary>
public static class AtyaHealthCheckResponseWriter
{
    private const string JsonContentType = "application/json; charset=utf-8";

    private static readonly JsonSerializerOptions s_jsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Writes a minimal JSON health response containing only the aggregate health status.
    /// </summary>
    /// <remarks>
    /// This is the default response writer. It deliberately omits check names, durations, descriptions,
    /// tags, exception messages, and check data so that an unauthenticated health endpoint cannot disclose
    /// service topology or internal diagnostics. Use <see cref="WriteDetailedJsonAsync"/> only on endpoints
    /// that are protected by authorization or are not reachable from untrusted networks.
    /// </remarks>
    /// <param name="httpContext">The HTTP context receiving the response.</param>
    /// <param name="report">The health report to write.</param>
    /// <returns>A task that completes when the response has been written.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="httpContext"/> or <paramref name="report"/> is <see langword="null"/>.</exception>
    public static Task WriteJsonAsync(HttpContext httpContext, HealthReport report)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(report);

        httpContext.Response.ContentType = JsonContentType;

        var payload = new MinimalHealthReportPayload(report.Status.ToString());

        return JsonSerializer.SerializeAsync(httpContext.Response.Body, payload, s_jsonOptions);
    }

    /// <summary>
    /// Writes a detailed JSON health report response, including per-check diagnostics.
    /// </summary>
    /// <remarks>
    /// The payload includes each check's name, status, duration, description, tags, exception message, and
    /// data. Those values are supplied by application health checks and routinely contain internal detail
    /// such as dependency host names or failure text. Expose this writer only behind authorization, or on a
    /// management port that untrusted callers cannot reach. Prefer <see cref="WriteJsonAsync"/> otherwise.
    /// </remarks>
    /// <param name="httpContext">The HTTP context receiving the response.</param>
    /// <param name="report">The health report to write.</param>
    /// <returns>A task that completes when the response has been written.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="httpContext"/> or <paramref name="report"/> is <see langword="null"/>.</exception>
    public static Task WriteDetailedJsonAsync(HttpContext httpContext, HealthReport report)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(report);

        httpContext.Response.ContentType = JsonContentType;

        var payload = new HealthReportPayload(
            report.Status.ToString(),
            report.TotalDuration,
            report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new HealthReportEntryPayload(
                    entry.Value.Status.ToString(),
                    entry.Value.Duration,
                    entry.Value.Description,
                    entry.Value.Tags.Order(StringComparer.Ordinal).ToArray(),
                    entry.Value.Exception?.Message,
                    entry.Value.Data)));

        return JsonSerializer.SerializeAsync(httpContext.Response.Body, payload, s_jsonOptions);
    }

    private sealed class MinimalHealthReportPayload
    {
        public MinimalHealthReportPayload(string status)
        {
            Status = status;
        }

        public string Status { get; }
    }

    private sealed class HealthReportPayload
    {
        public HealthReportPayload(
            string status,
            TimeSpan totalDuration,
            IReadOnlyDictionary<string, HealthReportEntryPayload> entries)
        {
            Status = status;
            TotalDuration = totalDuration;
            Entries = entries;
        }

        public string Status { get; }

        public TimeSpan TotalDuration { get; }

        public IReadOnlyDictionary<string, HealthReportEntryPayload> Entries { get; }
    }

    private sealed class HealthReportEntryPayload
    {
        public HealthReportEntryPayload(
            string status,
            TimeSpan duration,
            string? description,
            IReadOnlyCollection<string> tags,
            string? exception,
            IReadOnlyDictionary<string, object> data)
        {
            Status = status;
            Duration = duration;
            Description = description;
            Tags = tags;
            Exception = exception;
            Data = data;
        }

        public string Status { get; }

        public TimeSpan Duration { get; }

        public string? Description { get; }

        public IReadOnlyCollection<string> Tags { get; }

        public string? Exception { get; }

        public IReadOnlyDictionary<string, object> Data { get; }
    }
}
