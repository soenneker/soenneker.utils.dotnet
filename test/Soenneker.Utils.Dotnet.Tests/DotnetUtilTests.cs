using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Soenneker.Facts.Manual;
using Soenneker.Tests.FixturedUnit;
using Soenneker.Utils.Dotnet.Abstract;
using Xunit;

namespace Soenneker.Utils.Dotnet.Tests;

[Collection("Collection")]
public class DotnetUtilTests : FixturedUnitTest
{
    private readonly IDotnetUtil _util;

    public DotnetUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IDotnetUtil>();
    }

    [Fact]
    public void Default() { }

    [ManualFact]
    public async ValueTask Build()
    {
        string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        string proj = Path.Combine(path!, "..", "..", "..", "..", "src");

        bool result = await _util.Build(proj);

        result.Should().BeTrue();
    }

    [ManualFact]
    public async ValueTask UpdatePackages()
    {
        bool result = await _util.UpdatePackages("", cancellationToken: CancellationToken);

        result.Should().BeTrue();
    }

    [ManualFact]
    public async ValueTask Test()
    {
        bool result = await _util.Test("", cancellationToken: CancellationToken);

        result.Should().BeFalse();
    }

    [ManualFact]
    public async ValueTask GetPackages()
    {
        var result = await _util.ListPackages("", transitive: true, cancellationToken: CancellationToken);
    }
}