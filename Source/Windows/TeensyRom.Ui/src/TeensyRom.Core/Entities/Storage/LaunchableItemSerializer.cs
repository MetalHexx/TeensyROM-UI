using System.Text.Json;

namespace TeensyRom.Core.Entities.Storage
{
    public static class LaunchableItemSerializer
    {
        private static readonly JsonSerializerOptions _options = CreateOptions();

        private static JsonSerializerOptions CreateOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            options.Converters.Add(new LaunchableItemConverter());

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
}
