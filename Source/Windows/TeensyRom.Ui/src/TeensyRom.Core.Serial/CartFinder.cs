using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Settings;

namespace TeensyRom.Core.Serial
{

    public interface ICartFinder
    {
        List<Cart> DiscoverCarts();
    }

    public class CartFinder(ILoggingService log, IFwVersionChecker versionChecker) : ICartFinder
    {
        private readonly SerialPort _serial = new() { BaudRate = 115200 };

        public List<Cart> DiscoverCarts()
        {
            if (_serial.IsOpen) _serial.Close();

            var ports = GetPorts();

            List<Cart> foundCarts = [];

            foreach (var port in ports)
            {
                try
                {
                    _serial.PortName = port;
                    _serial.Open();
                }
                catch (Exception)
                {
                    log.ExternalError($"CartFinder.Find: Unable to connect to {_serial.PortName}");
                    continue;
                }
                _serial.Write([(byte)TeensyToken.VersionCheck.Value], 0, 1);

                var response = _serial.ReadAndLogSerialAsString(200);

                var isTeensyRom = IsTeensyRom(response);
                var (isCompatible, version) = GetVersion(response);

                if (isTeensyRom)
                {
                    foundCarts.Add(new Cart
                    {
                        ComPort = port,
                        FwVersion = version,
                        IsCompatible = isCompatible
                    });
                }
                _serial.Close();
            }
            return foundCarts;
        }

        private bool IsTeensyRom(string response)
        {
            return response.Contains("teensyrom", StringComparison.OrdinalIgnoreCase)
                || response.Contains("busy", StringComparison.OrdinalIgnoreCase);
        }

        private (bool, Version?) GetVersion(string response)
        {
            if (!response.Contains("busy", StringComparison.OrdinalIgnoreCase))
            {
                return versionChecker.VersionCheck(response);
            }
            return (false, null);
        }

        private List<string> GetPorts() => SerialPort.GetPortNames().Distinct().ToList();
    }
}
