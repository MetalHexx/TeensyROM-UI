using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Api.Models
{
    /// <summary>
    /// Data transfer object representing an image associated with a viewable item in TeensyROM storage.
    /// </summary>
    public class ViewableItemImageDto
    {
        /// <summary>
        /// The file name of the image.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// The full path to the image file.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// The source or origin of the image.
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Creates a <see cref="ViewableItemImageDto"/> from a <see cref="ViewableItemImage"/> entity.
        /// </summary>
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
