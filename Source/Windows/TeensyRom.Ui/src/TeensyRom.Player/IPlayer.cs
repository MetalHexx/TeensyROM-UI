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

        Task PlayItem();
        Task Pause();
        Task Stop();
        Task PlayNext();
        Task PlayPrevious();
        void SetPlayItem(ILaunchableItem item);
        void SetPlaylist(List<ILaunchableItem> Items);
        void SetPlayTimer(TimeSpan time);
        void SetFilter(TeensyFilterType filter);
        void ClearPlaytimer();
        void EnableSongTimeOverride();
        void ClearSongTimeOverride();
    }
}
