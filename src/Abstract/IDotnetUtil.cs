using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Utils.Dotnet.Abstract;

/// <summary>
/// Provides methods for executing .NET CLI commands programmatically.
/// This includes commands like run, restore, build, test, pack, and package management.
/// </summary>
public interface IDotnetUtil
{
    /// <summary>
    /// Executes the 'dotnet run' command to run a specified project or solution.
    /// </summary>
    /// <param name="path">The path to the project or solution to run.</param>
    /// <param name="framework">The target framework to run (optional).</param>
    /// <param name="log">Indicates whether to log the command execution details.</param>
    /// <param name="configuration">The build configuration (e.g., Debug or Release). Default is "Release".</param>
    /// <param name="verbosity">The verbosity level for the output (e.g., quiet, normal, detailed).</param>
    /// <param name="build">Indicates whether to build the project before running. Default is true.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Returns true if the command executes successfully; otherwise, false.</returns>
    ValueTask<bool> Run(string path, string? framework = null, bool log = true, string? configuration = "Release", string? verbosity = "normal", bool? build = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the 'dotnet restore' command to restore project dependencies and tools.
    /// </summary>
    /// <param name="path">The path to the project or solution file to restore.</param>
    /// <param name="log">Indicates whether to log the command execution details.</param>
    /// <param name="verbosity">The verbosity level for the output (e.g., quiet, normal, detailed).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Returns true if the restore completes successfully; otherwise, false.</returns>
    ValueTask<bool> Restore(string path, bool log = true, string? verbosity = "normal", CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the 'dotnet build' command to build a specified project or solution.
    /// </summary>
    /// <param name="path">The path to the project or solution file to build.</param>
    /// <param name="log">Indicates whether to log the command execution details.</param>
    /// <param name="configuration">The build configuration (e.g., Debug or Release). Default is "Release".</param>
    /// <param name="restore">Indicates whether to restore dependencies before building. Default is true.</param>
    /// <param name="verbosity">The verbosity level for the output (e.g., quiet, normal, detailed).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Returns true if the build completes successfully with no errors; otherwise, false.</returns>
    ValueTask<bool> Build(string path, bool log = true, string? configuration = "Release", bool? restore = true, string? verbosity = "normal", CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the 'dotnet test' command to run unit tests for a specified project.
    /// </summary>
    /// <param name="path">The path to the project or solution containing tests.</param>
    /// <param name="log">Indicates whether to log the command execution details.</param>
    /// <param name="restore">Indicates whether to restore dependencies before running tests. Default is true.</param>
    /// <param name="verbosity">The verbosity level for the output (e.g., quiet, normal, detailed).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Returns true if all tests pass successfully; otherwise, false.</returns>
    ValueTask<bool> Test(string path, bool log = true, bool? restore = true, string? verbosity = "normal", CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the 'dotnet pack' command to create a NuGet package for a specified project.
    /// </summary>
    /// <param name="path">The path to the project file to pack.</param>
    /// <param name="version">The version number for the package.</param>
    /// <param name="log">Indicates whether to log the command execution details.</param>
    /// <param name="configuration">The build configuration (e.g., Debug or Release). Default is "Release".</param>
    /// <param name="build">Indicates whether to build the project before packing. Default is false.</param>
    /// <param name="restore">Indicates whether to restore dependencies before packing. Default is false.</param>
    /// <param name="output">The output directory for the generated package.</param>
    /// <param name="verbosity">The verbosity level for the output (e.g., quiet, normal, detailed).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Returns true if the pack operation completes successfully; otherwise, false.</returns>
    ValueTask<bool> Pack(string path, string version, bool log = true, string? configuration = "Release", bool? build = false, bool? restore = false, string? output = ".",
        string? verbosity = "normal", CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the 'dotnet remove package' command to remove a specified package from a project.
    /// </summary>
    /// <param name="path">The path to the project file.</param>
    /// <param name="packageId">The ID of the NuGet package to remove.</param>
    /// <param name="log">Indicates whether to log the command execution details.</param>
    /// <param name="restore">Indicates whether to restore dependencies after removing the package. Default is true.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Returns true if the package is removed successfully; otherwise, false.</returns>
    ValueTask<bool> RemovePackage(string path, string packageId, bool log = true, bool? restore = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the 'dotnet add package' command to add a specified NuGet package to a project.
    /// </summary>
    /// <param name="projectPath">The path to the project file.</param>
    /// <param name="packageId">The ID of the NuGet package to add.</param>
    /// <param name="version">The version of the package to add (optional).</param>
    /// <param name="log">Indicates whether to log the command execution details.</param>
    /// <param name="restore">Indicates whether to restore dependencies after adding the package. Default is true.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Returns true if the package is added successfully; otherwise, false.</returns>
    ValueTask<bool> AddPackage(string projectPath, string packageId, string? version = null, bool log = true, bool? restore = true, CancellationToken cancellationToken = default);

    ValueTask<bool> Clean(string path, bool log = true, string? configuration = "Release", string? verbosity = "normal", CancellationToken cancellationToken = default);

    ValueTask<List<KeyValuePair<string, string>>> ListPackages(string path, bool outdated = false, bool transitive = false, bool includePrerelease = false, bool vulnerable = false,
        bool deprecated = false, bool log = true, string? verbosity = "normal", CancellationToken cancellationToken = default);

    ValueTask<bool> UpdatePackages(string path, bool log = true, string? verbosity = "normal", CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a generic 'dotnet' command with custom arguments and success criteria.
    /// </summary>
    /// <param name="command">The dotnet command to execute (e.g., add, build, test).</param>
    /// <param name="projectPath">The path to the project or solution file.</param>
    /// <param name="argumentBuilder">A function to build the arguments for the command.</param>
    /// <param name="successCriteria">A function to evaluate whether the command output indicates success.</param>
    /// <param name="failureCriteria"></param>
    /// <param name="log">Indicates whether to log the command execution details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Returns true if the command executes successfully based on the provided criteria; otherwise, false.</returns>
    ValueTask<bool> ExecuteCommand(string command, string projectPath, Func<string, string> argumentBuilder,
        Func<string, bool> successCriteria, Func<string, bool>? failureCriteria = null, bool log = true, CancellationToken cancellationToken = default);

    ValueTask<List<string>> ExecuteCommandWithOutput(string command, string projectPath, Func<string, string> argumentBuilder, bool log = true, CancellationToken cancellationToken = default);

    ValueTask<string> Execute(string arguments, CancellationToken cancellationToken = default);

    /// <summary>
    /// Convenience helper that gives you **both** the direct and transitive
    /// dependency sets using the fast JSON route (no NuGet.org traffic).
    /// You’re free to use it elsewhere if you like.
    /// </summary>
    ValueTask<(List<KeyValuePair<string, string>> Direct, HashSet<string> Transitive)> GetDependencySetsLocal(string csproj, CancellationToken cancellationToken = default);
}