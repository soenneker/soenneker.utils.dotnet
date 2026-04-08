using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Soenneker.Utils.Dotnet.Dtos;

internal sealed class PackageListReport
{
    [JsonPropertyName("projects")]
    public List<ProjectReport> Projects { get; set; } = [];
}
