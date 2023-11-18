using System.IO;
using TeensyRom.Core.Storage.Extensions;

namespace TeensyRom.Core.Storage.Entities
{
    public class TeensyLaunchFileRequest
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