namespace TeensyRom.Core.Common
{
    public static class DoubleExtensions 
    {
        public static sbyte ToSbyte(this double value, int clampMin = sbyte.MinValue, int clampMax = sbyte.MaxValue)
        {
            return (sbyte)Math.Clamp(value, clampMin, clampMax);
        }

        public static short ToScaledShort(this double speed) => (short)(speed * 256);        
    }
}
