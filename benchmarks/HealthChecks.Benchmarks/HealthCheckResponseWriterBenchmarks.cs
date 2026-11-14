// <copyright file="HealthCheckResponseWriterBenchmarks.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>

using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Atya.Web.HealthChecks.Benchmarks;

/// <summary>
/// Benchmarks the public Atya JSON health-check response writer.
/// </summary>
[MemoryDiagnoser]
public class HealthCheckResponseWriterBenchmarks
{
    private readonly HealthReport _report = new(
        new Dictionary<string, HealthReportEntry>
        {
            ["self"] = new(
                HealthStatus.Healthy,
                "Process is running.",
                TimeSpan.FromMilliseconds(1),
                null,
                new Dictionary<string, object>(),
                [AtyaHealthCheckTags.Live]),
            ["database"] = new(
                HealthStatus.Healthy,
                "Database dependency is reachable.",
                TimeSpan.FromMilliseconds(8),
                null,
                new Dictionary<string, object> { ["region"] = "eu" },
                [AtyaHealthCheckTags.Ready]),
        },
        TimeSpan.FromMilliseconds(9));

    private DefaultHttpContext _context = new();

    /// <summary>
    /// Resets the response target before each benchmark operation.
    /// </summary>
    [IterationSetup]
    public void IterationSetup()
    {
        _context = new DefaultHttpContext
        {
            Response =
            {
                Body = Stream.Null,
            },
        };
    }

    /// <summary>
    /// Writes the default minimal Atya JSON health-check response payload.
    /// </summary>
    /// <returns>A task that completes when the payload is written.</returns>
    [Benchmark(Baseline = true)]
    public Task WriteJsonResponseAsync()
    {
        return AtyaHealthCheckResponseWriter.WriteJsonAsync(_context, _report);
    }

    /// <summary>
    /// Writes the opt-in detailed Atya JSON health-check response payload.
    /// </summary>
    /// <returns>A task that completes when the payload is written.</returns>
    [Benchmark]
    public Task WriteDetailedJsonResponseAsync()
    {
        return AtyaHealthCheckResponseWriter.WriteDetailedJsonAsync(_context, _report);
    }
}
