using TeensyRom.Core.Player;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Cli.Services
{
    internal interface IPlayerService
    {
        IObservable<ILaunchableItem> FileLaunched { get; }

        PlayerState GetState();
        Task LaunchItem(string path);
        Task PlayNext();
        Task PlayPrevious();
        Task PlayRandom();
        void SetFilter(TeensyFilterType filterType);
        void SetStreamTime(TimeSpan? timespan);
        void SetSidTimer(SidTimer value);
        void StopStream();
        void SetDirectoryScope(string path);
        List<ILaunchableItem> SetSearchMode(string query);
        Task<List<ILaunchableItem>> SetDirectoryMode(string path);
        void SetRandomMode();
        void TogglePlay();
        void SetStorage(TeensyStorageType storageType);
    }
}
