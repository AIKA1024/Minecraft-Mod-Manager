using NetCore.Models;
using System.Text.Json.Serialization;
using MAZDA_MCTool.Abstraction.Contracts.Models;
using MAZDA_MCTool.Models;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using System;
using System.Collections.Generic;

namespace MAZDA_MCTool.Serialization;
[JsonSerializable(typeof(Setting))]
[JsonSerializable(typeof(ModInfo))]
[JsonSerializable(typeof(List<SynchronizationInfo>))]
public partial class JsonContext : JsonSerializerContext;

