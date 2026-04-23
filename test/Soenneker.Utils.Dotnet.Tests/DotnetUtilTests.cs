using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeAssertions;
using Soenneker.Tests.Attributes.Local;
using Soenneker.Tests.HostedUnit;
using Soenneker.Utils.Dotnet.Abstract;

namespace Soenneker.Utils.Dotnet.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class DotnetUtilTests : HostedUnitTest
{
    private readonly IDotnetUtil _util;

    public DotnetUtilTests(Host host) : base(host)
    {
        _util = Resolve<IDotnetUtil>();
    }

    [Test]
    public void Default()
    {
    }

    [Skip("Manual")]
    [Test]
    public async ValueTask Build()
    {
        string? path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly()
                                                               .Location);

        string proj = System.IO.Path.Combine(path!, "..", "..", "..", "..", "src");

        bool result = await _util.Build(proj);

        result.Should()
              .BeTrue();
    }

    [LocalOnly]
    [Test]
    public async ValueTask GetDependencySetsLocal()
    {
        (List<KeyValuePair<string, string>> Direct, HashSet<string> Transitive) result = await _util.GetDependencySetsLocal("C:\\git\\Soenneker\\Utils\\soenneker.utils.process\\src\\Soenneker.Utils.Process\\Soenneker.Utils.Process.csproj", System.Threading.CancellationToken.None);

        result.Should()
              .NotBeNull();
    }

    [LocalOnly]
    //[Skip("Manual")]
    [Test]
    public async ValueTask UpdatePackages()
    {
        bool result = await _util.UpdatePackages("C:\\git\\Soenneker\\Utils\\soenneker.utils.process\\src\\Soenneker.Utils.Process\\Soenneker.Utils.Process.csproj", cancellationToken: System.Threading.CancellationToken.None);

        result.Should()
              .BeTrue();
    }

    //[Skip("Manual")]
    [LocalOnly]
    [Test]
    public async ValueTask Test()
    {
        bool result = await _util.Test("", cancellationToken: System.Threading.CancellationToken.None);

        result.Should()
              .BeFalse();
    }

    [LocalOnly]
    [Test]
    public async ValueTask GetPackages()
    {
        List<KeyValuePair<string, string>> result = await _util.ListPackages("C:\\git\\Soenneker\\Utils\\soenneker.utils.process\\src\\Soenneker.Utils.Process\\Soenneker.Utils.Process.csproj", transitive: true, cancellationToken: System.Threading.CancellationToken.None);

        result.Should()
              .NotBeNullOrEmpty();
    }
}


