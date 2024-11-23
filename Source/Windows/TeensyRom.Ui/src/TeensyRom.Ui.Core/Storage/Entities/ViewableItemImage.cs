namespace TeensyRom.Ui.Core.Storage.Entities
{
    public class ViewableItemImage
    {
        public string FileName { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;

        public ViewableItemImage Clone() => new()
        {
            Path = Path,
            Source = Source
        };
    }
}
