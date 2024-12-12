using System.Text.Json.Serialization;

namespace StepCa.Client;

internal record CaJson([property: JsonPropertyName("ca")] string Ca);
