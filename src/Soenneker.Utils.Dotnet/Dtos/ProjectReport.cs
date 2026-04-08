using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Soenneker.Utils.Dotnet.Dtos;

internal sealed class ProjectReport
{
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("frameworks")]
    public List<FrameworkReport> Frameworks { get; set; } = [];
}
