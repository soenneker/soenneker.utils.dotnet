[![](https://img.shields.io/nuget/v/soenneker.utils.dotnet.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.utils.dotnet/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.dotnet/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.utils.dotnet/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.utils.dotnet.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.utils.dotnet/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.dotnet/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.utils.dotnet/actions/workflows/codeql.yml)

# Soenneker.Utils.Dotnet

DI-friendly wrappers around the `dotnet` CLI for restore, build, run, test, pack, clean, and NuGet package operations.

## Installation

```bash
dotnet add package Soenneker.Utils.Dotnet
```

## Registration

```csharp
builder.Services.AddDotnetUtilAsSingleton();
```

Scoped registration is also available with `AddDotnetUtilAsScoped()`.

## Build workflow

```csharp
bool restored = await dotnet.Restore(solutionPath, cancellationToken: cancellationToken);

bool built = restored && await dotnet.Build(
    solutionPath,
    configuration: "Release",
    restore: false,
    cancellationToken: cancellationToken);

bool packed = built && await dotnet.Pack(
    projectPath,
    version: "1.2.3",
    build: false,
    restore: false,
    output: packageDirectory,
    cancellationToken: cancellationToken);
```

The typed command methods return `false` when the CLI cannot be started or exits unsuccessfully. Cancellation remains cancellation and throws `OperationCanceledException`; it is not converted to `false`.

`Execute(arguments)` accepts the complete raw argument string after `dotnet` and returns captured output. Use it only with arguments assembled from trusted values. Prefer the typed methods when user-controlled paths or options are involved.

## Run an application

`Run()` waits for `dotnet run` to exit:

```csharp
bool succeeded = await dotnet.Run(
    projectPath,
    configuration: "Release",
    applicationArguments: ["--import", inputPath],
    cancellationToken: cancellationToken);
```

`Start()` returns after launching a long-running process:

```csharp
using Process? process = await dotnet.Start(
    projectPath,
    outputCallback: line => logger.LogInformation("{Line}", line),
    errorCallback: line => logger.LogError("{Line}", line),
    cancellationToken: stoppingToken);
```

Cancelling the token supplied to `Start()` kills the launched process tree. The caller owns the returned `Process` and should dispose it. A `null` result means path validation or process startup failed; it does not mean the launched application later exited successfully.

## Package inspection and updates

```csharp
List<KeyValuePair<string, string>> outdated = await dotnet.ListPackages(
    projectPath,
    outdated: true,
    cancellationToken: cancellationToken);

bool updated = await dotnet.UpdatePackages(repositoryPath, cancellationToken: cancellationToken);
```

`UpdatePackages()` finds project files recursively, restores the path, updates each outdated top-level package, and restores again. It edits project files incrementally and does not roll back earlier updates when a later package or final restore fails. Run it in a clean, version-controlled working tree when those mutations need review or recovery.

The utility sets the CLI language to English, disables telemetry, and suppresses the .NET logo so package-list JSON and captured output remain predictable.
