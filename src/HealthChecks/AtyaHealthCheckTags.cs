// <copyright file="AtyaHealthCheckTags.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>

namespace Atya.Web.HealthChecks;

/// <summary>
/// Standard health-check tags used by Atya endpoint predicates.
/// </summary>
public static class AtyaHealthCheckTags
{
    /// <summary>
    /// Tags a check as part of the liveness endpoint.
    /// </summary>
    public const string Live = "live";

    /// <summary>
    /// Tags a check as part of the readiness endpoint.
    /// </summary>
    public const string Ready = "ready";
}
