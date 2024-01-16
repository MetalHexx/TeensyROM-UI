using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Core.Serial.State
{
    public interface ISerialState
    {
        public abstract void Handle(SerialStateContext context);
        public abstract bool CanTransitionTo(Type nextStateType);
    }
}