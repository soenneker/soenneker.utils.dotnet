using Microsoft.Extensions.Logging;
using Soenneker.Extensions.ValueTask;
using Soenneker.Utils.Directory.Abstract;
using Soenneker.Utils.Dotnet.Abstract;
using Soenneker.Utils.Dotnet.Dtos;
using Soenneker.Utils.File.Abstract;
using Soenneker.Utils.Process.Abstract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Soenneker.Extensions.String;
using System.Threading.Tasks;

namespace Soenneker.Utils.Dotnet;

/// <inheritdoc cref="IDotnetUtil"/>
public sealed class DotnetUtil : IDotnetUtil
{
    private static readonly Dictionary<string, string> _environmentalVars = new(StringComparer.Ordinal)
    {
        ["DOTNET_CLI_UI_LANGUAGE"] = "en",
        ["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1",
        ["DOTNET_NOLOGO"] = "1"
    };

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ILogger<DotnetUtil> _logger;
    private readonly IProcessUtil _processUtil;
    private readonly IFileUtil _fileUtil;
    private readonly IDirectoryUtil _directoryUtil;

    public DotnetUtil(ILogger<DotnetUtil> logger, IProcessUtil processUtil, IFileUtil fileUtil, IDirectoryUtil directoryUtil)
    {
        _logger = logger;
        _processUtil = processUtil;
        _fileUtil = fileUtil;
        _directoryUtil = directoryUtil;
    }

    public async ValueTask<string> Execute(string arguments, CancellationToken cancellationToken = default)
    {
        List<string> output = await ExecuteRaw("dotnet", arguments, log: false, cancellationToken)
            .NoSync();

        return JoinOutput(output);
    }

    public async ValueTask<(List<KeyValuePair<string, string>> Direct, HashSet<string> Transitive)> GetDependencySetsLocal(string csproj,
        CancellationToken cancellationToken = default)
    {
        PackageListReport report = await GetPackageListReport(
                csproj, includeTransitive: true, outdated: false, noRestore: false, cancellationToken: cancellationToken)
            .NoSync();

        var direct = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var transitive = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (ProjectReport project in report.Projects)
        {
            foreach (FrameworkReport framework in project.Frameworks)
            {
                if (framework.TopLevelPackages is { Count: > 0 })
                {
                    foreach (PackageEntry package in framework.TopLevelPackages)
                    {
                        if (package.Id.IsNullOrWhiteSpace())
                            continue;

                        string version = FirstNonEmpty(package.ResolvedVersion, package.RequestedVersion, package.LatestVersion) ?? string.Empty;
                        direct[package.Id] = version;
                    }
                }

                if (framework.TransitivePackages is { Count: > 0 })
                {
                    foreach (PackageEntry package in framework.TransitivePackages)
                    {
                        if (package.Id.HasContent())
                            transitive.Add(package.Id);
                    }
                }
            }
        }

        return (direct.Select(static kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value))
                      .ToList(), transitive);
    }

    public async ValueTask<bool> Run(string path, string? framework = null, bool log = true, string? configuration = "Release", string? verbosity = "normal",
        bool? build = true, bool? restore = true, string? urls = null, string? launchProfile = null, string? environment = null,
        IReadOnlyList<string>? applicationArguments = null, CancellationToken cancellationToken = default)
    {
        (bool valid, _, _) = await ValidateRunPath(path, log, cancellationToken)
            .NoSync();

        if (!valid)
            return false;

        return await TryExecuteDotnet(
                ArgumentUtil.Run(path, framework, configuration, verbosity, build, restore, urls, launchProfile, environment, applicationArguments), log,
                cancellationToken)
            .NoSync();
    }

