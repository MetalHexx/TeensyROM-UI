using System.Text.Json.Serialization;
using System.Text.Json;

namespace TeensyRom.Core.Entities.Storage
{
    public class LaunchableItemConverter : JsonConverter<LaunchableItem>
    {
        public override LaunchableItem? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            if (!root.TryGetProperty("Type", out var typeProperty))
                throw new JsonException("Missing discriminator 'Type'");

            var typeDiscriminator = typeProperty.GetString();

            return typeDiscriminator switch
            {
                "Song" => root.Deserialize<SongItem>(options),
                "Game" => root.Deserialize<GameItem>(options),
                "Hex" => root.Deserialize<HexItem>(options),
                "Image" => root.Deserialize<ImageItem>(options),
                _ => throw new JsonException($"Unknown type discriminator: {typeDiscriminator}")
            };
        }

        public override void Write(Utf8JsonWriter writer, LaunchableItem value, JsonSerializerOptions options)
        {
            var type = value switch
            {
                SongItem => "Song",
                GameItem => "Game",
                HexItem => "Hex",
                ImageItem => "Image",
                _ => throw new JsonException($"Unsupported type: {value.GetType()}")
            };

            using var jsonDoc = JsonDocument.Parse(JsonSerializer.Serialize(value, value.GetType(), options));
            writer.WriteStartObject();
            writer.WriteString("Type", type);
            foreach (var prop in jsonDoc.RootElement.EnumerateObject())
            {
                prop.WriteTo(writer);
            }
            writer.WriteEndObject();
        }
    }
}
