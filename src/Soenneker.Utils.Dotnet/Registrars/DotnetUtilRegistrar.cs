using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Utils.Directory.Registrars;
using Soenneker.Utils.Dotnet.Abstract;
using Soenneker.Utils.File.Registrars;
using Soenneker.Utils.Process.Registrars;

namespace Soenneker.Utils.Dotnet.Registrars;

/// <summary>
/// A utility library for the dotnet executable
/// </summary>
public static class DotnetUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="IDotnetUtil"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddDotnetUtilAsSingleton(this IServiceCollection services)
    {
        services.AddProcessUtilAsSingleton()
                .AddFileUtilAsSingleton()
                .AddDirectoryUtilAsSingleton()
                .TryAddSingleton<IDotnetUtil, DotnetUtil>();
        return services;
    }

    /// <summary>
    /// Adds <see cref="IDotnetUtil"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddDotnetUtilAsScoped(this IServiceCollection services)
    {
        services.AddProcessUtilAsScoped()
                .AddFileUtilAsScoped()
                .AddDirectoryUtilAsScoped()
                .TryAddScoped<IDotnetUtil, DotnetUtil>();
        return services;
    }
}