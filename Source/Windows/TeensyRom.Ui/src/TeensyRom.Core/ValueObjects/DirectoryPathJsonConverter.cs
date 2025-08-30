namespace TeensyRom.Core.ValueObjects
{
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class DirectoryPathJsonConverter : JsonConverter<DirectoryPath>
    {
        public override DirectoryPath Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var pathString = reader.GetString();
            return pathString is null ? new DirectoryPath(string.Empty) : new DirectoryPath(pathString);
        }

        public override void Write(Utf8JsonWriter writer, DirectoryPath value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    public class FilePathJsonConverter : JsonConverter<FilePath>
    {
        public override FilePath Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var pathString = reader.GetString();
            return pathString is null ? new FilePath(string.Empty) : new FilePath(pathString);
        }

        public override void Write(Utf8JsonWriter writer, FilePath value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
