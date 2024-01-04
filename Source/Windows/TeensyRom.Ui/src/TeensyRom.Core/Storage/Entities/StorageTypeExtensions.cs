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
        public static StorageItem ToStorageItem(this string path)
        {
            if(string.IsNullOrWhiteSpace(path)) throw new TeensyException("Path is empty");

            var extension = path.GetUnixFileExtension();
            var name = path.GetFileNameFromPath();

            if (string.IsNullOrWhiteSpace(extension)) return new DirectoryItem { Name = name, Path = path }; //TODO: Check this logic later.

            return extension.GetFileType() switch {                 
                TeensyFileType.Sid => new SongItem { Name = name, Path = path},
                TeensyFileType.Crt => new FileItem { Name = name, Path = path },
                TeensyFileType.Prg => new FileItem { Name = name, Path = path },
                TeensyFileType.Hex => new FileItem { Name = name, Path = path },
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
