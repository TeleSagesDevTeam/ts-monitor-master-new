using System.Text.Json;
using System.Text.Json.Serialization;

namespace ContractIndexer.Json;

public class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return !DateTimeOffset.TryParse(value, out var dt) 
            ? throw new JsonException("DateTimeOffset could not be parsed") 
            : dt;
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}