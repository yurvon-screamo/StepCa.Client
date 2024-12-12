using StepCa.Client;

using System.Text.Json.Serialization;

namespace StepCaClient;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(IDictionary<string, object>))]
[JsonSerializable(typeof(SignRequest))]
[JsonSerializable(typeof(CaJson))]
[JsonSerializable(typeof(ProvisionerList))]
[JsonSerializable(typeof(Root))]
internal partial class SourceGenerationContext : JsonSerializerContext;
