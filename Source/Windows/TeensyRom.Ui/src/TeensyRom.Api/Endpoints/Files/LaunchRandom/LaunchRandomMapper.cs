using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Api.Endpoints.Files.LaunchRandom
{
    public class LaunchRandomMapper : IRadMapper<FileItemViewModel, ILaunchableItem>
    {
        public FileItemViewModel FromEntity(ILaunchableItem e) => FileItemViewModel.FromLaunchable(e);

    }
}