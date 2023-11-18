namespace TeensyRom.Core.Storage.Entities
{
    public class TeensyLaunchFileRequest
    {
        public string TargetPath { get; set; } = string.Empty;
        public TeensyStorageType StorageType { get; set; } = TeensyStorageType.SD;
    }
}