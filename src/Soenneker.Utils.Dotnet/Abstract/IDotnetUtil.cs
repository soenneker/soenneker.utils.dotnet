using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DiagnosticsProcess = System.Diagnostics.Process;

namespace Soenneker.Utils.Dotnet.Abstract;

/// <summary>
/// Utility for executing <c>dotnet</c> CLI commands in a structured, programmatic way.
/// Provides wrappers for common operations such as run, build, test, restore, pack, and package management.
/// </summary>
public interface IDotnetUtil
{
    /// <summary>
    /// Executes a raw <c>dotnet</c> CLI command and returns the combined output.
    /// </summary>
    /// <param name="arguments">Arguments to pass to the <c>dotnet</c> executable.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The combined stdout output as a single string.</returns>
    ValueTask<string> Execute(string arguments, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves direct and transitive dependency sets for a given project.
    /// </summary>
    /// <param name="csproj">Path to a <c>.csproj</c> file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item><description>Direct dependencies (package ID and version).</description></item>
    /// <item><description>Transitive dependency package IDs.</description></item>
    /// </list>
    /// </returns>
    ValueTask<(List<KeyValuePair<string, string>> Direct, HashSet<string> Transitive)> GetDependencySetsLocal(string csproj,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes <c>dotnet run</c> for a project or directory.
    /// </summary>
    /// <param name="path">Path to a <c>.csproj</c> or directory containing a project.</param>
    /// <param name="framework">Target framework to run.</param>
    /// <param name="log">Whether to log execution details.</param>
    /// <param name="configuration">Build configuration (e.g. Release or Debug).</param>
    /// <param name="verbosity">CLI verbosity level.</param>
    /// <param name="build">Whether to build before running.</param>
    /// <param name="restore">Whether to restore before running.</param>
    /// <param name="urls">Application URLs to bind (ASP.NET scenarios).</param>
    /// <param name="launchProfile">Launch profile to use.</param>
    /// <param name="environment">Environment name (e.g. Development, Production).</param>
    /// <param name="applicationArguments">Arguments passed to the application (after <c>--</c>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the command succeeded; otherwise <c>false</c>.</returns>
    ValueTask<bool> Run(string path, string? framework = null, bool log = true, string? configuration = "Release",
        string? verbosity = "normal", bool? build = true, bool? restore = true, string? urls = null,
        string? launchProfile = null, string? environment = null, IReadOnlyList<string>? applicationArguments = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts <c>dotnet run</c> without waiting for the target process to exit.
    /// Intended for long-running apps such as local web servers used by integration tests.
    /// </summary>
    /// <param name="path">Path to a <c>.csproj</c> or directory containing a project.</param>
    /// <param name="framework">Target framework to run.</param>
    /// <param name="log">Whether to log execution details.</param>
    /// <param name="configuration">Build configuration (e.g. Release or Debug).</param>
    /// <param name="verbosity">CLI verbosity level.</param>
    /// <param name="build">Whether to build before running.</param>
    /// <param name="restore">Whether to restore before running.</param>
    /// <param name="urls">Application URLs to bind (ASP.NET scenarios).</param>
    /// <param name="launchProfile">Launch profile to use.</param>
    /// <param name="environment">Environment name (e.g. Development, Production).</param>
    /// <param name="applicationArguments">Arguments passed to the application (after <c>--</c>).</param>
    /// <param name="outputCallback">Optional callback invoked for each stdout line.</param>
    /// <param name="errorCallback">Optional callback invoked for each stderr line.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The started process, or <c>null</c> if launch validation or startup failed.</returns>
    ValueTask<DiagnosticsProcess?> Start(string path, string? framework = null, bool log = true, string? configuration = "Release",
        string? verbosity = "normal", bool? build = true, bool? restore = true, string? urls = null,
        string? launchProfile = null, string? environment = null, IReadOnlyList<string>? applicationArguments = null,
        Action<string>? outputCallback = null, Action<string>? errorCallback = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes <c>dotnet restore</c>.
    /// </summary>
    /// <param name="path">Project or solution path.</param>
    /// <param name="log">Whether to log execution details.</param>
    /// <param name="verbosity">CLI verbosity level.</param>
    /// <param name="runtime">Runtime identifier (RID).</param>
    /// <param name="packages">Custom global packages folder.</param>
    /// <param name="sources">Package sources.</param>
    /// <param name="configFile">NuGet config file path.</param>
    /// <param name="disableParallel">Disable parallel restore.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if successful.</returns>
    ValueTask<bool> Restore(string path, bool log = true, string? verbosity = "normal", string? runtime = null,
        string? packages = null, IReadOnlyList<string>? sources = null, string? configFile = null, bool disableParallel = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes <c>dotnet build</c>.
    /// </summary>
    /// <param name="path">Project or solution path.</param>
    /// <param name="log">Whether to log execution details.</param>
    /// <param name="configuration">Build configuration.</param>
    /// <param name="restore">Whether to restore before building.</param>
    /// <param name="verbosity">CLI verbosity level.</param>
    /// <param name="framework">Target framework.</param>
    /// <param name="runtime">Runtime identifier (RID).</param>
    /// <param name="selfContained">Whether to produce a self-contained build.</param>
    /// <param name="output">Output directory.</param>
    /// <param name="properties">MSBuild properties.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if successful.</returns>
    ValueTask<bool> Build(string path, bool log = true, string? configuration = "Release", bool? restore = true,
        string? verbosity = "normal", string? framework = null, string? runtime = null, bool? selfContained = null,
        string? output = null, IReadOnlyList<KeyValuePair<string, string?>>? properties = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes <c>dotnet test</c>.
    /// </summary>
    /// <param name="path">Project or solution path.</param>
    /// <param name="log">Whether to log execution details.</param>
    /// <param name="restore">Whether to restore before testing.</param>
    /// <param name="build">Whether to build before testing.</param>
    /// <param name="configuration">Build configuration.</param>
    /// <param name="verbosity">CLI verbosity level.</param>
    /// <param name="framework">Target framework.</param>
    /// <param name="filter">Test filter expression.</param>
    /// <param name="logger">Logger configuration.</param>
    /// <param name="resultsDirectory">Directory for test results.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if successful.</returns>
    ValueTask<bool> Test(string path, bool log = true, bool? restore = true, bool? build = true,
        string? configuration = "Release", string? verbosity = "normal", string? framework = null,
        string? filter = null, string? logger = null, string? resultsDirectory = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes <c>dotnet pack</c>.
    /// </summary>
    /// <param name="path">Project path.</param>
    /// <param name="version">Package version.</param>
    /// <param name="log">Whether to log execution details.</param>
    /// <param name="configuration">Build configuration.</param>
    /// <param name="build">Whether to build before packing.</param>
    /// <param name="restore">Whether to restore before packing.</param>
    /// <param name="output">Output directory.</param>
    /// <param name="verbosity">CLI verbosity level.</param>
    /// <param name="framework">Target framework.</param>
    /// <param name="runtime">Runtime identifier.</param>
    /// <param name="includeSymbols">Include symbols package.</param>
    /// <param name="includeSource">Include source files.</param>
    /// <param name="serviceable">Marks package as serviceable.</param>
    /// <param name="properties">MSBuild properties.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if successful.</returns>
    ValueTask<bool> Pack(string path, string version, bool log = true, string? configuration = "Release",
        bool? build = false, bool? restore = false, string? output = ".", string? verbosity = "normal",
        string? framework = null, string? runtime = null, bool? includeSymbols = null,
        bool? includeSource = null, bool? serviceable = null,
        IReadOnlyList<KeyValuePair<string, string?>>? properties = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a NuGet package to a project.
    /// </summary>
    ValueTask<bool> AddPackage(string projectPath, string packageId, string? version = null, bool log = true,
        bool? restore = true, string? framework = null, string? source = null, bool prerelease = false,
        string? packageDirectory = null, bool interactive = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a NuGet package from a project.
    /// </summary>
    ValueTask<bool> RemovePackage(string path, string packageId, bool log = true, bool? restore = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cleans build outputs for a project or solution.
    /// </summary>
    ValueTask<bool> Clean(string path, bool log = true, string? configuration = "Release", string? verbosity = "normal",
        string? framework = null, string? runtime = null, string? output = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists NuGet packages for a project.
    /// </summary>
    ValueTask<List<KeyValuePair<string, string>>> ListPackages(string path, bool outdated = false, bool transitive = false,
        bool includePrerelease = false, bool vulnerable = false, bool deprecated = false, bool log = true,
        string? verbosity = "normal", string? framework = null, bool interactive = false, string? source = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates all outdated top-level packages across projects under a path.
    /// </summary>
    ValueTask<bool> UpdatePackages(string path, bool log = true, string? verbosity = "normal",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all project files under a path.
    /// </summary>
    ValueTask<List<string>> GetProjectFiles(string path, CancellationToken cancellationToken = default);
}