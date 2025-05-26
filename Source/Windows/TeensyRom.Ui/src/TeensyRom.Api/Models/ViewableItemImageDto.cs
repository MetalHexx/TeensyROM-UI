using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Api.Models
{
    public class ViewableItemImageDto
    {
        public string FileName { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;

        public static ViewableItemImageDto FromViewableItemImage(ViewableItemImage image)
        {
            return new ()
            {
                FileName = image.FileName,
                Path = image.Path,
                Source = image.Source
            };
        }
    }
}
