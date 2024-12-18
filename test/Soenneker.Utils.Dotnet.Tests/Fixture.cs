using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Threading.Tasks;
using Soenneker.Fixtures.Unit;
using Soenneker.Utils.Dotnet.Registrars;
using Soenneker.Utils.Test;

namespace Soenneker.Utils.Dotnet.Tests;

public class Fixture : UnitFixture
{
    public override async ValueTask InitializeAsync()
    {
        SetupIoC(Services);

        await base.InitializeAsync();
    }

    private static void SetupIoC(IServiceCollection services)
    {
        services.AddLogging(builder => { builder.AddSerilog(dispose: true); });

        IConfiguration config = TestUtil.BuildConfig();
        services.AddSingleton(config);

        services.AddDotnetUtilAsSingleton();
    }
}