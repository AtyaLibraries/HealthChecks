// <copyright file="AtyaHealthChecksEndpointConventionBuilder.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>

using Atya.Foundation.Guards;
using Microsoft.AspNetCore.Builder;

namespace Atya.Web.HealthChecks;

/// <summary>
/// Applies endpoint conventions to every endpoint mapped by <c>MapAtyaWebHealthChecks</c>.
/// </summary>
public sealed class AtyaHealthChecksEndpointConventionBuilder : IEndpointConventionBuilder
{
    private readonly IReadOnlyCollection<IEndpointConventionBuilder> _builders;

    internal AtyaHealthChecksEndpointConventionBuilder(IReadOnlyCollection<IEndpointConventionBuilder> builders)
    {
        _builders = Guard.AgainstNullOrEmpty(builders);
    }

    /// <inheritdoc />
    public void Add(Action<EndpointBuilder> convention)
    {
        Guard.AgainstNull(convention);

        foreach (IEndpointConventionBuilder builder in _builders)
        {
            builder.Add(convention);
        }
    }

    /// <inheritdoc />
    public void Finally(Action<EndpointBuilder> finallyConvention)
    {
        Guard.AgainstNull(finallyConvention);

        foreach (IEndpointConventionBuilder builder in _builders)
        {
            builder.Finally(finallyConvention);
        }
    }
}
