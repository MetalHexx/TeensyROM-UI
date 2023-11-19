using System.IO;
using TeensyRom.Core.Common;

namespace TeensyRom.Core.Storage.Entities
{
    public class LaunchFileRequest
    {
        public string TargetPath { get; set; } = string.Empty;
        public TeensyStorageType StorageType { get; set; } = TeensyStorageType.SD;
        public TeensyFileType Type => TargetPath.GetUnixFileExtension() switch
        {
            ".sid" => TeensyFileType.Sid,
            ".crt" => TeensyFileType.Crt,
            ".prg" => TeensyFileType.Prg,
            ".hex" => TeensyFileType.Hex,
            _ => TeensyFileType.Unknown
        };
    }
}