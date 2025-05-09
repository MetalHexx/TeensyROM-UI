using MediatR;
using System.IO.Ports;
using System.Reactive.Linq;
using System.Transactions;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Commands.GetFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial.Commands.GetFile;
using TeensyRom.Core.Serial.Commands.SaveFiles;
using TeensyRom.Core.Settings;

namespace TeensyRom.Core.Serial
{
    public interface ICartTagger
    {
        Task<Cart> EnsureCartTag(Cart cart);
    }
    public class CartTagger(ILoggingService log, ISerialFactory serialFactory, IMediator mediator) : ICartTagger
    {
        public async Task<Cart> EnsureCartTag(Cart cart)
        {
            var sdResult = await EnsureCartTag(cart.ComPort, cart.SdStorage);
            var usbResult = await EnsureCartTag(cart.ComPort, cart.UsbStorage);

            if (sdResult is null && usbResult is null) return cart;

            cart.DeviceId = sdResult ?? usbResult;

            return cart;
        }

        private async Task<string?> EnsureCartTag(string comPort, CartStorage storage) 
        {
            using var serialPort = serialFactory.Create(comPort);
            serialPort.OpenPort();

            var state = await serialPort.CurrentState.FirstOrDefaultAsync();

            var getFileCommand = new GetFileCommand(storage.Type, "/cart-tag.txt")
            {
                Serial = serialPort
            };
            var getFileResult = await mediator.Send(getFileCommand);

            if (getFileResult.ErrorCode is GetFileErrorCode.StorageUnavailable) 
            {
                log.InternalWarning($"{storage.Type} storage is unavailable.");     
                storage.Available = false;
                return null;
            }

            if (getFileResult.ErrorCode is GetFileErrorCode.FileNotFound)
            {
                log.InternalWarning($"Failed to get remote config file from {storage.Type}");
            }
            else 
            {
                var tagFromTr = getFileResult.FileData.Deserialize<CartTag>();

                if (tagFromTr is not null)
                {
                    storage.Available = true;
                    return tagFromTr.DeviceId!;
                }
            }
            var deviceHash = Guid.NewGuid().ToString().GenerateFilenameSafeHash();

            var newTag = new CartTag { DeviceId = deviceHash };
            var newTagBuffer = newTag.Serialize();

            if (newTagBuffer is null) 
            {
                log.InternalError("Unable to serialize cart config.  Skipping device.");
                storage.Available = false;
                return null;
            }
            var fileTransferItem = new FileTransferItem
            (
                buffer: newTagBuffer,
                name: "cart-tag.txt",
                targetPath: StorageConstants.Remote_Path_Root,
                targetStorage: storage.Type
            );
            var saveFileCommand = new SaveFilesCommand([fileTransferItem])
            {
                Serial = serialPort
            };
            var saveFileResult = await mediator.Send(saveFileCommand);

            if (saveFileResult.IsSuccess is false)
            {
                log.InternalError("Failed to save remote config file");
                return null;
            }
            storage.Available = true;
            return deviceHash;
        }
    }
}
