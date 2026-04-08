using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Soenneker.Utils.Dotnet.Dtos;

internal sealed class FrameworkReport
{
    [JsonPropertyName("framework")]
    public string? Framework { get; set; }

    [JsonPropertyName("topLevelPackages")]
    public List<PackageEntry>? TopLevelPackages { get; set; }

    [JsonPropertyName("transitivePackages")]
    public List<PackageEntry>? TransitivePackages { get; set; }
}
