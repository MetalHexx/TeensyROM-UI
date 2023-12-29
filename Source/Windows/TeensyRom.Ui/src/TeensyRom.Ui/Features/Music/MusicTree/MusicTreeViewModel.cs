using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Features.Music.State;

namespace TeensyRom.Ui.Features.Music.MusicTree
{
    public class MusicTreeViewModel: ReactiveObject
    {
        [Reactive] public DirectoryTreeViewModel DirectoryTreeViewModel { get; set; } 

        public MusicTreeViewModel(IMusicState musicState)
        {
            DirectoryTreeViewModel = new DirectoryTreeViewModel(musicState.DirectoryTree);

            DirectoryTreeViewModel.DirectorySelectedCommand = ReactiveCommand.CreateFromTask<DirectoryItem>(async (directory) => 
                await musicState.LoadDirectory(directory.Path), outputScheduler: RxApp.MainThreadScheduler);
        }
    }
}
