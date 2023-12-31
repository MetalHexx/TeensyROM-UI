using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Common
{
    public static class TeensyStorageTypeExtensions
    {
        public static uint GetStorageToken(this TeensyStorageType type)
        {
            return type switch
            {
                TeensyStorageType.SD => TeensyStorageToken.SdCard,
                TeensyStorageType.USB => TeensyStorageToken.UsbStick,
                _ => throw new ArgumentException("Unknown Storage Type")
            };
        }
    }
}
