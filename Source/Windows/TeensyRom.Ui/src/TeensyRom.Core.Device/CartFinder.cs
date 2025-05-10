using System.IO.Ports;
using TeensyRom.Core.Entities.Device;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;

namespace TeensyRom.Core.Device
{
    public interface ICartFinder
    {
        List<Cart> FindCarts();
    }

    public class CartFinder(ILoggingService log, IFwVersionChecker versionChecker) : ICartFinder
    {
        public List<Cart> FindCarts()
        {
            var ports = SerialHelper.GetPorts();

            using var serial = new SerialPort { BaudRate = 115200 };

            List<Cart> foundCarts = [];

            foreach (var port in ports)
            {
                if (serial.IsOpen) serial.Close();

                try
                {
                    serial.PortName = port;
                    serial.Open();
                }
                catch (Exception)
                {
                    log.ExternalError($"CartFinder.Find: Unable to connect to {serial.PortName}");
                    continue;
                }
                serial.Write([(byte)TeensyToken.VersionCheck.Value], 0, 1);

                var response = serial.ReadAndLogSerialAsString(200);

                var isTeensyRom = response.IsTeensyRom();
                var (isCompatible, version) = GetVersion(response);

                if (isTeensyRom)
                {
                    foundCarts.Add(new Cart
                    {
                        ComPort = port,
                        Name = "Unnamed",
                        FwVersion = version?.ToString() ?? "",
                        IsCompatible = isCompatible
                    });
                }
            }
            return foundCarts;
        }

        private (bool, Version?) GetVersion(string response)
        {
            if (!response.Contains("busy", StringComparison.OrdinalIgnoreCase))
            {
                return versionChecker.VersionCheck(response);
            }
            return (false, null);
        }
    }
}
