using Soenneker.Extensions.String;
using Soenneker.Utils.PooledStringBuilders;

namespace Soenneker.Utils.Dotnet;

internal static class ArgumentUtil
{
    internal static string Run(string path, string? framework, string? configuration, string? verbosity, bool? build)
    {
        using var sb = new PooledStringBuilder();

        sb.Append('"'); sb.Append(path); sb.Append('"');

        if (framework != null)
        {
            sb.Append(" -f ");
            sb.Append(framework);
        }

        if (configuration != null)
        {
            sb.Append(" -c ");
            sb.Append(configuration);
        }

        if (verbosity != null)
        {
            sb.Append(" -v ");
            sb.Append(verbosity);
        }

        if (build.HasValue && !build.Value)
            sb.Append(" --no-build");

        return sb.ToString();
    }

    internal static string Restore(string path, string? verbosity)
    {
        using var sb = new PooledStringBuilder();

        sb.Append('"'); sb.Append(path); sb.Append('"');

        if (verbosity != null)
        {
            sb.Append(" -v ");
            sb.Append(verbosity);
        }

        return sb.ToString();
    }

    internal static string Build(string path, string? configuration, bool? restore, string? verbosity)
    {
        using var sb = new PooledStringBuilder();

        sb.Append('"'); sb.Append(path); sb.Append('"');

        if (configuration != null)
        {
            sb.Append(" -c ");
            sb.Append(configuration);
        }

        if (restore.HasValue && !restore.Value)
            sb.Append(" --no-restore");

        if (verbosity != null)
        {
            sb.Append(" -v ");
            sb.Append(verbosity);
        }

        return sb.ToString();
    }

    internal static string Test(string path, bool? restore, string? verbosity)
    {
        using var sb = new PooledStringBuilder();

        sb.Append('"'); sb.Append(path); sb.Append('"');

        if (restore.HasValue && !restore.Value)
            sb.Append(" --no-restore");

        if (verbosity != null)
        {
            sb.Append(" -v ");
            sb.Append(verbosity);
        }

        return sb.ToString();
    }

    internal static string Pack(string path, string version, string? configuration, bool? build, bool? restore, string? output, string? verbosity)
    {
        using var sb = new PooledStringBuilder();

        sb.Append('"'); sb.Append(path); sb.Append('"');

        sb.Append(" -p:PackageVersion=");
        sb.Append(version);

        if (configuration != null)
        {
            sb.Append(" -c ");
            sb.Append(configuration);
        }

        if (build.HasValue && !build.Value)
            sb.Append(" --no-build");

        if (restore.HasValue && !restore.Value)
            sb.Append(" --no-restore");

        if (output != null)
        {
            sb.Append(" --output \"");
            sb.Append(output);
            sb.Append('"');
        }

        if (verbosity != null)
        {
            sb.Append(" -v ");
            sb.Append(verbosity);
        }

        return sb.ToString();
    }

    internal static string AddPackage(string path, string packageId, string? version, bool? restore)
    {
        using var sb = new PooledStringBuilder();

        sb.Append('"'); sb.Append(path); sb.Append('"');
        sb.Append(" package \"");
        sb.Append(packageId);
        sb.Append('"');

        if (!version.IsNullOrEmpty())
        {
            sb.Append(" --version ");
            sb.Append(version);
        }

        if (restore.HasValue && !restore.Value)
            sb.Append(" --no-restore");

        return sb.ToString();
    }

    internal static string RemovePackage(string path, string packageId, bool? restore)
    {
        using var sb = new PooledStringBuilder();

        sb.Append('"'); sb.Append(path); sb.Append('"');
        sb.Append(" package \"");
        sb.Append(packageId);
        sb.Append('"');

        if (restore.HasValue && !restore.Value)
            sb.Append(" --no-restore");

        return sb.ToString();
    }

    internal static string Clean(string path, string? configuration, string? verbosity)
    {
        using var sb = new PooledStringBuilder();

        sb.Append('"'); sb.Append(path); sb.Append('"');

        if (!configuration.IsNullOrEmpty())
        {
            sb.Append(" --configuration ");
            sb.Append(configuration);
        }

        if (!verbosity.IsNullOrEmpty())
        {
            sb.Append(" -v ");
            sb.Append(verbosity);
        }

        return sb.ToString();
    }

    internal static string ListPackages(string path, bool outdated, bool transitive, bool includePrerelease, bool vulnerable, bool deprecated,
        string? verbosity)
    {
        using var sb = new PooledStringBuilder();

        sb.Append('"'); sb.Append(path); sb.Append('"');
        sb.Append(" package");

        if (deprecated)
            sb.Append(" --deprecated");

        if (outdated)
            sb.Append(" --outdated");

        if (transitive)
            sb.Append(" --include-transitive");

        if (includePrerelease)
            sb.Append(" --include-prerelease");

        if (vulnerable)
            sb.Append(" --vulnerable");

        if (!verbosity.IsNullOrEmpty())
        {
            sb.Append(" -v ");
            sb.Append(verbosity);
        }

        return sb.ToString();
    }
}
