using System.Text.Json.Serialization;

namespace NetCore.Models;

public class HashsRequest
{
  [JsonPropertyName("hashes")]
  public IEnumerable<string> Hashes { get; set; } =[];
  [JsonPropertyName("algorithm")]
  public string Algorithm { get; set; } = "sha1";
}