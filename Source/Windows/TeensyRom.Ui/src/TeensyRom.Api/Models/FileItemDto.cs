using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Api.Models
{
    /// <summary>
    /// Data transfer object representing a file item in TeensyROM storage.
    /// </summary>
    public class FileItemDto
    {
        /// <summary>
        /// The name of the file.
        /// </summary>
        [Required] public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The full path to the file.
        /// </summary>
        [Required] public string Path { get; set; } = string.Empty;

        /// <summary>
        /// The size of the file in bytes.
        /// </summary>
        [Required] public long Size { get; set; }

        /// <summary>
        /// Indicates whether the file is marked as a favorite.
        /// </summary>
        [Required] public bool IsFavorite { get; set; }

        /// <summary>
        /// The title of the file, if available.
        /// </summary>
        [Required] public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The creator or author of the file, if available.
        /// </summary>
        [Required] public string Creator { get; set; } = string.Empty;

        /// <summary>
        /// Release information for the file, if available.
        /// </summary>
        [Required] public string ReleaseInfo { get; set; } = string.Empty;

        /// <summary>
        /// A description of the file, if available.
        /// </summary>
        [Required] public string Description { get; set; } = string.Empty;

        /// <summary>
        /// A shareable URL for the file, if available.
        /// </summary>
        [Required] public string ShareUrl { get; set; } = string.Empty;

        /// <summary>
        /// The source of the metadata for the file.
        /// </summary>
        [Required] public string MetadataSource { get; set; } = string.Empty;

        /// <summary>
        /// Additional metadata field 1.
        /// </summary>
        [Required] public string Meta1 { get; set; } = string.Empty;

        /// <summary>
        /// Additional metadata field 2.
        /// </summary>
        [Required] public string Meta2 { get; set; } = string.Empty;

        /// <summary>
        /// The path to the metadata source file, if available.
        /// </summary>
        [Required] public string MetadataSourcePath { get; set; } = string.Empty;

        /// <summary>
        /// The path to the favorite parent, if applicable.
        /// </summary>
        [Required] public string ParentPath { get; set; } = string.Empty;

        /// <summary>
        /// The total play length of the file, if applicable.
        /// </summary>
        [Required] public TimeSpan PlayLength { get; set; }

        /// <summary>
        /// The play lengths of subtunes, if applicable.
        /// </summary>
        [Required] public List<TimeSpan> SubtuneLengths { get; set; } = [];

        /// <summary>
        /// The starting subtune number, if applicable.
        /// </summary>
        [Required] public int StartSubtuneNum { get; set; }

        /// <summary>
        /// A list of images associated with the file.
        /// </summary>
        [Required] public List<ViewableItemImageDto> Images { get; set; } = [];

        /// <summary>
        /// The type of the file (e.g., Song, Game, Image, Hex, Unknown).
        /// </summary>
        [Required] public FileItemType Type { get; set; } = FileItemType.Unknown;

        /// <summary>
        /// Creates a <see cref="FileItemDto"/> from an <see cref="LaunchableItem"/> entity.
        /// </summary>
        public static FileItemDto FromLaunchable(LaunchableItem item)
        {
            var vm = new FileItemDto
            {
                Name = item.Name,
                Path = item.Path.Value,
                Size = item.Size,
                IsFavorite = item.IsFavorite,
                Title = item.Title,
                Creator = item.Creator,
                ReleaseInfo = item.ReleaseInfo,
                Description = item.Description,
                ShareUrl = item.ShareUrl,
                MetadataSource = item.MetadataSource,
                Meta1 = item.Meta1,
                Meta2 = item.Meta2,
                MetadataSourcePath = item.MetadataSourcePath.Value,
                ParentPath = item.ParentPath.Value,
            };

            switch (item)
            {
                case SongItem song:
                    vm.Type = FileItemType.Song;
                    vm.PlayLength = song.PlayLength;
                    vm.SubtuneLengths = song.SubtuneLengths;
                    vm.StartSubtuneNum = song.StartSubtuneNum;
                    vm.Images = song.Images.Select(ViewableItemImageDto.FromViewableItemImage).ToList();
                    break;

                case GameItem game:
                    vm.Type = FileItemType.Game;
                    vm.PlayLength = game.PlayLength;
                    vm.Images = game.Images.Select(ViewableItemImageDto.FromViewableItemImage).ToList();
                    break;

                case ImageItem image:
                    vm.Type = FileItemType.Image;
                    vm.PlayLength = image.PlayLength;
                    break;

                case HexItem hex:
                    vm.Type = FileItemType.Hex;
                    vm.Images = hex.Images.Select(ViewableItemImageDto.FromViewableItemImage).ToList();
                    break;

                default:
                    vm.Type = FileItemType.Unknown;
                    break;
            }

            return vm;
        }
    }
}
