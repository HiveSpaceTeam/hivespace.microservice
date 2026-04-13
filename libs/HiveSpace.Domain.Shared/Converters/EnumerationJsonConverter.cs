using System.Text.Json;
using System.Text.Json.Serialization;
using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.Domain.Shared.Converters;

public class EnumerationJsonConverter<T> : JsonConverter<T> where T : Enumeration
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
            throw new JsonException($"Expected a number for {typeToConvert.Name}, got {reader.TokenType}.");

        var id = reader.GetInt32();
        return Enumeration.FromValue<T>(id);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value.Id);
}

