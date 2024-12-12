using System.Text.Json.Serialization;

namespace StepCa.Client;

internal record Provisioner([property: JsonPropertyName("name")] string Name, [property: JsonPropertyName("encryptedKey")] string EncryptedKey);
internal record ProvisionerList([property: JsonPropertyName("provisioners")] Provisioner[] Provisioners);
