using MediatR;
using System.IO.Ports;
using System.Reactive.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Transactions;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Commands.GetFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Device;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.Commands.GetFile;
using TeensyRom.Core.Serial.Commands.SaveFiles;
using TeensyRom.Core.Settings;

namespace TeensyRom.Core.Device
{
    public interface ICartTagger
    {
        Task<CartStorage> EnsureTag(ISerialStateContext serial, TeensyStorageType storageType);
    }
    public class CartTagger(ILoggingService log, IMediator mediator) : ICartTagger
    {
        public async Task<CartStorage> EnsureTag(ISerialStateContext serial, TeensyStorageType storageType)
        {
            var pingResult = await mediator.Send(new PingCommand 
            {
                Serial = serial
            });
            if (pingResult.IsBusy) 
            {
                await mediator.Send(new ResetCommand
                {
                    Serial = serial
                });
            }
            var getFileCommand = new GetFileCommand(storageType, "/cart-tag.txt")
            {
                Serial = serial
            };
            var getFileResult = await mediator.Send(getFileCommand);

            if (getFileResult.ErrorCode is GetFileErrorCode.StorageUnavailable)
            {
                log.InternalWarning($"{storageType} storage is unavailable.");

                return new CartStorage
                {
                    Available = false,
                    Type = storageType
                };
            }
            if (getFileResult.ErrorCode is GetFileErrorCode.FileNotFound)
            {
                log.InternalWarning($"Failed to get remote config file from {storageType}");
            }
            else
            {
                var tagFromTr = getFileResult.FileData.Deserialize<CartTag>();

                if (tagFromTr is not null)
                {
                    return new CartStorage
                    {
                        Available = true,
                        Type = storageType,
                        DeviceId = tagFromTr.DeviceId
                    };
                }
            }
            var deviceHash = Guid.NewGuid().ToString().GenerateFilenameSafeHash();

            var newTag = new CartTag { DeviceId = deviceHash };
            var newTagBuffer = newTag.Serialize();

            if (newTagBuffer is null)
            {
                log.InternalError("Unable to serialize cart config.  Skipping device.");
                return new CartStorage
                {
                    Available = false,
                    Type = storageType
                };
            }
            var fileTransferItem = new FileTransferItem
            (
                buffer: newTagBuffer,
                name: "cart-tag.txt",
                targetPath: StorageHelper.Remote_Path_Root,
                targetStorage: storageType
            );
            var saveFileCommand = new SaveFilesCommand([fileTransferItem])
            {
                Serial = serial
            };
            var saveFileResult = await mediator.Send(saveFileCommand);

            if (saveFileResult.IsSuccess is false)
            {
                log.InternalError("Failed to save remote config file");
                return new CartStorage
                {
                    Available = false,
                    Type = storageType
                };
            }
            return new CartStorage
            {
                Available = true,
                Type = storageType,
                DeviceId = deviceHash
            };
        }
    }
}
