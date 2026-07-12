// <copyright file="ServiceCollectionExtensionsTests.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>

using Atya.Web.HealthChecks.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Atya.Web.HealthChecks.UnitTests;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAtyaWebHealthChecks_Should_Throw_When_Services_Is_Null()
    {
        var act = () => ServiceCollectionExtensions.AddAtyaWebHealthChecks(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddAtyaWebHealthChecks_Should_Register_HealthCheckService()
    {
        using var provider = new ServiceCollection()
            .AddAtyaWebHealthChecks()
            .Services
            .BuildServiceProvider();

        provider.GetRequiredService<HealthCheckService>().Should().NotBeNull();
    }

    [Fact]
    public void AddAtyaWebHealthChecks_Should_Apply_Builder_Configuration()
    {
        using var provider = new ServiceCollection()
            .AddAtyaWebHealthChecks(builder =>
                builder.AddCheck("ready-check", () => HealthCheckResult.Healthy(), tags: [AtyaHealthCheckTags.Ready]))
            .Services
            .BuildServiceProvider();

        var service = provider.GetRequiredService<HealthCheckService>();

        service.Should().NotBeNull();
    }

    [Fact]
    public void AddAtyaWebHealthChecks_Should_Support_Compile_Shapes()
    {
        using var defaultProvider = new ServiceCollection()
            .AddAtyaWebHealthChecks()
            .Services
            .BuildServiceProvider();
        using var configuredProvider = new ServiceCollection()
            .AddAtyaWebHealthChecks(h => { })
            .Services
            .BuildServiceProvider();

        defaultProvider.GetRequiredService<HealthCheckService>().Should().NotBeNull();
        configuredProvider.GetRequiredService<HealthCheckService>().Should().NotBeNull();
    }
}
