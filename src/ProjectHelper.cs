using System;
using System.Collections.Generic;
using System.IO;

namespace Soenneker.Utils.Dotnet;

internal static class ProjectHelper
{
    internal static List<string> GetProjectFiles(string path)
    {
        var projectFiles = new List<string>();

        if (File.Exists(path) && path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
        {
            // Path is a single project file
            projectFiles.Add(path);
        }
        else if (Directory.Exists(path))
        {
            // Path is a directory; search for all .csproj files
            projectFiles.AddRange(Directory.GetFiles(path, "*.csproj", SearchOption.AllDirectories));
        }

        return projectFiles;
    }
}