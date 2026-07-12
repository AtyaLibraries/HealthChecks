namespace Atya.Web.HealthChecks.UnitTests;

public sealed class TemplateWiringTests
{
    [Fact]
    public void LibraryAssembly_Can_Be_Loaded()
    {
        var assembly = typeof(HealthChecksMarker).Assembly;

        assembly.Should().NotBeNull();
        assembly.GetName().Name.Should().Be("Atya.Web.HealthChecks");
    }

    [Fact]
    public void Marker_Exposes_Package_Id()
    {
        HealthChecksMarker.PackageId.Should().Be("Atya.Web.HealthChecks");
    }
}
