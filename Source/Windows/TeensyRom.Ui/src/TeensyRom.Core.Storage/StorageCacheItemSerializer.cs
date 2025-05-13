using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TeensyRom.Core.Storage
{
    public static class StorageCacheItemSerializer
    {
        private static readonly JsonSerializerOptions _options = CreateOptions();

        private static JsonSerializerOptions CreateOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            options.Converters.Add(new StorageCacheItemConverter());
            return options;
        }

        public static string Serialize<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, _options);
        }

        public static T? Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, _options);
        }

        public static T? Deserialize<T>(byte[] jsonBytes)
        {
            return JsonSerializer.Deserialize<T>(jsonBytes, _options);
        }

        public static JsonSerializerOptions Options => _options;
    }

    public class StorageCacheItemConverter : JsonConverter<IStorageCacheItem>
    {
        public override IStorageCacheItem? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<StorageCacheItem>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, IStorageCacheItem value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (StorageCacheItem)value, options);
        }
    }
}
