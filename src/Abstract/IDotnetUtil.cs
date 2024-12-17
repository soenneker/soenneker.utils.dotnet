using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Utils.Dotnet.Abstract;

/// <summary>
/// A utility library for the dotnet executable
/// </summary>
public interface IDotnetUtil
{
    ValueTask Run(string path, string? framework = null, bool log = true, string? configuration = "Release", string? verbosity = "normal", bool? build = true, CancellationToken cancellationToken = default);

    ValueTask Restore(string path, bool log = true, string? verbosity = "normal", CancellationToken cancellationToken = default);

    ValueTask<bool> Build(string path, bool log = true, string? configuration = "Release", bool? restore = true, string? verbosity = "normal", CancellationToken cancellationToken = default);

    ValueTask<bool> Test(string path, bool log = true, bool? restore = true, string? verbosity = "normal", CancellationToken cancellationToken = default);

    ValueTask<bool> Pack(string path, string version, bool log = true, string? configuration = "Release", bool? build = false, bool? restore = false, string? output = ".", string? verbosity = "normal", CancellationToken cancellationToken = default);

    ValueTask<bool> Remove(string path, string packageName, bool log = true, bool? restore = true, string? verbosity = "normal", CancellationToken cancellationToken = default);
}