﻿using System.Reactive;
using TeensyRom.Core.Abstractions;

namespace TeensyRom.Core.Serial.State
{
    public class SerialConnectableState : SerialState
    {
        public SerialConnectableState(IObservableSerialPort _serialPort) : base(_serialPort) { }
        public override bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(SerialConnectedState)
                || nextStateType == typeof(SerialStartState)
                || nextStateType == typeof(SerialBusyState);
        }
        public override Unit SetPort(string port) => _serialPort.SetPort(port);
        public override string? OpenPort() => _serialPort.OpenPort();
    }
}
