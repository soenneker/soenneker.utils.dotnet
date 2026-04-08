using System.Text.Json.Serialization;

namespace Soenneker.Utils.Dotnet.Dtos;

internal sealed class PackageEntry
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("requestedVersion")]
    public string? RequestedVersion { get; set; }

    [JsonPropertyName("resolvedVersion")]
    public string? ResolvedVersion { get; set; }

    [JsonPropertyName("latestVersion")]
    public string? LatestVersion { get; set; }
}
