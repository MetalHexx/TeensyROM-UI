using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Api.Endpoints.Common
{
    public class FileItemDto
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public long Size { get; set; }
        public bool IsFavorite { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Creator { get; set; } = string.Empty;
        public string ReleaseInfo { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ShareUrl { get; set; } = string.Empty;
        public string MetadataSource { get; set; } = string.Empty;
        public string Meta1 { get; set; } = string.Empty;
        public string Meta2 { get; set; } = string.Empty;
        public string MetadataSourcePath { get; set; } = string.Empty;

        public string FavChildPath { get; set; } = string.Empty;
        public string FavParentPath { get; set; } = string.Empty;

        public TimeSpan? PlayLength { get; set; }
        public List<TimeSpan>? SubtuneLengths { get; set; }
        public int? StartSubtuneNum { get; set; }
        public List<ViewableItemImageDto>? Images { get; set; }

        public FileItemType Type { get; set; } = FileItemType.Unknown;

        public static FileItemDto FromLaunchable(ILaunchableItem item)
        {
            var vm = new FileItemDto
            {
                Name = item.Name,
                Path = item.Path,
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
                MetadataSourcePath = item.MetadataSourcePath,
                FavChildPath = item.FavChildPath,
                FavParentPath = item.FavParentPath,
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
