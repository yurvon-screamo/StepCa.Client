using System.Text.Json;
using System.Text.Json.Serialization;

namespace StepCa.Client;

internal class AotNestedDictionariesConverter : JsonConverter<object>
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (typeToConvert != typeof(object))
        {
            return base.CanConvert(typeToConvert);
        }

        return true;
    }

    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString();
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetInt64();
        }

        if (reader.TokenType == JsonTokenType.True)
        {
            return true;
        }

        if (reader.TokenType == JsonTokenType.False)
        {
            return false;
        }

        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        Type returnType = typeToConvert;

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            returnType = typeof(IDictionary<string, object>);
        }
        else if (reader.TokenType == JsonTokenType.StartArray)
        {
            returnType = typeof(IEnumerable<object>);
        }

        return JsonSerializer.Deserialize(ref reader, returnType, options);
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, typeof(object), options);
    }
}