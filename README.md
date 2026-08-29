[![](https://img.shields.io/nuget/v/soenneker.utils.dotnet.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.utils.dotnet/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.dotnet/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.utils.dotnet/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.utils.dotnet.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.utils.dotnet/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.dotnet/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.utils.dotnet/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Utils.Dotnet
A utility library for the dotnet executable.

## Installation

```bash
dotnet add package Soenneker.Utils.Dotnet
```

## Quick start

```csharp
using Soenneker.Utils.Dotnet.Registrars;

services.AddDotnetUtilAsSingleton();
```

Then inject `IDotnetUtil` wherever you need it.

## Common operations

- `Execute()` - Executes a raw `dotnet` CLI command and returns the combined output.
- `Run()` - Executes `dotnet run` for a project or directory. Returns `true` if the command succeeded; otherwise `false`.
- `Start()` - Starts `dotnet run` without waiting for the target process to exit. Intended for long-running apps such as local web servers used by integration tests. Returns the started process, or `null` if launch validation or startup failed.
- `Restore()` - Executes `dotnet restore`. Returns `true` if successful.
- `Build()` - Executes `dotnet build`. Returns `true` if successful.
- `Test()` - Executes `dotnet test`. Returns `true` if successful.
- `Pack()` - Executes `dotnet pack`. Returns `true` if successful.
- `RemovePackage()` - Removes a NuGet package from a project.
- `AddPackage()` - Adds a NuGet package to a project.
- `UpdatePackages()` - Updates all outdated top-level packages across projects under a path.
- `Clean()` - Cleans build outputs for a project or solution.
- `ListPackages()` - Lists NuGet packages for a project.

The package also includes one additional operation for more specialized cases.
