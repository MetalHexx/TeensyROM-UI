using System.Text.Json.Serialization;

namespace TeensyRom.Core.Entities.Storage
{
    public class FileTag 
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}
