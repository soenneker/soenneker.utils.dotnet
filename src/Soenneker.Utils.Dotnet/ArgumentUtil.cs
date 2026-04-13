using System.Collections.Generic;
using Soenneker.Extensions.String;
using Soenneker.Utils.PooledStringBuilders;

namespace Soenneker.Utils.Dotnet;

internal static class ArgumentUtil
{
    internal static string Run(string path, string? framework, string? configuration, string? verbosity, bool? build, bool? restore, string? urls,
        string? launchProfile, string? environment, IReadOnlyList<string>? applicationArguments)
    {
        var sb = new PooledStringBuilder();

        sb.Append("run --project \"");
        sb.Append(path);
        sb.Append('"');

        if (framework.HasContent())
        {
            sb.Append(" -f ");
            sb.Append(framework);
        }

        if (configuration.HasContent())
        {
            sb.Append(" -c ");
            sb.Append(configuration);
        }

        if (verbosity.HasContent())
        {
            sb.Append(" -v ");
            sb.Append(verbosity);
        }

        if (build.HasValue && !build.Value)
            sb.Append(" --no-build");

        if (restore.HasValue && !restore.Value)
            sb.Append(" --no-restore");

        if (launchProfile.HasContent())
        {
            sb.Append(" --launch-profile \"");
            sb.Append(launchProfile);
            sb.Append('"');
        }

        if (environment.HasContent())
        {
            sb.Append(" --environment \"");
            sb.Append(environment);
            sb.Append('"');
        }

        if (urls.HasContent())
        {
            sb.Append(" --urls \"");
            sb.Append(urls);
            sb.Append('"');
        }

        AppendApplicationArguments(ref sb, applicationArguments);

        return sb.ToStringAndDispose();
    }

    internal static string Restore(string path, string? verbosity, string? runtime, string? packages, IReadOnlyList<string>? sources, string? configFile,
        bool disableParallel)
    {
        var sb = new PooledStringBuilder();

        sb.Append("restore \"");
        sb.Append(path);
        sb.Append('"');

        if (verbosity.HasContent())
        {
            sb.Append(" -v ");
            sb.Append(verbosity);
        }

        if (runtime.HasContent())
        {
            sb.Append(" --runtime ");
            sb.Append(runtime);
        }

        if (packages.HasContent())
        {
            sb.Append(" --packages \"");
            sb.Append(packages);
            sb.Append('"');
        }

        AppendRepeatedOption(ref sb, "--source", sources);

        if (configFile.HasContent())
        {
            sb.Append(" --configfile \"");
            sb.Append(configFile);
            sb.Append('"');
        }

        if (disableParallel)
            sb.Append(" --disable-parallel");

        return sb.ToStringAndDispose();
    }

    internal static string Build(string path, string? configuration, bool? restore, string? verbosity, string? framework, string? runtime, bool? selfContained,
        string? output, IReadOnlyList<KeyValuePair<string, string?>>? properties)
    {
        var sb = new PooledStringBuilder();

        sb.Append("build \"");
        sb.Append(path);
        sb.Append('"');

        if (configuration.HasContent())
        {
            sb.Append(" -c ");
            sb.Append(configuration);
        }

        if (restore.HasValue && !restore.Value)
            sb.Append(" --no-restore");

        if (verbosity.HasContent())
        {
            sb.Append(" -v ");
            sb.Append(verbosity);
        }

        if (framework.HasContent())
        {
            sb.Append(" -f ");
            sb.Append(framework);
        }

        if (runtime.HasContent())
        {
            sb.Append(" -r ");
            sb.Append(runtime);
        }

        if (selfContained.HasValue)
        {
            sb.Append(" --self-contained ");
            sb.Append(selfContained.Value ? "true" : "false");
        }

        if (output.HasContent())
        {
            sb.Append(" -o \"");
            sb.Append(output);
            sb.Append('"');
        }

        AppendProperties(ref sb, properties);

        return sb.ToStringAndDispose();
    }

    internal static string Test(string path, bool? restore, bool? build, string? configuration, string? verbosity, string? framework, string? filter,
        string? logger, string? resultsDirectory)
    {
        using var sb = new PooledStringBuilder();

        sb.Append("test \"");
        sb.Append(path);
        sb.Append('"');

        if (restore.HasValue && !restore.Value)
            sb.Append(" --no-restore");

        if (build.HasValue && !build.Value)
            sb.Append(" --no-build");

        if (configuration.HasContent())
        {
            sb.Append(" -c ");
            sb.Append(configuration);
        }

        if (verbosity.HasContent())
        {
            sb.Append(" -v ");
            sb.Append(verbosity);
        }

        if (framework.HasContent())
        {
            sb.Append(" -f ");
            sb.Append(framework);
        }

        if (filter.HasContent())
        {
            sb.Append(" --filter \"");
            sb.Append(filter);
            sb.Append('"');
        }

        if (logger.HasContent())
        {
            sb.Append(" --logger \"");
            sb.Append(logger);
            sb.Append('"');
        }

        if (resultsDirectory.HasContent())
        {
            sb.Append(" --results-directory \"");
            sb.Append(resultsDirectory);
            sb.Append('"');
        }

        return sb.ToString();
    }

