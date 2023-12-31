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
                TeensyStorageType.SD => TeensyConstants.Sd_Card_Token,
                TeensyStorageType.USB => TeensyConstants.Usb_Stick_Token,
                _ => throw new ArgumentException("Unknown Storage Type")
            };
        }
    }
}
