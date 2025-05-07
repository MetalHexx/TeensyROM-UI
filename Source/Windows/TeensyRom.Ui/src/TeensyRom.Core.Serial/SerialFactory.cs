using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Core.Serial
{
    public interface ISerialFactory
    {
        ISerialStateContext Create(string portName);
    }

    public class SerialFactory(IAlertService alert, ILoggingService log) : ISerialFactory
    {
        public ISerialStateContext Create(string portName)
        {
            var serial = new SimpleObservableSerialPort(log, alert);
            var context = new SerialStateContext(serial, log);
            serial.SetPort(portName);
            return context;
        }
    }
}
