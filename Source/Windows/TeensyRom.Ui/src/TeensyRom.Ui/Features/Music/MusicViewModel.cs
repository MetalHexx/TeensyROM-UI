using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TeensyRom.Ui.Features.Music.PlayToolbar;
using TeensyRom.Ui.Helpers.ViewModel;

namespace TeensyRom.Ui.Features.Music
{
    public class MusicViewModel : FeatureViewModelBase
    {
        private readonly IMusicState _musicState;
        [Reactive] public PlayToolbarViewModel PlayToolBar { get; set; }       
        
        public MusicViewModel(IMusicState musicState, PlayToolbarViewModel playToolBar) 
        {
            FeatureTitle = "Music";
            _musicState = musicState;
            PlayToolBar = playToolBar;
        }
    }
}
