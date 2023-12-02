using System;
using System.Threading.Tasks;

namespace Soenneker.Utils.Dotnet.Abstract;

/// <summary>
/// A utility library for the dotnet executable
/// </summary>
public interface IDotnetUtil
{
    ValueTask Run(string path, string? framework = null, bool log = true, string? configuration = "Release", string? verbosity = "normal", bool? build = true);

    ValueTask Restore(string path, bool log = true, string? configuration = "Release", string? verbosity = "normal");

    ValueTask<bool> Build(string path, bool log = true, string? configuration = "Release", bool? restore = true, string? verbosity = "normal");

    ValueTask<bool> Test(string path, bool log = true, bool? restore = true, string? verbosity = "normal");

    ValueTask<bool> Pack(string path, bool log = true, string? configuration = "Release", bool? build = false, string? output = ".");
}