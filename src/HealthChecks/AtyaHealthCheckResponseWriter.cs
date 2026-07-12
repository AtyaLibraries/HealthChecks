// <copyright file="AtyaHealthCheckResponseWriter.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>

using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Atya.Web.HealthChecks;

/// <summary>
/// Writes Atya's JSON health-check response payload.
/// </summary>
public static class AtyaHealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions s_jsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Writes a JSON health report response to the HTTP context.
    /// </summary>
    /// <param name="httpContext">The HTTP context receiving the response.</param>
    /// <param name="report">The health report to write.</param>
    /// <returns>A task that completes when the response has been written.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="httpContext"/> or <paramref name="report"/> is <see langword="null"/>.</exception>
    public static Task WriteJsonAsync(HttpContext httpContext, HealthReport report)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(report);

        httpContext.Response.ContentType = "application/json; charset=utf-8";

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
