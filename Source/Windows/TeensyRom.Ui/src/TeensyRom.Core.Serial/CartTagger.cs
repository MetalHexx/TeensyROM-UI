using MediatR;
using System.IO.Ports;
using System.Reactive.Linq;
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
            using var serialPort = serialFactory.Create(cart.ComPort);
            serialPort.OpenPort();

            var state = await serialPort.CurrentState.FirstOrDefaultAsync();
            
            var getFileCommand = new GetFileCommand(TeensyStorageType.SD, "/remote-config.txt")
            {
                Serial = serialPort
            };
            var getFileResult = await mediator.Send(getFileCommand);

            if (getFileResult.IsSuccess is false) 
            {
                log.InternalError("Failed to get remote config file");
                throw new TeensyException("Failed to get remote config file");
            }
            var cartFromTr = getFileResult.FileData.Deserialize<Cart>();

            if (cartFromTr is not null)
            {
                return cartFromTr with
                {
                    ComPort = cart.ComPort,
                };
            }

            var deviceHash = Guid.NewGuid().ToString().GenerateFilenameSafeHash();

            cart = cart with
            {
                DeviceId = deviceHash
            };
            var cartBuffer = cart.Serialize() ?? throw new Exception("Unable to serialize cart config");

            var fileTransferItem = new FileTransferItem
            (
                buffer: cartBuffer,
                name: "remote-config.txt",
                targetPath: StorageConstants.Remote_Path_Root,
                targetStorage: TeensyStorageType.SD
            );

            var saveFileCommand = new SaveFilesCommand([fileTransferItem])
            {
                Serial = serialPort
            };
            var saveFileResult = await mediator.Send(getFileCommand);

            if(saveFileResult.IsSuccess is false)
            {
                log.InternalError("Failed to save remote config file");
                throw new TeensyException("Failed to save remote config file");
            }
            return cart;
        }
    }
}