    public async ValueTask<Process?> Start(string path, string? framework = null, bool log = true, string? configuration = "Release",
        string? verbosity = "normal", bool? build = true, bool? restore = true, string? urls = null, string? launchProfile = null,
        string? environment = null, IReadOnlyList<string>? applicationArguments = null, Action<string>? outputCallback = null,
        Action<string>? errorCallback = null, CancellationToken cancellationToken = default)
    {
        (bool valid, bool fileExists, _) = await ValidateRunPath(path, log, cancellationToken)
            .NoSync();

        if (!valid)
            return null;

        string arguments = ArgumentUtil.Run(path, framework, configuration, verbosity, build, restore, urls, launchProfile, environment, applicationArguments);

        if (log)
            _logger.LogInformation("Starting: dotnet {Arguments}", arguments);

        var startInfo = new ProcessStartInfo("dotnet", arguments)
        {
            WorkingDirectory = ResolveWorkingDirectory(path, fileExists),
            UseShellExecute = false,
            RedirectStandardOutput = outputCallback is not null,
            RedirectStandardError = errorCallback is not null,
            CreateNoWindow = true
        };

        foreach (KeyValuePair<string, string> environmentalVar in _environmentalVars)
        {
            startInfo.Environment[environmentalVar.Key] = environmentalVar.Value;
        }

        var process = new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };

        if (outputCallback is not null)
        {
            process.OutputDataReceived += (_, args) =>
            {
                if (args.Data.HasContent())
                    outputCallback(args.Data);
            };
        }

        if (errorCallback is not null)
        {
            process.ErrorDataReceived += (_, args) =>
            {
                if (args.Data.HasContent())
                    errorCallback(args.Data);
            };
        }

