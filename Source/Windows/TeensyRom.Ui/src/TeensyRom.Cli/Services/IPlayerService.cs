using TeensyRom.Cli.Core.Player;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Cli.Services
{
    internal interface IPlayerService
    {
        IObservable<ILaunchableItem> FileLaunched { get; }

        PlayerState GetState();
        Task<bool> LaunchItem(ILaunchableItem item);
        Task LaunchFromDirectory(string path);
        Task PlayNext();
        Task PlayPrevious();
        Task PlayRandom();
        void SetFilter(TeensyFilterType filterType);
        void SetStreamTime(TimeSpan? timespan);
        void SetSidTimer(SidTimer value);
        void StopStream();
        void SetDirectoryScope(string path);
        void SetSearchMode(string query);
        void SetDirectoryMode(string path);
        void SetRandomMode(string path);
        void TogglePlay();
        void SetStorage(TeensyStorageType storageType);
    }
}
