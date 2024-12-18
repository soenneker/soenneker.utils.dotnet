using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Soenneker.Facts.Local;
using Soenneker.Tests.FixturedUnit;
using Soenneker.Utils.Dotnet.Abstract;
using Xunit;

namespace Soenneker.Utils.Dotnet.Tests.Utils;

[Collection("Collection")]
public class DotnetUtilTests : FixturedUnitTest
{
    private readonly IDotnetUtil _util;

    public DotnetUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IDotnetUtil>();
    }

    [LocalFact]
    public async ValueTask Build()
    {
        string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        string proj = Path.Combine(path!, "..", "..", "..", "..", "src");

        bool result = await _util.Build(proj);

        result.Should().BeTrue();
    }

    [LocalFact]
    public async ValueTask UpdatePackages()
    {
        bool result = await _util.UpdatePackages("", cancellationToken: TestContext.Current.CancellationToken);

        result.Should().BeTrue();
    }

    [LocalFact]
    public async ValueTask Test()
    {
        bool result = await _util.Test("", cancellationToken: TestContext.Current.CancellationToken);

        result.Should().BeFalse();
    }
}