    internal static string Pack(string path, string version, string? configuration, bool? build, bool? restore, string? output, string? verbosity,
        string? framework, string? runtime, bool? includeSymbols, bool? includeSource, bool? serviceable,
        IReadOnlyList<KeyValuePair<string, string?>>? properties)
    {
        var sb = new PooledStringBuilder();

        sb.Append("pack \"");
        sb.Append(path);
        sb.Append('"');

        sb.Append(" -p:PackageVersion=");
        sb.Append(version);

        if (configuration.HasContent())
        {
            sb.Append(" -c ");
            sb.Append(configuration);
        }

        if (build.HasValue && !build.Value)
            sb.Append(" --no-build");

        if (restore.HasValue && !restore.Value)
            sb.Append(" --no-restore");

        if (output.HasContent())
        {
            sb.Append(" --output \"");
            sb.Append(output);
            sb.Append('"');
        }

        if (verbosity.HasContent())
        {
            sb.Append(" -v ");
            sb.Append(verbosity);
        }

        if (framework.HasContent())
        {
            sb.Append(" -f ");
            sb.Append(framework);
        }

        if (runtime.HasContent())
        {
            sb.Append(" -r ");
            sb.Append(runtime);
        }

        if (includeSymbols.HasValue && includeSymbols.Value)
            sb.Append(" --include-symbols");

        if (includeSource.HasValue && includeSource.Value)
            sb.Append(" --include-source");

        if (serviceable.HasValue)
        {
            sb.Append(" -p:Serviceable=");
            sb.Append(serviceable.Value ? "true" : "false");
        }

        AppendProperties(ref sb, properties);

        return sb.ToStringAndDispose();
    }

    internal static string AddPackage(string path, string packageId, string? version, bool? restore, string? framework, string? source, bool prerelease,
        string? packageDirectory, bool interactive)
    {
        using var sb = new PooledStringBuilder();

        sb.Append("package add \"");
        sb.Append(packageId);
        sb.Append('"');

        sb.Append(" --project \"");
        sb.Append(path);
        sb.Append('"');

        if (version.HasContent())
        {
            sb.Append(" --version ");
            sb.Append(version);
        }

        if (framework.HasContent())
        {
            sb.Append(" --framework ");
            sb.Append(framework);
        }

        if (source.HasContent())
        {
            sb.Append(" --source \"");
            sb.Append(source);
            sb.Append('"');
        }

        if (prerelease)
            sb.Append(" --prerelease");

        if (packageDirectory.HasContent())
        {
            sb.Append(" --package-directory \"");
            sb.Append(packageDirectory);
            sb.Append('"');
        }

        if (interactive)
            sb.Append(" --interactive");

        if (restore.HasValue && !restore.Value)
            sb.Append(" --no-restore");

        return sb.ToString();
    }

    internal static string RemovePackage(string path, string packageId, bool? restore)
    {
        using var sb = new PooledStringBuilder();

        sb.Append("package remove \"");
        sb.Append(packageId);
        sb.Append('"');

        sb.Append(" --project \"");
        sb.Append(path);
        sb.Append('"');

        if (restore.HasValue && !restore.Value)
            sb.Append(" --no-restore");

        return sb.ToString();
    }

    internal static string Clean(string path, string? configuration, string? verbosity, string? framework, string? runtime, string? output)
    {
        using var sb = new PooledStringBuilder();

        sb.Append("clean \"");
        sb.Append(path);
        sb.Append('"');

        if (configuration.HasContent())
        {
            sb.Append(" --configuration ");
            sb.Append(configuration);
        }

        if (verbosity.HasContent())
        {
            sb.Append(" -v ");
            sb.Append(verbosity);
        }

        if (framework.HasContent())
        {
            sb.Append(" -f ");
            sb.Append(framework);
        }

        if (runtime.HasContent())
        {
            sb.Append(" -r ");
            sb.Append(runtime);
        }

        if (output.HasContent())
        {
            sb.Append(" -o \"");
            sb.Append(output);
            sb.Append('"');
        }

        return sb.ToString();
    }

    internal static string ListPackages(string path, bool includeTransitive, bool outdated, bool includePrerelease, bool vulnerable, bool deprecated,
        bool noRestore, string? verbosity, string? framework, bool interactive, string? source)
    {
        var parts = new List<string>(18)
        {
            "package",
            "list",
            "--project",
            $"\"{path}\"",
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

        if (framework.HasContent())
        {
            parts.Add("--framework");
            parts.Add(framework!);
        }

        if (source.HasContent())
        {
            parts.Add("--source");
            parts.Add($"\"{source}\"");
        }

        if (interactive)
            parts.Add("--interactive");

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

    private static void AppendRepeatedOption(ref PooledStringBuilder sb, string option, IReadOnlyList<string>? values)
    {
        if (values is not { Count: > 0 })
            return;

        foreach (string value in values)
        {
            if (value.IsNullOrWhiteSpace())
                continue;

            sb.Append(' ');
            sb.Append(option);
            sb.Append(" \"");
            sb.Append(value);
            sb.Append('"');
        }
    }

    private static void AppendApplicationArguments(ref PooledStringBuilder sb, IReadOnlyList<string>? arguments)
    {
        if (arguments is not { Count: > 0 })
            return;

        var appendedSeparator = false;

        foreach (string argument in arguments)
        {
            if (argument.IsNullOrWhiteSpace())
                continue;

            if (!appendedSeparator)
            {
                sb.Append(" --");
                appendedSeparator = true;
            }

            sb.Append(" \"");
            sb.Append(argument);
            sb.Append('"');
        }
    }

    private static void AppendProperties(ref PooledStringBuilder sb, IReadOnlyList<KeyValuePair<string, string?>>? properties)
    {
        if (properties is not { Count: > 0 })
            return;

        foreach (KeyValuePair<string, string?> property in properties)
        {
            if (property.Key.IsNullOrWhiteSpace())
                continue;

            sb.Append(" -p:");
            sb.Append(property.Key);

            if (property.Value is not null)
            {
                sb.Append('=');
                sb.Append(property.Value);
            }
        }
    }
}