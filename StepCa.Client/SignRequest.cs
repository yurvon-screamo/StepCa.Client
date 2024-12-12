using System.Text.Json.Serialization;

namespace StepCa.Client;

internal record SignRequest(
    [property: JsonPropertyName("csr")] string Csr,
    [property: JsonPropertyName("ott")] string Ott);
