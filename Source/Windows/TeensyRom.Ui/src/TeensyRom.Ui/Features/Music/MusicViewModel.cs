using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TeensyRom.Ui.Features.Music.PlayToolbar;
using TeensyRom.Ui.Features.Music.SongList;
using TeensyRom.Ui.Helpers.ViewModel;

namespace TeensyRom.Ui.Features.Music
{
    public class MusicViewModel : FeatureViewModelBase
    {
        private readonly IMusicState _musicState;
        [Reactive] public PlayToolbarViewModel PlayToolBar { get; set; }       
        [Reactive] public SongListViewModel SongList { get; set; }

        public MusicViewModel(IMusicState musicState, PlayToolbarViewModel playToolBar, SongListViewModel songList) 
        {
            FeatureTitle = "Music";
            _musicState = musicState;
            PlayToolBar = playToolBar;
            SongList = songList;
        }
    }
}
