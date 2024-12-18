using System;
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

    public ValueTask<bool> Run(string path, string? framework = null, bool log = true, string? configuration = "Release", string? verbosity = "normal", bool? build = true,
        CancellationToken cancellationToken = default)
    {
        return ExecuteCommand(
            "run",
            path,
            p => ArgumentUtil.Run(p, framework, configuration, verbosity, build),
            _ => true, // No specific success criteria for `dotnet run`
            log,
            cancellationToken
        );
    }

    public ValueTask<bool> Restore(string path, bool log = true, string? verbosity = "normal", CancellationToken cancellationToken = default)
    {
        return ExecuteCommand(
            "restore",
            path,
            p => ArgumentUtil.Restore(p, verbosity),
            output => output.Contains("Restore completed", StringComparison.OrdinalIgnoreCase),
            log,
            cancellationToken
        );
    }

    public ValueTask<bool> Build(string path, bool log = true, string? configuration = "Release", bool? restore = true, string? verbosity = "normal", CancellationToken cancellationToken = default)
    {
        return ExecuteCommand(
            "build",
            path,
            p => ArgumentUtil.Build(p, configuration, restore, verbosity),
            output => output.Contains("0 Error(s)", StringComparison.OrdinalIgnoreCase),
            log,
            cancellationToken
        );
    }

    public ValueTask<bool> Test(string path, bool log = true, bool? restore = true, string? verbosity = "normal", CancellationToken cancellationToken = default)
    {
        return ExecuteCommand(
            "test",
            path,
            p => ArgumentUtil.Test(p, restore, verbosity),
            output => output.Contains("0 Error(s)", StringComparison.OrdinalIgnoreCase),
            log,
            cancellationToken
        );
    }

    public ValueTask<bool> Pack(string path, string version, bool log = true, string? configuration = "Release", bool? build = false, bool? restore = false, string? output = ".",
        string? verbosity = "normal", CancellationToken cancellationToken = default)
    {
        return ExecuteCommand(
            "pack",
            path,
            p => ArgumentUtil.Pack(p, version, configuration, build, restore, output, verbosity),
            o => o.Contains("0 Error(s)", StringComparison.OrdinalIgnoreCase),
            log,
            cancellationToken
        );
    }

    public ValueTask<bool> RemovePackage(string path, string packageId, bool log = true, bool? restore = true, string? verbosity = "normal", CancellationToken cancellationToken = default)
    {
        return ExecuteCommand(
            "remove",
            path,
            p => ArgumentUtil.RemovePackage(p, packageId, restore, verbosity),
            output => output.Contains("Successfully removed", StringComparison.OrdinalIgnoreCase) ||
                      output.Contains("does not contain", StringComparison.OrdinalIgnoreCase),
            log,
            cancellationToken
        );
    }


    public ValueTask<bool> AddPackage(string projectPath, string packageId, string? version = null, bool log = true, bool? restore = true, string? verbosity = "normal",
        CancellationToken cancellationToken = default)
    {
        return ExecuteCommand(
            "add",
            projectPath,
            path => ArgumentUtil.AddPackage(path, packageId, version, restore, verbosity),
            output => output.Contains("added package", StringComparison.OrdinalIgnoreCase),
            log,
            cancellationToken
        );
    }

    public async ValueTask<bool> UpdatePackages(string path, bool log = true, string? verbosity = "normal", CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating packages for path ({path})...", path);

        List<string> outdatedPackages = await ListPackages(
            path,
            outdatedOnly: true,
            log: log,
            verbosity: verbosity,
            cancellationToken: cancellationToken
        ).NoSync();

        if (outdatedPackages.Count == 0)
        {
            _logger.LogInformation("No outdated packages found to update");
            return true;
        }

        foreach (string package in outdatedPackages)
        {
            _logger.LogInformation("Updating package: {Package}", package);

            bool updateSuccess = await AddPackage(
                path,
                packageId: package,
                version: null, // Update to the latest version
                log: log,
                restore: true,
                verbosity: verbosity,
                cancellationToken: cancellationToken
            ).NoSync();

            if (!updateSuccess)
            {
                _logger.LogError("Failed to update package ({Package}); exiting early", package);
                return false; // Stop and return false if any update fails
            }
        }

        _logger.LogInformation("Successfully updated all outdated packages");
        return true;
    }

    public ValueTask<bool> Clean(string path, bool log = true, string? configuration = "Release", string? verbosity = "normal", CancellationToken cancellationToken = default)
    {
        return ExecuteCommand(
            "clean",
            path,
            p => ArgumentUtil.Clean(p, configuration, verbosity),
            output => output.Contains("Cleaned", StringComparison.OrdinalIgnoreCase),
            log,
            cancellationToken
        );
    }

    public async ValueTask<List<string>> ListPackages(string path, bool outdatedOnly = false, bool log = true, string? verbosity = "normal", CancellationToken cancellationToken = default)
    {
        var packages = new List<string>();

        List<string> processOutput = await ExecuteCommandWithOutput(
            "list",
            path,
            p => ArgumentUtil.ListPackages(p, outdatedOnly, verbosity),
            log,
            cancellationToken
        );

        foreach (string output in processOutput)
        {
            if (output.Contains('>', StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = output.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 2)
                    packages.Add(parts[1]);
            }
        }

        return packages;
    }

    public async ValueTask<bool> ExecuteCommand(string command, string projectPath, Func<string, string> argumentBuilder, Func<string, bool> successCriteria,
        bool log = true, CancellationToken cancellationToken = default)
    {
        List<string> processOutput = await ExecuteCommandWithOutput(command, projectPath, argumentBuilder, log, cancellationToken).NoSync();

        foreach (string output in processOutput)
        {
            if (successCriteria(output))
                return true;
        }

        return false;
    }

    public async ValueTask<List<string>> ExecuteCommandWithOutput(string command, string projectPath, Func<string, string> argumentBuilder, bool log = true,
        CancellationToken cancellationToken = default)
    {
        string arguments = argumentBuilder(projectPath);

        if (log)
            _logger.LogInformation("Executing: dotnet {Command} {Arguments} ...", command, arguments);

        List<string> processOutput = await _processUtil.StartProcess("dotnet", null, $"{command} {arguments}", true, true, log, cancellationToken).NoSync();

        return processOutput;
    }
}