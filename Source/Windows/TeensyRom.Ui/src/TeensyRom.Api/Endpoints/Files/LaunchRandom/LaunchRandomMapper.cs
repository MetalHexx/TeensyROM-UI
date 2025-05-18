using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Api.Endpoints.Files.LaunchRandom
{
    public class LaunchRandomMapper : IRadMapper<LaunchableItemViewModel, ILaunchableItem>
    {
        public LaunchableItemViewModel FromEntity(ILaunchableItem e) => LaunchableItemViewModel.FromLaunchable(e);

    }
}