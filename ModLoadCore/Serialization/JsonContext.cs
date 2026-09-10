using ModLoadCore.Models;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ModLoadCore.Serialization;
[JsonSerializable(typeof(TomlRootModel))]
[JsonSerializable(typeof(ForgeTomlEntry))]
[JsonSerializable(typeof(TomlDependencyEntry))]
public partial class JsonContext : JsonSerializerContext;