        try
        {
            if (!process.Start())
            {
                if (log)
                    _logger.LogError("dotnet {Arguments} did not start a process", arguments);

                process.Dispose();
                return null;
            }

            if (startInfo.RedirectStandardOutput)
                process.BeginOutputReadLine();

            if (startInfo.RedirectStandardError)
                process.BeginErrorReadLine();

            if (cancellationToken.CanBeCanceled)
            {
                CancellationTokenRegistration registration = cancellationToken.Register(static state =>
                {
                    var process = (Process)state!;

                    try
                    {
                        if (!process.HasExited)
                            process.Kill(entireProcessTree: true);
                    }
                    catch
                    {
                    }
                }, process);

                process.Exited += (_, _) => registration.Dispose();
            }

            return process;
        }
        catch (Exception ex)
        {
            process.Dispose();

            if (log)
                _logger.LogError(ex, "dotnet {Arguments} failed to start", arguments);

            return null;
        }
    }

    public ValueTask<bool> Restore(string path, bool log = true, string? verbosity = "normal", string? runtime = null, string? packages = null,
        IReadOnlyList<string>? sources = null, string? configFile = null, bool disableParallel = false, CancellationToken cancellationToken = default)
    {
        return TryExecuteDotnet(ArgumentUtil.Restore(path, verbosity, runtime, packages, sources, configFile, disableParallel), log, cancellationToken);
    }

    public ValueTask<bool> Build(string path, bool log = true, string? configuration = "Release", bool? restore = true, string? verbosity = "normal",
        string? framework = null, string? runtime = null, bool? selfContained = null, string? output = null,
        IReadOnlyList<KeyValuePair<string, string?>>? properties = null, CancellationToken cancellationToken = default)
    {
        return TryExecuteDotnet(ArgumentUtil.Build(path, configuration, restore, verbosity, framework, runtime, selfContained, output, properties), log,
            cancellationToken);
    }

    public ValueTask<bool> Test(string path, bool log = true, bool? restore = true, bool? build = true, string? configuration = "Release",
        string? verbosity = "normal", string? framework = null, string? filter = null, string? logger = null, string? resultsDirectory = null,
        CancellationToken cancellationToken = default)
    {
        return TryExecuteDotnet(ArgumentUtil.Test(path, restore, build, configuration, verbosity, framework, filter, logger, resultsDirectory), log,
            cancellationToken);
    }

    public ValueTask<bool> Pack(string path, string version, bool log = true, string? configuration = "Release", bool? build = false, bool? restore = false,
        string? output = ".", string? verbosity = "normal", string? framework = null, string? runtime = null, bool? includeSymbols = null,
        bool? includeSource = null, bool? serviceable = null, IReadOnlyList<KeyValuePair<string, string?>>? properties = null,
        CancellationToken cancellationToken = default)
    {
        return TryExecuteDotnet(
            ArgumentUtil.Pack(path, version, configuration, build, restore, output, verbosity, framework, runtime, includeSymbols, includeSource, serviceable,
                properties), log, cancellationToken);
    }

    public ValueTask<bool> RemovePackage(string path, string packageId, bool log = true, bool? restore = true, CancellationToken cancellationToken = default)
    {
        return TryExecuteDotnet(ArgumentUtil.RemovePackage(path, packageId, restore), log, cancellationToken);
    }

    public ValueTask<bool> AddPackage(string projectPath, string packageId, string? version = null, bool log = true, bool? restore = true,
        string? framework = null, string? source = null, bool prerelease = false, string? packageDirectory = null, bool interactive = false,
        CancellationToken cancellationToken = default)
    {
        return TryExecuteDotnet(ArgumentUtil.AddPackage(projectPath, packageId, version, restore, framework, source, prerelease, packageDirectory, interactive),
            log, cancellationToken);
    }

    public async ValueTask<bool> UpdatePackages(string path, bool log = true, string? verbosity = "normal", CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating packages for path ({Path})", path);

        List<string> projectFiles = await GetProjectFiles(path, cancellationToken)
            .NoSync();

        if (projectFiles.Count == 0)
        {
            _logger.LogError("No projects found at path ({Path})", path);
            return false;
        }

        bool restoreSuccess = await Restore(path, log, verbosity, cancellationToken: cancellationToken)
            .NoSync();

        if (!restoreSuccess)
        {
            _logger.LogError("Initial restore failed for path ({Path})", path);
            return false;
        }

        var allPackagesUpdated = true;

        foreach (string projectFile in projectFiles)
        {
            _logger.LogInformation("Checking outdated packages for project ({ProjectFile})...", projectFile);

            PackageListReport report = await GetPackageListReport(projectFile, includeTransitive: false, outdated: true, noRestore: true, verbosity: verbosity,
                    log: log, cancellationToken: cancellationToken)
                .NoSync();

            List<PackageUpdateCandidate> candidates = GetOutdatedTopLevelPackages(report);

            if (candidates.Count == 0)
            {
                _logger.LogInformation("No outdated packages found for project ({ProjectFile})", projectFile);
                continue;
            }

            foreach (PackageUpdateCandidate candidate in candidates)
            {
                _logger.LogInformation("Updating package ({PackageId}) in project ({ProjectFile}) from {ResolvedVersion} to {LatestVersion}...",
                    candidate.PackageId, projectFile, candidate.ResolvedVersion, candidate.LatestVersion);

                bool updated = await AddPackage(projectFile, candidate.PackageId, candidate.LatestVersion, log, restore: false,
                        cancellationToken: cancellationToken)
                    .NoSync();

                if (!updated)
                {
                    _logger.LogError("Failed to update package ({PackageId}) in project ({ProjectFile})", candidate.PackageId, projectFile);
                    allPackagesUpdated = false;
                }
            }
        }

        bool finalRestoreSuccess = await Restore(path, log, verbosity, cancellationToken: cancellationToken)
            .NoSync();

        if (!finalRestoreSuccess)
        {
            _logger.LogError("Final restore failed after package updates for path ({Path})", path);
            return false;
        }

        if (allPackagesUpdated)
            _logger.LogInformation("Successfully updated all outdated packages in all projects");
        else
            _logger.LogWarning("Some packages failed to update");

        return allPackagesUpdated;
    }

    public ValueTask<bool> Clean(string path, bool log = true, string? configuration = "Release", string? verbosity = "normal", string? framework = null,
        string? runtime = null, string? output = null, CancellationToken cancellationToken = default)
    {
        return TryExecuteDotnet(ArgumentUtil.Clean(path, configuration, verbosity, framework, runtime, output), log, cancellationToken);
    }

    public async ValueTask<List<KeyValuePair<string, string>>> ListPackages(string path, bool outdated = false, bool transitive = false,
        bool includePrerelease = false, bool vulnerable = false, bool deprecated = false, bool log = true, string? verbosity = "normal",
        string? framework = null, bool interactive = false, string? source = null, CancellationToken cancellationToken = default)
    {
        PackageListReport report = await GetPackageListReport(path, includeTransitive: transitive, outdated: outdated, includePrerelease: includePrerelease,
                vulnerable: vulnerable, deprecated: deprecated, noRestore: false, verbosity: verbosity, log: log, framework: framework,
                interactive: interactive, source: source, cancellationToken: cancellationToken)
            .NoSync();

        var packages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (ProjectReport project in report.Projects)
        {
            foreach (FrameworkReport frameworkReport in project.Frameworks)
            {
                IReadOnlyList<PackageEntry>? entries = transitive ? frameworkReport.TransitivePackages : frameworkReport.TopLevelPackages;

                if (entries is not { Count: > 0 })
                    continue;

                foreach (PackageEntry entry in entries)
                {
                    if (entry.Id.IsNullOrWhiteSpace())
                        continue;

                    if (outdated)
                    {
                        string? resolved = FirstNonEmpty(entry.ResolvedVersion, entry.RequestedVersion);
                        string? latest = entry.LatestVersion;

                        if (resolved.IsNullOrWhiteSpace() || latest.IsNullOrWhiteSpace())
                            continue;

                        if (string.Equals(resolved, latest, StringComparison.OrdinalIgnoreCase))
                            continue;

                        packages[entry.Id] = resolved;
                    }
                    else
                    {
                        string version = FirstNonEmpty(entry.ResolvedVersion, entry.RequestedVersion, entry.LatestVersion) ?? string.Empty;
                        packages[entry.Id] = version;
                    }
                }
            }
        }

        return packages.Select(static kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value))
                       .ToList();
    }

    private async ValueTask<PackageListReport> GetPackageListReport(string path, bool includeTransitive, bool outdated, bool includePrerelease = false,
        bool vulnerable = false, bool deprecated = false, bool noRestore = false, string? verbosity = null, bool log = true, string? framework = null,
        bool interactive = false, string? source = null, CancellationToken cancellationToken = default)
    {
        string? effectiveVerbosity = verbosity;

        if (effectiveVerbosity != null && !string.Equals(effectiveVerbosity, "quiet", StringComparison.OrdinalIgnoreCase))
            effectiveVerbosity = "quiet";

        string args = ArgumentUtil.ListPackages(path, includeTransitive, outdated, includePrerelease, vulnerable, deprecated, noRestore, effectiveVerbosity,
            framework, interactive, source);

        List<string> output = await ExecuteDotnet(args, log, cancellationToken)
            .NoSync();

        string json = JoinOutput(output);

        var report = JsonSerializer.Deserialize<PackageListReport>(json, _jsonOptions);

        if (report is null)
            throw new InvalidOperationException($"Failed to deserialize dotnet package list JSON for '{path}'.");

        return report;
    }

    private ValueTask<List<string>> ExecuteDotnet(string arguments, bool log, CancellationToken cancellationToken)
    {
        return ExecuteRaw("dotnet", arguments, log, cancellationToken);
    }

    private async ValueTask<List<string>> ExecuteRaw(string fileName, string arguments, bool log, CancellationToken cancellationToken)
    {
        if (log)
            _logger.LogInformation("Executing: {FileName} {Arguments}", fileName, arguments);

        return await _processUtil.Start(fileName, arguments: arguments, log: false, environmentalVars: _environmentalVars, cancellationToken: cancellationToken)
                                 .NoSync();
    }

    private async ValueTask<bool> TryExecuteDotnet(string arguments, bool log, CancellationToken cancellationToken)
    {
        try
        {
            await ExecuteDotnet(arguments, log, cancellationToken)
                .NoSync();

            return true;
        }
        catch (Exception ex)
        {
            if (log)
                _logger.LogError(ex, "dotnet {Arguments} failed", arguments);

            return false;
        }
    }

    private async ValueTask<(bool valid, bool fileExists, bool directoryExists)> ValidateRunPath(string path, bool log, CancellationToken cancellationToken)
    {
        if (path.IsNullOrWhiteSpace())
            throw new ArgumentException("Path cannot be null or whitespace.", nameof(path));

        bool fileExists = await _fileUtil.Exists(path, cancellationToken)
                                         .NoSync();

        bool directoryExists = !fileExists && await _directoryUtil.Exists(path, cancellationToken)
                                                                  .NoSync();

        if (!fileExists && !directoryExists)
        {
            if (log)
                _logger.LogError("Cannot run dotnet because path does not exist: {Path}", path);

            return (false, false, false);
        }

        if (fileExists && !path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
        {
            if (log)
                _logger.LogError("Cannot run dotnet because file is not a .csproj: {Path}", path);

            return (false, true, false);
        }

        return (true, fileExists, directoryExists);
    }

    private static string ResolveWorkingDirectory(string path, bool fileExists)
    {
        if (!fileExists)
            return path;

        string? directory = Path.GetDirectoryName(path);

        if (directory.IsNullOrWhiteSpace())
            throw new InvalidOperationException($"Could not determine working directory for '{path}'.");

        return directory;
    }

    public async ValueTask<List<string>> GetProjectFiles(string path, CancellationToken cancellationToken)
    {
        var projectFiles = new List<string>();

        if (await _fileUtil.Exists(path, cancellationToken) && path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
        {
            projectFiles.Add(path);
        }
        else if (await _directoryUtil.Exists(path, cancellationToken))
        {
            List<string> files = await _directoryUtil.GetFilesByExtension(path, "csproj", true, cancellationToken)
                                                     .NoSync();

            projectFiles.AddRange(files);
        }

        return projectFiles;
    }

    private static List<PackageUpdateCandidate> GetOutdatedTopLevelPackages(PackageListReport report)
    {
        var candidates = new Dictionary<string, PackageUpdateCandidate>(StringComparer.OrdinalIgnoreCase);

        foreach (ProjectReport project in report.Projects)
        {
            foreach (FrameworkReport framework in project.Frameworks)
            {
                if (framework.TopLevelPackages is not { Count: > 0 })
                    continue;

                foreach (PackageEntry package in framework.TopLevelPackages)
                {
                    if (package.Id.IsNullOrWhiteSpace())
                        continue;

                    string? resolved = FirstNonEmpty(package.ResolvedVersion, package.RequestedVersion);
                    string? latest = package.LatestVersion;

                    if (resolved.IsNullOrWhiteSpace() || latest.IsNullOrWhiteSpace())
                        continue;

                    if (string.Equals(resolved, latest, StringComparison.OrdinalIgnoreCase))
                        continue;

                    candidates[package.Id] = new PackageUpdateCandidate(package.Id, resolved, latest);
                }
            }
        }

        return candidates.Values.ToList();
    }

    private static string JoinOutput(IReadOnlyList<string> output)
    {
        return output.Count switch
        {
            0 => string.Empty,
            1 => output[0],
            _ => string.Join(Environment.NewLine, output)
        };
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (string? value in values)
        {
            if (value.HasContent())
                return value;
        }

        return null;
    }
}