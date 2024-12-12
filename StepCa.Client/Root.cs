using System.Text.Json.Serialization;

namespace StepCa.Client;

internal record Root(
    [property: JsonPropertyName("crt")] string Crt,
    [property: JsonPropertyName("ca")] string Ca
);
