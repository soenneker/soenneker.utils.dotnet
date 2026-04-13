using System.Collections.Generic;
using Soenneker.Extensions.String;
using Soenneker.Utils.PooledStringBuilders;

namespace Soenneker.Utils.Dotnet;

internal static class ArgumentUtil
{
    internal static string Run(string path, string? framework, string? configuration, string? verbosity, bool? build, string? urls)
    {
        using var sb = new PooledStringBuilder();

        sb.Append("run \"");
        sb.Append(path);
        sb.Append('"');

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

        if (urls.HasContent())
        {
            sb.Append(" --urls ");
            sb.Append(urls);
        }

        return sb.ToString();
    }

    internal static string Restore(string path, string? verbosity)
    {
        using var sb = new PooledStringBuilder();

        sb.Append("restore \"");
        sb.Append(path);
        sb.Append('"');

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

        sb.Append("build \"");
        sb.Append(path);
        sb.Append('"');

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

        sb.Append("test \"");
        sb.Append(path);
        sb.Append('"');

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

        sb.Append("pack \"");
        sb.Append(path);
        sb.Append('"');

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

        sb.Append("add \"");
        sb.Append(path);
        sb.Append('"');

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

        sb.Append("remove \"");
        sb.Append(path);
        sb.Append('"');

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

        sb.Append("clean \"");
        sb.Append(path);
        sb.Append('"');

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

    internal static string ListPackages(string path, bool includeTransitive, bool outdated, bool includePrerelease, bool vulnerable, bool deprecated,
        bool noRestore, string? verbosity)
    {
        var parts = new List<string>(12)
        {
            "list",
            $"\"{path}\"",
            "package",
            "--format",
            "json",
            "--output-version",
            "1"
        };

        if (verbosity.HasContent())
        {
            parts.Add("--verbosity");
            parts.Add(verbosity!);
        }

        if (includeTransitive)
            parts.Add("--include-transitive");

        if (outdated)
            parts.Add("--outdated");

        if (includePrerelease)
            parts.Add("--include-prerelease");

        if (vulnerable)
            parts.Add("--vulnerable");

        if (deprecated)
            parts.Add("--deprecated");

        if (noRestore)
            parts.Add("--no-restore");

        return string.Join(' ', parts);
    }
}