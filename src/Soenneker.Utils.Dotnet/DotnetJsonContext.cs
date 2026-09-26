using System.Text.Json.Serialization;
using Soenneker.Utils.Dotnet.Dtos;

namespace Soenneker.Utils.Dotnet;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(PackageListReport))]
internal partial class DotnetJsonContext : JsonSerializerContext;
