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
            null,
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
            null,
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
            null,
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
            output =>
            {
                return output.Contains("test succeeded", StringComparison.OrdinalIgnoreCase)
                       || output.Contains("Passed!", StringComparison.OrdinalIgnoreCase)
                       || output.Contains("Test Run Successful.", StringComparison.OrdinalIgnoreCase);
            },
            output =>
            {
                return output.Contains("build failed", StringComparison.OrdinalIgnoreCase)
                       || output.Contains("test failed", StringComparison.OrdinalIgnoreCase);
            },
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
            null,
            log,
            cancellationToken
        );
    }

    public ValueTask<bool> RemovePackage(string path, string packageId, bool log = true, bool? restore = true, CancellationToken cancellationToken = default)
    {
        return ExecuteCommand(
            "remove",
            path,
            p => ArgumentUtil.RemovePackage(p, packageId, restore),
            output => output.Contains("Successfully removed", StringComparison.OrdinalIgnoreCase) ||
                      output.Contains("does not contain", StringComparison.OrdinalIgnoreCase),
            null,
            log,
            cancellationToken
        );
    }

    public ValueTask<bool> AddPackage(string projectPath, string packageId, string? version = null, bool log = true, bool? restore = true,
        CancellationToken cancellationToken = default)
    {
        return ExecuteCommand(
            "add",
            projectPath,
            path => ArgumentUtil.AddPackage(path, packageId, version, restore), output =>
            {
                // Check for success indicators in the output
                return output.Contains("PackageReference for package", StringComparison.OrdinalIgnoreCase)
                       && output.Contains("updated in file", StringComparison.OrdinalIgnoreCase);
            },
            null,
            log,
            cancellationToken
        );
    }

    public async ValueTask<bool> UpdatePackages(string path, bool log = true, string? verbosity = "normal", CancellationToken cancellationToken = default)
    {
        await Restore(path, log, verbosity, cancellationToken).NoSync();

        _logger.LogInformation("Updating packages for path ({Path})", path);

        List<string> projectFiles = ProjectHelper.GetProjectFiles(path);

        if (projectFiles.Count == 0)
        {
            _logger.LogError("No projects found at path ({Path})", path);
            return false;
        }

        var allPackagesUpdated = true;

        // Step 2: Iterate through each project and update packages
        foreach (string projectFile in projectFiles)
        {
            _logger.LogInformation("Checking outdated packages for project ({ProjectFile})...", projectFile);

            // Get outdated packages for the current project
            List<string> outdatedPackages = await ListPackages(
                projectFile,
                outdated: true,
                log: log,
                verbosity: verbosity,
                cancellationToken: cancellationToken
            ).NoSync();

            if (outdatedPackages.Count == 0)
            {
                _logger.LogInformation("No outdated packages found for project ({ProjectFile})", projectFile);
                continue;
            }

            foreach (string package in outdatedPackages)
            {
                _logger.LogInformation("Updating package ({Package}) in project ({ProjectFile})...", package, projectFile);

                bool updateSuccess = await AddPackage(
                    projectFile,
                    packageId: package,
                    version: null, // Update to the latest version
                    log: log,
                    restore: true,
                    cancellationToken: cancellationToken
                ).NoSync();

                if (!updateSuccess)
                {
                    _logger.LogError("Failed to update package ({Package}) in project ({ProjectFile})", package, projectFile);
                    allPackagesUpdated = false; // Mark as failure
                }
            }
        }

        if (allPackagesUpdated)
            _logger.LogInformation("Successfully updated all outdated packages in all projects");
        else
            _logger.LogWarning("Some packages failed to update");

        return allPackagesUpdated;
    }

    public ValueTask<bool> Clean(string path, bool log = true, string? configuration = "Release", string? verbosity = "normal", CancellationToken cancellationToken = default)
    {
        return ExecuteCommand(
            "clean",
            path,
            p => ArgumentUtil.Clean(p, configuration, verbosity),
            output => output.Contains("Cleaned", StringComparison.OrdinalIgnoreCase),
            null,
            log,
            cancellationToken
        );
    }

    public async ValueTask<List<string>> ListPackages(string path, bool outdated = false, bool transitive = false, bool includePrerelease = false, bool vulnerable = false,
        bool deprecated = false, bool log = true, string? verbosity = "normal", CancellationToken cancellationToken = default)
    {
        var packages = new List<string>();

        List<string> processOutput = await ExecuteCommandWithOutput(
            "list",
            path,
            p => ArgumentUtil.ListPackages(p, outdated, transitive, includePrerelease, vulnerable, deprecated, verbosity),
            log,
            cancellationToken
        );

        bool inTransitiveSection = false;

        foreach (string output in processOutput)
        {
            // Detect section headers
            if (output.StartsWith("   Top-level Package", StringComparison.OrdinalIgnoreCase))
            {
                inTransitiveSection = false;
                continue;
            }
            if (output.StartsWith("   Transitive Package", StringComparison.OrdinalIgnoreCase))
            {
                inTransitiveSection = true;
                continue;
            }

            // Process outdated format
            if (outdated && output.Contains('>', StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = output.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 4)
                {
                    string packageName = parts[1];
                    string resolvedVersion = parts[2];
                    string latestVersion = parts[3];

                    // Add to results only if the package is outdated
                    if (!string.Equals(resolvedVersion, latestVersion, StringComparison.OrdinalIgnoreCase))
                    {
                        packages.Add(packageName);
                    }
                }
            }
            // Process standard format
            else if (!outdated && output.Contains('>', StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = output.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 2)
                {
                    string packageName = parts[1];

                    // Include packages based on the transitive parameter
                    if ((transitive && inTransitiveSection) || (!transitive && !inTransitiveSection))
                    {
                        packages.Add(packageName);
                    }
                }
            }
        }

        return packages;
    }

    public async ValueTask<bool> ExecuteCommand(string command, string projectPath, Func<string, string> argumentBuilder,
        Func<string, bool> successCriteria, Func<string, bool>? failureCriteria = null, bool log = true, CancellationToken cancellationToken = default)
    {
        List<string> processOutput = await ExecuteCommandWithOutput(command, projectPath, argumentBuilder, log, cancellationToken).NoSync();

        foreach (string output in processOutput)
        {
            if (successCriteria(output))
                return true;

            if (failureCriteria != null && failureCriteria(output))
                return false;
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