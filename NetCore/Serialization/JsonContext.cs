using NetCore.Models;
using System.Text.Json.Serialization;

namespace NetCore.Serialization;
[JsonSerializable(typeof(HashsRequest))]
[JsonSerializable(typeof(CurseForgeProjectRequest))]
[JsonSerializable(typeof(FingerprintRequest))]
[JsonSerializable(typeof(List<string>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class JsonContext : JsonSerializerContext;
