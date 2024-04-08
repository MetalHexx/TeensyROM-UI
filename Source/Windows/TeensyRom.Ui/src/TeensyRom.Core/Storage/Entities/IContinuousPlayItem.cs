namespace TeensyRom.Core.Storage.Entities
{
    public interface IContinuousPlayItem : IFileItem
    {
        TimeSpan PlayLength { get; set; }
    }    
}
