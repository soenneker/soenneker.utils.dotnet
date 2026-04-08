namespace Soenneker.Utils.Dotnet.Dtos;

internal sealed record PackageUpdateCandidate(string PackageId, string ResolvedVersion, string LatestVersion);
