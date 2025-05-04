namespace TeensyRom.Core.Serial
{
    public class Cart 
    {
        public string ComPort { get; set; } = string.Empty;
        public Version? FwVersion { get; set; }
        public bool IsCompatible { get; set; }
    }
}
