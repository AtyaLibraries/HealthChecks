// <copyright file="AtyaHealthChecksEndpointOptionsTests.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>

using Atya.Web.HealthChecks.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Atya.Web.HealthChecks.UnitTests;

public sealed class AtyaHealthChecksEndpointOptionsTests
{
    [Fact]
    public void Defaults_Should_Use_Standard_Endpoint_Paths_And_Json_Response()
    {
        var options = new AtyaHealthChecksEndpointOptions();

        options.HealthPath.Should().Be("/health");
        options.LivePath.Should().Be("/health/live");
        options.ReadyPath.Should().Be("/health/ready");
        options.UseJsonResponse.Should().BeTrue();
        options.AllowCachingResponses.Should().BeFalse();
    }

    [Theory]
    [InlineData("health")]
    [InlineData("")]
    [InlineData("   ")]
    public void Path_Setters_Should_Reject_Invalid_Paths(string path)
    {
        var options = new AtyaHealthChecksEndpointOptions();

        Action act = () => options.HealthPath = path;

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Path_Setters_Should_Trim_Valid_Paths()
    {
        var options = new AtyaHealthChecksEndpointOptions
        {
            HealthPath = " /status ",
        };

        options.HealthPath.Should().Be("/status");
    }

    [Fact]
    public void Default_LivePredicate_Should_Select_Live_Tag()
    {
        var options = new AtyaHealthChecksEndpointOptions();

        options.LivePredicate(CreateRegistration(AtyaHealthCheckTags.Live)).Should().BeTrue();
        options.LivePredicate(CreateRegistration(AtyaHealthCheckTags.Ready)).Should().BeFalse();
    }

    [Fact]
    public void Default_ReadyPredicate_Should_Select_Ready_Tag()
    {
        var options = new AtyaHealthChecksEndpointOptions();

        options.ReadyPredicate(CreateRegistration(AtyaHealthCheckTags.Ready)).Should().BeTrue();
        options.ReadyPredicate(CreateRegistration(AtyaHealthCheckTags.Live)).Should().BeFalse();
    }

    [Fact]
    public void Predicate_Setters_Should_Reject_Null()
    {
        var options = new AtyaHealthChecksEndpointOptions();

        Action liveAct = () => options.LivePredicate = null!;
        Action readyAct = () => options.ReadyPredicate = null!;

        liveAct.Should().Throw<ArgumentNullException>();
        readyAct.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Predicate_Setters_Should_Store_Custom_Predicates()
    {
        var options = new AtyaHealthChecksEndpointOptions
        {
            LivePredicate = _ => true,
            ReadyPredicate = _ => false,
        };
        HealthCheckRegistration registration = CreateRegistration();

        options.LivePredicate(registration).Should().BeTrue();
        options.ReadyPredicate(registration).Should().BeFalse();
    }

    private static HealthCheckRegistration CreateRegistration(params string[] tags)
    {
        return new HealthCheckRegistration(
            "check",
            _ => new StubHealthCheck(),
            HealthStatus.Unhealthy,
            tags);
    }

    private sealed class StubHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}
