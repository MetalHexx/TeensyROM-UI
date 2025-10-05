namespace TeensyRom.Core.Entities.Storage
{
    public class ViewableItemImage
    {
        public string FileName { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string BaseAssetPath { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;

        public ViewableItemImage Clone() => new()
        {
            FileName = FileName,
            Path = Path,
            BaseAssetPath = BaseAssetPath,
            Source = Source
        };
    }
}
