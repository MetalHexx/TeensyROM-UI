namespace TeensyRom.Core.Common
{
    public static class DoubleExtensions 
    {
        public static sbyte ToSbyte(this double value, int clampMin = sbyte.MinValue, int clampMax = sbyte.MaxValue)
        {
            return (sbyte)Math.Clamp(value, clampMin, clampMax);
        }
    }
}
