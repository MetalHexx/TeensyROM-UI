using Ardalis.SmartEnum;

namespace TeensyRom.Core.Storage
{
    public sealed class TeensyStorageToken : SmartEnum<TeensyStorageToken, uint>
    {
        public static readonly TeensyStorageToken SdCard = new(1U, nameof(SdCard));
        public static readonly TeensyStorageToken UsbStick = new(0U, nameof(UsbStick));

        private TeensyStorageToken(uint value, string name) : base(name, value) { }
    }
}
