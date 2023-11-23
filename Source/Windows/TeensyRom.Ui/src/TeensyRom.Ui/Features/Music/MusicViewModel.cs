using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive.Linq;
using TeensyRom.Ui.Features.Music.PlayToolbar;
using TeensyRom.Ui.Features.Music.SongList;
using TeensyRom.Ui.Features.Music.State;
using TeensyRom.Ui.Helpers.ViewModel;

namespace TeensyRom.Ui.Features.Music
{
    public class MusicViewModel : FeatureViewModelBase
    {
        private readonly IMusicState _musicState;

        [ObservableAsProperty] public bool ShowPlayToolbar { get; set; }
        [Reactive] public PlayToolbarViewModel PlayToolBar { get; set; }       
        [Reactive] public SongListViewModel SongList { get; set; }

        public MusicViewModel(IMusicState musicState, PlayToolbarViewModel playToolBar, SongListViewModel songList) 
        {
            FeatureTitle = "Music";
            _musicState = musicState;
            PlayToolBar = playToolBar;
            SongList = songList;

            _musicState.CurrentSong
                .Select(s => s is null)
                .ToPropertyEx(this, x => x.ShowPlayToolbar);
        }
    }
}
