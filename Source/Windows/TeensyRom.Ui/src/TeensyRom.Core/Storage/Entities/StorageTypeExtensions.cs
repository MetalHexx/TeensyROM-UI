using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial;

namespace TeensyRom.Core.Storage.Entities
{
    public static class StorageFileExtensions
    {
        public static StorageItem ToStorageItem(this TeensyFileInfo fileInfo)
        {
            return fileInfo.Type switch {                 
                TeensyFileType.Sid => new SongItem { Name = fileInfo.Name, Path = fileInfo.TargetPath.UnixPathCombine(fileInfo.Name), Size = fileInfo.Size },
                TeensyFileType.Crt => new FileItem { Name = fileInfo.Name, Path = fileInfo.TargetPath.UnixPathCombine(fileInfo.Name), Size = fileInfo.Size },
                TeensyFileType.Prg => new FileItem { Name = fileInfo.Name, Path = fileInfo.TargetPath.UnixPathCombine(fileInfo.Name), Size = fileInfo.Size },
                TeensyFileType.Hex => new FileItem { Name = fileInfo.Name, Path = fileInfo.TargetPath.UnixPathCombine(fileInfo.Name), Size = fileInfo.Size },
                _ => throw new TeensyException("Unknown file type")
            };
        }
    }
    public static class StorageTypeExtensions
    {
        public static uint GetStorageToken(this TeensyStorageType type)
        {
            return type switch
            {
                TeensyStorageType.SD => TeensyStorageToken.SdCard,
                TeensyStorageType.USB => TeensyStorageToken.UsbStick,
                _ => throw new ArgumentException("Unknown Storage Type")
            };
        }

        public static TeensyFileType GetFileType(this string extension) => extension switch
        {
            ".sid" => TeensyFileType.Sid,
            ".crt" => TeensyFileType.Crt,
            ".prg" => TeensyFileType.Prg,
            ".hex" => TeensyFileType.Hex,
            _ => TeensyFileType.Unknown
        };
    }
}
