namespace Soenneker.Utils.Dotnet;

internal static class ArgumentUtil
{
    internal static string Run(string path, string? framework, string? configuration, string? verbosity, bool? build)
    {
        var argument = $"\"{path}\"";

        if (framework != null)
            argument += $" -f {framework}";

        if (configuration != null)
            argument += $" -c {configuration}";

        if (verbosity != null)
            argument += $" -v {verbosity}";

        if (build != null)
        {
            if (!build.Value)
                argument += " --no-build";
        }

        return argument;
    }

    internal static string Restore(string path, string? verbosity)
    {
        var argument = $"\"{path}\"";

        if (verbosity != null)
            argument += $" -v {verbosity}";

        return argument;
    }

    internal static string Build(string path, string? configuration, bool? restore, string? verbosity)
    {
        var argument = $"\"{path}\"";

        if (configuration != null)
            argument += $" -c {configuration}";

        if (restore != null)
        {
            if (!restore.Value)
                argument += " --no-restore";
        }

        if (verbosity != null)
            argument += $" -v {verbosity}";

        return argument;
    }

    internal static string Test(string path, bool? restore, string? verbosity)
    {
        var argument = $"\"{path}\"";

        if (restore != null)
        {
            if (!restore.Value)
                argument += " --no-restore";
        }

        if (verbosity != null)
            argument += $" -v {verbosity}";

        return argument;
    }

    internal static string Pack(string path, string version, string? configuration, bool? build, bool? restore, string? output, string? verbosity)
    {
        var argument = $"\"{path}\"";

        argument += $" -p:PackageVersion={version}";

        if (configuration != null)
            argument += $" -c {configuration}";

        if (build != null)
        {
            if (!build.Value)
                argument += " --no-build";
        }

        if (restore != null)
        {
            if (!restore.Value)
                argument += " --no-restore";
        }

        if (output != null)
            argument += $" --output \"{output}\"";

        if (verbosity != null)
            argument += $" -v {verbosity}";

        return argument;
    }

    internal static string AddPackage(string path, string packageId, string? version, bool? restore, string? verbosity)
    {
        var argument = $"\"{path}\" package \"{packageId}\"";

        if (version != null)
            argument += $" --version {version}";

        if (restore.HasValue && !restore.Value)
            argument += " --no-restore";

        if (!string.IsNullOrEmpty(verbosity))
            argument += $" -v {verbosity}";

        return argument;
    }

    internal static string RemovePackage(string path, string packageId, bool? restore, string? verbosity)
    {
        var argument = $"\"{path}\" package \"{packageId}\"";

        if (restore != null && !restore.Value)
            argument += " --no-restore";

        if (verbosity != null)
            argument += $" -v {verbosity}";

        return argument;
    }

    internal static string Clean(string path, string? configuration, string? verbosity)
    {
        var argument = $"\"{path}\"";

        if (!string.IsNullOrEmpty(configuration))
            argument += $" --configuration {configuration}";

        if (!string.IsNullOrEmpty(verbosity))
            argument += $" -v {verbosity}";

        return argument;
    }

    internal static string ListPackages(string path, bool outdatedOnly, string? verbosity)
    {
        var argument = $"\"{path}\" package";

        if (outdatedOnly)
            argument += " --outdated";

        if (!string.IsNullOrEmpty(verbosity))
            argument += $" -v {verbosity}";

        return argument;
    }
}