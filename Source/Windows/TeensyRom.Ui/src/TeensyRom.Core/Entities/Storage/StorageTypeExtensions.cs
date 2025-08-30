using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Common;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Entities.Storage
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
        public static FileItem ToFileItem(this FileTransferItem transferItem)
        {
            var fileName = transferItem.TargetPath.FileName;

            return transferItem.Type switch
            {
                TeensyFileType.Sid => new SongItem  { Name = fileName, Title = fileName, Path = transferItem.TargetPath, Size = transferItem.Size },
                TeensyFileType.Crt => new GameItem  { Name = fileName, Title = fileName, Path = transferItem.TargetPath, Size = transferItem.Size },
                TeensyFileType.Prg => new GameItem  { Name = fileName, Title = fileName, Path = transferItem.TargetPath, Size = transferItem.Size },
                TeensyFileType.P00 => new GameItem  { Name = fileName, Title = fileName, Path = transferItem.TargetPath, Size = transferItem.Size },
                TeensyFileType.Hex => new HexItem   { Name = fileName, Title = fileName, Path = transferItem.TargetPath, Size = transferItem.Size },
                TeensyFileType.Kla => new ImageItem { Name = fileName, Title = fileName, Path = transferItem.TargetPath, Size = transferItem.Size },
                TeensyFileType.Koa => new ImageItem { Name = fileName, Title = fileName, Path = transferItem.TargetPath, Size = transferItem.Size },
                TeensyFileType.Art => new ImageItem { Name = fileName, Title = fileName, Path = transferItem.TargetPath, Size = transferItem.Size },
                TeensyFileType.Aas => new ImageItem { Name = fileName, Title = fileName, Path = transferItem.TargetPath, Size = transferItem.Size },
                TeensyFileType.Hpi => new ImageItem { Name = fileName, Title = fileName, Path = transferItem.TargetPath, Size = transferItem.Size },
                TeensyFileType.Seq => new ImageItem { Name = fileName, Title = fileName, Path = transferItem.TargetPath, Size = transferItem.Size },
                TeensyFileType.Txt => new ImageItem { Name = fileName, Title = fileName, Path = transferItem.TargetPath, Size = transferItem.Size },
                
                _ => new FileItem { Name = fileName, Path = transferItem.TargetPath, Size = transferItem.Size },
            };
        }

        public static FileItem MapFileItem(this FileItem fileItem)
        {
            return fileItem.FileType switch
            {
                TeensyFileType.Sid => new SongItem  { Name = fileItem.Name, Title = fileItem.Name, Path = fileItem.Path, Size = fileItem.Size },
                TeensyFileType.Crt => new GameItem  { Name = fileItem.Name, Title = fileItem.Name, Path = fileItem.Path, Size = fileItem.Size },
                TeensyFileType.Prg => new GameItem  { Name = fileItem.Name, Title = fileItem.Name, Path = fileItem.Path, Size = fileItem.Size },
                TeensyFileType.P00 => new GameItem  { Name = fileItem.Name, Title = fileItem.Name, Path = fileItem.Path, Size = fileItem.Size },
                TeensyFileType.Hex => new HexItem   { Name = fileItem.Name, Title = fileItem.Name, Path = fileItem.Path, Size = fileItem.Size },
                TeensyFileType.Kla => new ImageItem { Name = fileItem.Name, Title = fileItem.Name, Path = fileItem.Path, Size = fileItem.Size },
                TeensyFileType.Koa => new ImageItem { Name = fileItem.Name, Title = fileItem.Name, Path = fileItem.Path, Size = fileItem.Size },
                TeensyFileType.Art => new ImageItem { Name = fileItem.Name, Title = fileItem.Name, Path = fileItem.Path, Size = fileItem.Size },
                TeensyFileType.Aas => new ImageItem { Name = fileItem.Name, Title = fileItem.Name, Path = fileItem.Path, Size = fileItem.Size },
                TeensyFileType.Hpi => new ImageItem { Name = fileItem.Name, Title = fileItem.Name, Path = fileItem.Path, Size = fileItem.Size },
                TeensyFileType.Seq => new ImageItem { Name = fileItem.Name, Title = fileItem.Name, Path = fileItem.Path, Size = fileItem.Size },
                TeensyFileType.Txt => new ImageItem { Name = fileItem.Name, Title = fileItem.Name, Path = fileItem.Path, Size = fileItem.Size },
                _ =>                  new FileItem  { Name = fileItem.Name, Title = fileItem.Name, Path = fileItem.Path, Size = fileItem.Size },
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
            ".seq" => TeensyFileType.Seq,
            ".txt" => TeensyFileType.Txt,
            ".d64" => TeensyFileType.D64,
            ".zip" => TeensyFileType.Zip,
            _ => TeensyFileType.Unknown
        };
    }
}
