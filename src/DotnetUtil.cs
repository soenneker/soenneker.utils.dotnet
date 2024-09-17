using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Soenneker.Extensions.ValueTask;
using Soenneker.Utils.Dotnet.Abstract;
using Soenneker.Utils.Process.Abstract;

namespace Soenneker.Utils.Dotnet;

/// <inheritdoc cref="IDotnetUtil"/>
public class DotnetUtil : IDotnetUtil
{
    private readonly ILogger<DotnetUtil> _logger;
    private readonly IProcessUtil _processUtil;

    public DotnetUtil(ILogger<DotnetUtil> logger, IProcessUtil processUtil)
    {
        _logger = logger;
        _processUtil = processUtil;
    }

    public async ValueTask Run(string path, string? framework = null, bool log = true, string? configuration = "Release", string? verbosity = "normal", bool? build = true, CancellationToken cancellationToken = default)
    {
        string arguments = CreateRunArgument(path, framework, configuration, verbosity, build);

        if (log)
            _logger.LogInformation("Executing: dotnet {arguments} ...", arguments);

        List<string> _ = await _processUtil.StartProcess("dotnet", null, arguments, true, true, log, cancellationToken).NoSync();
    }

    private static string CreateRunArgument(string path, string? framework, string? configuration, string? verbosity, bool? build)
    {
        var argument = $"run \"{path}\"";

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

    public async ValueTask Restore(string path, bool log = true, string? verbosity = "normal", CancellationToken cancellationToken = default)
    {
        string arguments = CreateRestoreArgument(path, verbosity);

        if (log)
            _logger.LogInformation("Executing: dotnet {arguments} ...", arguments);

        List<string> _ = await _processUtil.StartProcess("dotnet", null, arguments, true, true, log, cancellationToken).NoSync();
    }

    private static string CreateRestoreArgument(string path, string? verbosity)
    {
        var argument = $"restore \"{path}\"";

        if (verbosity != null)
            argument += $" -v {verbosity}";

        return argument;
    }

    public async ValueTask<bool> Build(string path, bool log = true, string? configuration = "Release", bool? restore = true, string? verbosity = "normal", CancellationToken cancellationToken = default)
    {
        string arguments = CreateBuildArgument(path, configuration, restore, verbosity);

        if (log)
            _logger.LogInformation("Executing: dotnet {arguments} ...", arguments);

        List<string> processOutput = await _processUtil.StartProcess("dotnet", null, arguments, true, true, log, cancellationToken).NoSync();

        foreach (string output in processOutput)
        {
            if (output == "    0 Error(s)")
                return true;
        }

        return false;
    }

    private static string CreateBuildArgument(string path, string? configuration, bool? restore, string? verbosity)
    {
        var argument = $"build \"{path}\"";

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

    public async ValueTask<bool> Test(string path, bool log = true, bool? restore = true, string? verbosity = "normal", CancellationToken cancellationToken = default)
    {
        string arguments = CreateTestArgument(path, restore, verbosity);

        if (log)
            _logger.LogInformation("Executing: dotnet {arguments} ...", arguments);

        List<string> processOutput = await _processUtil.StartProcess("dotnet", null, arguments, true, true, log, cancellationToken).NoSync();

        foreach (string output in processOutput)
        {
            if (output == "    0 Error(s)")
                return true;
        }

        return false;
    }

    private static string CreateTestArgument(string path, bool? restore, string? verbosity)
    {
        var argument = $"test \"{path}\"";

        if (restore != null)
        {
            if (!restore.Value)
                argument += " --no-restore";
        }

        if (verbosity != null)
            argument += $" -v {verbosity}";

        return argument;
    }

    public async ValueTask<bool> Pack(string path, string version, bool log = true, string? configuration = "Release", bool? build = false, bool? restore = false, string? output = ".", string? verbosity = "normal", CancellationToken cancellationToken = default)
    {
        string arguments = CreatePackArgument(path, version, configuration, build, restore, output, verbosity);

        if (log)
            _logger.LogInformation("Executing: dotnet {arguments} ...", arguments);

        List<string> processOutput = await _processUtil.StartProcess("dotnet", null, arguments, true, true, log, cancellationToken).NoSync();

        foreach (string str in processOutput)
        {
            if (str == "    0 Error(s)")
                return true;
        }

        return false;
    }

    private static string CreatePackArgument(string path, string version, string? configuration, bool? build, bool? restore, string? output, string? verbosity)
    {
        var argument = $"pack \"{path}\"";

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
}