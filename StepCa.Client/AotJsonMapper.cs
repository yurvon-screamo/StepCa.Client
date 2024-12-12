using System.Text.Encodings.Web;
using System.Text.Json;

using Jose;

using StepCaClient;

namespace StepCa.Client;

internal class AotJsonMapper : IJsonMapper
{
    private readonly JsonSerializerOptions SerializeOptions;

    private readonly JsonSerializerOptions DeserializeOptions;

    public AotJsonMapper()
    {
        SerializeOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = SourceGenerationContext.Default,
        };
        DeserializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = SourceGenerationContext.Default,
        };
        DeserializeOptions.Converters.Add(new AotNestedDictionariesConverter());
    }

    public string Serialize(object obj)
    {
        return JsonSerializer.Serialize(obj, SerializeOptions);
    }

    public T? Parse<T>(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return default;
        }

        _ = typeof(T) == typeof(IDictionary<string, object>);

        return JsonSerializer.Deserialize<T>(json, DeserializeOptions);
    }
}
