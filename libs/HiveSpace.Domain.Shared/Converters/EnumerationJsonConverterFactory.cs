using System.Text.Json;
using System.Text.Json.Serialization;
using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.Domain.Shared.Converters;

public class EnumerationJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsClass
           && !typeToConvert.IsAbstract
           && typeToConvert.IsSubclassOf(typeof(Enumeration));

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(EnumerationJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}
