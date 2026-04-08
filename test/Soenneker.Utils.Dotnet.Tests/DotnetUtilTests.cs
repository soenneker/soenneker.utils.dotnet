using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using AwesomeAssertions;
using Soenneker.Facts.Local;
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
    public void Default()
    {
    }

    [ManualFact]
    public async ValueTask Build()
    {
        string? path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly()
                                                               .Location);

        string proj = System.IO.Path.Combine(path!, "..", "..", "..", "..", "src");

        bool result = await _util.Build(proj);

        result.Should()
              .BeTrue();
    }

    [LocalFact]
    public async ValueTask GetDependencySetsLocal()
    {
        (List<KeyValuePair<string, string>> Direct, HashSet<string> Transitive) result = await _util.GetDependencySetsLocal("C:\\git\\Soenneker\\Utils\\soenneker.utils.process\\src\\Soenneker.Utils.Process\\Soenneker.Utils.Process.csproj", CancellationToken);

        result.Should()
              .NotBeNull();
    }

    [LocalFact]
    //[ManualFact]
    public async ValueTask UpdatePackages()
    {
        bool result = await _util.UpdatePackages("C:\\git\\Soenneker\\Utils\\soenneker.utils.process\\src\\Soenneker.Utils.Process\\Soenneker.Utils.Process.csproj", cancellationToken: CancellationToken);

        result.Should()
              .BeTrue();
    }

    //[ManualFact]
    [LocalFact]
    public async ValueTask Test()
    {
        bool result = await _util.Test("", cancellationToken: CancellationToken);

        result.Should()
              .BeFalse();
    }

    [LocalFact]
    public async ValueTask GetPackages()
    {
        List<KeyValuePair<string, string>> result = await _util.ListPackages("C:\\git\\Soenneker\\Utils\\soenneker.utils.process\\src\\Soenneker.Utils.Process\\Soenneker.Utils.Process.csproj", transitive: true, cancellationToken: CancellationToken);

        result.Should()
              .NotBeNullOrEmpty();
    }
}