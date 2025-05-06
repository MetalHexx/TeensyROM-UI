using System.IO.Ports;
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
        Cart EnsureCartTag(Cart cart);
    }
    public class CartTagger(ILoggingService log) : ICartTagger
    {
        public Cart EnsureCartTag(Cart cart)
        {
            using var serialPort = new SerialPort(cart.ComPort, 115200);
            serialPort.PortName = cart.ComPort;
            serialPort.Open();
            var buffer = serialPort.GetFile("/remote-config.txt", TeensyStorageType.SD);

            var cartFromTr = buffer.Deserialize<Cart>();

            if (cartFromTr is not null)
            {
                return cartFromTr with
                {
                    ComPort = cart.ComPort,
                };
            }

            var deviceHash = Guid.NewGuid().ToString().GetFileNameSafeHash();

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

            serialPort.SaveFiles([fileTransferItem], log);

            return cart;
        }
    }
}
