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
    public static class TeensyFileTypeExtensions
    {
        public static TeensyFileType[] GetLaunchFileTypes()
        {
            return Enum.GetValues(typeof(TeensyFileType))
                .Cast<TeensyFileType>()
                .Where(f => f != TeensyFileType.Hex && f != TeensyFileType.Unknown)
                .ToArray();
        }
    }
    public static class StorageFileExtensions
    {
        public static IFileItem ToFileItem(this FileTransferItem fileInfo)
        {
            return fileInfo.Type switch {                 
                TeensyFileType.Sid => new SongItem  { Name = fileInfo.Name, Title = fileInfo.Name, Path = fileInfo.TargetPath.UnixPathCombine(fileInfo.Name), Size = fileInfo.Size },
                TeensyFileType.Crt => new GameItem  { Name = fileInfo.Name, Title = fileInfo.Name, Path = fileInfo.TargetPath.UnixPathCombine(fileInfo.Name), Size = fileInfo.Size },
                TeensyFileType.Prg => new GameItem  { Name = fileInfo.Name, Title = fileInfo.Name, Path = fileInfo.TargetPath.UnixPathCombine(fileInfo.Name), Size = fileInfo.Size },
                TeensyFileType.P00 => new GameItem  { Name = fileInfo.Name, Title = fileInfo.Name, Path = fileInfo.TargetPath.UnixPathCombine(fileInfo.Name), Size = fileInfo.Size },
                TeensyFileType.Hex => new HexItem   { Name = fileInfo.Name, Title = fileInfo.Name, Path = fileInfo.TargetPath.UnixPathCombine(fileInfo.Name), Size = fileInfo.Size },
                TeensyFileType.Kla => new ImageItem { Name = fileInfo.Name, Title = fileInfo.Name, Path = fileInfo.TargetPath.UnixPathCombine(fileInfo.Name), Size = fileInfo.Size },
                TeensyFileType.Koa => new ImageItem { Name = fileInfo.Name, Title = fileInfo.Name, Path = fileInfo.TargetPath.UnixPathCombine(fileInfo.Name), Size = fileInfo.Size },
                TeensyFileType.Art => new ImageItem { Name = fileInfo.Name, Title = fileInfo.Name, Path = fileInfo.TargetPath.UnixPathCombine(fileInfo.Name), Size = fileInfo.Size },
                TeensyFileType.Aas => new ImageItem { Name = fileInfo.Name, Title = fileInfo.Name, Path = fileInfo.TargetPath.UnixPathCombine(fileInfo.Name), Size = fileInfo.Size },
                TeensyFileType.Hpi => new ImageItem { Name = fileInfo.Name, Title = fileInfo.Name, Path = fileInfo.TargetPath.UnixPathCombine(fileInfo.Name), Size = fileInfo.Size },

                _ => new FileItem { Name = fileInfo.Name, Path = fileInfo.TargetPath.UnixPathCombine(fileInfo.Name), Size = fileInfo.Size },
            };
        }

        public static IFileItem MapFileItem(this IFileItem fileItem)
        {
            return fileItem.FileType switch
            {
                TeensyFileType.Sid => new SongItem   { Name = fileItem.Name, Title = fileItem.Name, Path = fileItem.Path, Size = fileItem.Size },
                TeensyFileType.Crt => new GameItem   { Name = fileItem.Name, Title = fileItem.Name, Path = fileItem.Path, Size = fileItem.Size },
                TeensyFileType.Prg => new GameItem   { Name = fileItem.Name, Title = fileItem.Name, Path = fileItem.Path, Size = fileItem.Size },
                TeensyFileType.P00 => new GameItem   { Name = fileItem.Name, Title = fileItem.Name, Path = fileItem.Path, Size = fileItem.Size },
                TeensyFileType.Hex => new HexItem    { Name = fileItem.Name, Title = fileItem.Name, Path = fileItem.Path, Size = fileItem.Size },
                TeensyFileType.Kla => new ImageItem  { Name = fileItem.Name, Title = fileItem.Name, Path = fileItem.Path, Size = fileItem.Size },
                TeensyFileType.Koa => new ImageItem  { Name = fileItem.Name, Title = fileItem.Name, Path = fileItem.Path, Size = fileItem.Size },
                TeensyFileType.Art => new ImageItem  { Name = fileItem.Name, Title = fileItem.Name, Path = fileItem.Path, Size = fileItem.Size },
                TeensyFileType.Aas => new ImageItem  { Name = fileItem.Name, Title = fileItem.Name, Path = fileItem.Path, Size = fileItem.Size },
                TeensyFileType.Hpi => new ImageItem  { Name = fileItem.Name, Title = fileItem.Name, Path = fileItem.Path, Size = fileItem.Size },
                                 _ => new FileItem   { Name = fileItem.Name, Title = fileItem.Name, Path = fileItem.Path, Size = fileItem.Size },
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

        public static TeensyFileType GetFileType(this string extension) => extension.ToLower() switch
        {
            ".sid" => TeensyFileType.Sid,
            ".crt" => TeensyFileType.Crt,
            ".prg" => TeensyFileType.Prg,
            ".p00" => TeensyFileType.P00,
            ".hex" => TeensyFileType.Hex,
            ".kla" => TeensyFileType.Kla,
            ".koa" => TeensyFileType.Koa,
            ".art" => TeensyFileType.Art,
            ".aas" => TeensyFileType.Aas,
            ".hpi" => TeensyFileType.Hpi,
            ".d64" => TeensyFileType.D64,
            _ => TeensyFileType.Unknown
        };
    }
}
