using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Utils.Dotnet.Abstract;

/// <summary>
/// Provides utilities for executing <c>dotnet</c> CLI commands and working with .NET projects,
/// including build operations, package management, and dependency inspection.
/// </summary>
public interface IDotnetUtil
{
    /// <summary>
    /// Executes a raw <c>dotnet</c> CLI command and returns the combined standard output as a single string.
    /// </summary>
    /// <param name="arguments">The arguments to pass to the <c>dotnet</c> CLI.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The combined standard output.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the process exits with a non-zero code.</exception>
    ValueTask<string> Execute(string arguments, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves direct and transitive package dependencies for a given project using <c>dotnet package list</c> JSON output.
    /// </summary>
    /// <param name="csproj">The path to the project file.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item><description>Direct dependencies (package ID and resolved version).</description></item>
    /// <item><description>Transitive dependency package IDs.</description></item>
    /// </list>
    /// </returns>
    ValueTask<(List<KeyValuePair<string, string>> Direct, HashSet<string> Transitive)> GetDependencySetsLocal(string csproj,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes <c>dotnet run</c> for a project.
    /// </summary>
    /// <param name="path">The project or solution path.</param>
    /// <param name="framework">Optional target framework.</param>
    /// <param name="log">Whether to log command execution.</param>
    /// <param name="configuration">Build configuration (e.g., Release, Debug).</param>
    /// <param name="verbosity">CLI verbosity level.</param>
    /// <param name="build">Whether to build before running.</param>
    /// <param name="urls">Optional ASP.NET Core URLs passed through <c>--urls</c>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><c>true</c> if execution succeeds; otherwise <c>false</c>.</returns>
    ValueTask<bool> Run(string path, string? framework = null, bool log = true, string? configuration = "Release", string? verbosity = "normal",
        bool? build = true, string? urls = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes <c>dotnet restore</c>.
    /// </summary>
    ValueTask<bool> Restore(string path, bool log = true, string? verbosity = "normal", CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes <c>dotnet build</c>.
    /// </summary>
    ValueTask<bool> Build(string path, bool log = true, string? configuration = "Release", bool? restore = true, string? verbosity = "normal",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes <c>dotnet test</c>.
    /// </summary>
    ValueTask<bool> Test(string path, bool log = true, bool? restore = true, string? verbosity = "normal", CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes <c>dotnet pack</c>.
    /// </summary>
    ValueTask<bool> Pack(string path, string version, bool log = true, string? configuration = "Release", bool? build = false, bool? restore = false,
        string? output = ".", string? verbosity = "normal", CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes <c>dotnet clean</c>.
    /// </summary>
    ValueTask<bool> Clean(string path, bool log = true, string? configuration = "Release", string? verbosity = "normal",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a package reference from a project using <c>dotnet remove package</c>.
    /// </summary>
    ValueTask<bool> RemovePackage(string path, string packageId, bool log = true, bool? restore = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds or updates a package reference using <c>dotnet add package</c>.
    /// </summary>
    /// <param name="projectPath">The project file path.</param>
    /// <param name="packageId">The package ID.</param>
    /// <param name="version">Optional version. If null, resolves to latest available.</param>
    /// <param name="log">Whether to log command execution.</param>
    /// <param name="restore">Whether to restore after adding.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    ValueTask<bool> AddPackage(string projectPath, string packageId, string? version = null, bool log = true, bool? restore = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates all outdated top-level packages across one or more projects.
    /// </summary>
    /// <remarks>
    /// This method:
    /// <list type="number">
    /// <item><description>Restores the solution or project.</description></item>
    /// <item><description>Identifies outdated packages using JSON output.</description></item>
    /// <item><description>Updates each package to its latest version.</description></item>
    /// <item><description>Performs a final restore.</description></item>
    /// </list>
    /// </remarks>
    ValueTask<bool> UpdatePackages(string path, bool log = true, string? verbosity = "normal", CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists packages for a project using <c>dotnet package list</c> JSON output.
    /// </summary>
    /// <param name="path">Project or solution path.</param>
    /// <param name="outdated">Whether to include only outdated packages.</param>
    /// <param name="transitive">Whether to include transitive dependencies.</param>
    /// <param name="includePrerelease">Whether to include prerelease versions.</param>
    /// <param name="vulnerable">Whether to include vulnerable packages.</param>
    /// <param name="deprecated">Whether to include deprecated packages.</param>
    /// <param name="log">Whether to log command execution.</param>
    /// <param name="verbosity">CLI verbosity level.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A list of package ID and version pairs.</returns>
    ValueTask<List<KeyValuePair<string, string>>> ListPackages(string path, bool outdated = false, bool transitive = false, bool includePrerelease = false,
        bool vulnerable = false, bool deprecated = false, bool log = true, string? verbosity = "normal", CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all project files (<c>.csproj</c>) from a path.
    /// </summary>
    /// <param name="path">A project file or directory.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A list of project file paths.</returns>
    ValueTask<List<string>> GetProjectFiles(string path, CancellationToken cancellationToken);
}