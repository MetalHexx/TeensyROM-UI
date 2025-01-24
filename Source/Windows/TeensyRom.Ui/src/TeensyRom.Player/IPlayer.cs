using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Player
{
    public interface IPlayer
    {
        IObservable<PlayerState> PlayerState { get; }
        IObservable<ILaunchableItem?> CurrentItem { get; }
        IObservable<TimeSpan> CurrentTime { get; }
        IObservable<ILaunchableItem> BadFile { get; }

        Task PlayItem(ILaunchableItem item);
        Task Pause();
        void Stop();
        Task PlayNext();
        Task PlayPrevious();
        void SetPlaylist(List<ILaunchableItem> Items);
        void SetPlayTimer(TimeSpan time);
        void SetFilter(TeensyFilterType filter);
        void ClearPlaytimer();
        void EnableSongTimeOverride();
        void DisableSongTimeOverride();
        Task ResumeItem();
        void RemoveItem(ILaunchableItem item);
        Task PlayItem(string path);
    }
}
