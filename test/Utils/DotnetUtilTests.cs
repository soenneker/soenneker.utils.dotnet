using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Soenneker.Facts.Local;
using Soenneker.Tests.FixturedUnit;
using Soenneker.Utils.Dotnet.Abstract;
using Xunit;
using Xunit.Abstractions;

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
    public async Task Download_should_download()
    {
        string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        string proj = Path.Combine(path!, "..", "..", "..", "..", "src");

        bool result = await _util.Build(proj);

        result.Should().BeTrue();
    }
}