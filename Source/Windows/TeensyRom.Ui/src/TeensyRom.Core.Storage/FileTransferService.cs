using MediatR;
using MediatR.NotificationPublishers;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Commands.MuteSidVoices;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Games;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Music;
using TeensyRom.Core.Storage.Tools.D64Extraction;
using TeensyRom.Core.Storage.Tools.Zip;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Storage
{
    public interface IFileTransferService
    {
        Task Send(List<FileTransferItem> transferItems);
    }

    public class FileTransferService(
        IZipExtractor zipExtractor, 
        ID64Extractor d64Extractor, 
        IMediator mediator, 
        ICachedStorageService storage, 
        ISidMetadataService sidMetadata,
        IGameMetadataService gameMetadata,
        IAlertService alert, 
        ILoggingService log) : IFileTransferService
    {
        private const int _retryLimit = 3;

        public async Task Send(List<FileTransferItem> transferItems)
        {
            var processedFiles = transferItems
                .SelectMany(ProcessFile)
                .ToList();

            var result = await Transfer(processedFiles);
            LogResults(result);

            if (result.SuccessfulFiles.Count > 0) 
            {
                var mappedFiles = MapFiles(result.SuccessfulFiles);
                storage.SaveFiles(mappedFiles);
            }
        }

        private List<FileTransferItem> ProcessFile(FileTransferItem transferItem)
        {
            var fileType = transferItem.FullSourcePath.GetFileExtension().GetFileType();

            return fileType switch
            {
                TeensyFileType.Zip => ProcessZip(transferItem),
                TeensyFileType.D64 => ProcessD64(transferItem),
                _ => [transferItem]
            };
        }

        private List<FileTransferItem> ProcessZip(FileTransferItem transferItem)
        {
            log.Internal($"Extracting Zip: {transferItem.FullSourcePath}");

            List<FileTransferItem> processedFiles = [];

            var extractionResult = zipExtractor.Extract(transferItem);

            foreach (var file in extractionResult.ExtractedFiles) 
            {
                var zipFileName = transferItem.TargetPath.FileName;

                var fileNameIndex = file.FullName
                    .IndexOf(zipFileName, StringComparison.OrdinalIgnoreCase);

                var pathAfterZip = file.FullName
                    .Substring(fileNameIndex + zipFileName.Length)
                    .ToUnixPath();

                var newTargetPath = transferItem.TargetPath.Directory.Combine(new FilePath(pathAfterZip));

                var fileTransferItem = new FileTransferItem(file, newTargetPath, transferItem.TargetStorage);

                processedFiles.AddRange(ProcessFile(fileTransferItem));
            }
            return processedFiles;
        }

        private List<FileTransferItem> ProcessD64(FileTransferItem transferItem)
        {
            log.Internal($"Extracting D64: {transferItem.FullSourcePath}");
            List<FileTransferItem> processedFiles = [];

            var extractionResult = d64Extractor.Extract(transferItem);

            foreach (var file in extractionResult.ExtractedFiles)
            {
                var d64TargetPath = new DirectoryPath(transferItem.TargetPath.Value)
                    .Combine(new FilePath(file.Name));

                var fileTransferItem = new FileTransferItem(file, d64TargetPath, transferItem.TargetStorage);

                processedFiles.Add(fileTransferItem);
            }
            return processedFiles;
        }

        private async Task<SaveFilesResult> Transfer(List<FileTransferItem> transferItems) 
        {
            var result = new SaveFilesResult
            {
                SuccessfulFiles = [],
                FailedFiles = []
            };
            
            var filesToTransfer = new List<FileTransferItem>(transferItems);
            
            await mediator.Send(new ResetCommand());
            
            for (int attempt = 0; attempt < _retryLimit + 1; attempt++)
            {
                if (filesToTransfer.Count == 0)
                    break;
                
                if (attempt > 0)
                {
                    log.InternalError($"Retrying {filesToTransfer.Count} previously failed file(s)...");
                }
                
                var attemptResult = await mediator.Send(new SaveFilesCommand(filesToTransfer));
                
                result.SuccessfulFiles.AddRange(attemptResult.SuccessfulFiles);
                
                filesToTransfer = [.. attemptResult.FailedFiles];
                
                if (filesToTransfer.Count == 0)
                    break;
            }
            
            foreach (var file in filesToTransfer)
            {
                log.InternalError($"Failed to transfer file after {_retryLimit} attempts: {file.TargetPath.Value}");
            }
            
            result.FailedFiles = filesToTransfer;
            
            return result;
        }

        private List<FileItem> MapFiles(List<FileTransferItem> transferredFiles)
        {
            List<FileItem> mappedFiles = [];
            
            foreach (var f in transferredFiles)
            {
                var storageItem = f.ToFileItem();

                if (storageItem is SongItem song) sidMetadata.EnrichSong(song);
                if (storageItem is GameItem game) gameMetadata.EnrichGame(game);
                if (storageItem is HexItem hex) HexItem.MapHexItem(hex);


                if (storageItem is FileItem file)
                {
                    mappedFiles.Add(file);
                }
            }
            return mappedFiles;
        }

        private void LogResults(SaveFilesResult saveResults)
        {
            if (saveResults.FailedFiles.Any())
            {
                alert.Publish($"{saveResults.FailedFiles.Count} files had an error being copied. \r\n See Logs.");
                saveResults.FailedFiles.ForEach(d => log.InternalError($"Error copying file: {d.FullSourcePath}"));
            }
            alert.Publish($"{saveResults.SuccessfulFiles.Count} files were saved to the TR.");
        }
    }
}
