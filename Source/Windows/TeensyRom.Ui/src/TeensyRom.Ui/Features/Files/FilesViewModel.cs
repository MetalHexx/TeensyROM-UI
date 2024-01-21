using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Features.Files.DirectoryContent;
using TeensyRom.Ui.Features.Files.Paging;
using TeensyRom.Ui.Features.Files.Search;
using TeensyRom.Ui.Features.Files.State;
using TeensyRom.Ui.Features.Global;
using TeensyRom.Ui.Features.Music.Search;
using TeensyRom.Ui.Features.Music.State;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Helpers.ViewModel;

namespace TeensyRom.Ui.Features.Files
{
    public class FilesViewModel : FeatureViewModelBase
    {
        [ObservableAsProperty] public bool FilesAvailable { get; set; }
        [ObservableAsProperty] public bool PagingEnabled { get; }
        [Reactive] public DirectoryTreeViewModel DirectoryTree { get; set; }
        [Reactive] public DirectoryContentViewModel DirectoryContent { get; set; }
        [Reactive] public SearchFilesViewModel Search { get; set; }
        [Reactive] public PagingViewModel Paging { get; set; }
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; set; }
        public ReactiveCommand<Unit, Unit> PlayRandomCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CacheAllCommand { get; set; }

        private readonly IFileState _fileState;

        public FilesViewModel(IGlobalState globalState, IFileState fileState, DirectoryContentViewModel directoryContent, SearchFilesViewModel search)
        {
            FeatureTitle = "File Explorer";            
            DirectoryContent = directoryContent;
            Search = search;
            _fileState = fileState;

            globalState.SerialConnected.ToPropertyEx(this, x => x.FilesAvailable);
            fileState.PagingEnabled.ToPropertyEx(this, x => x.PagingEnabled);

            RefreshCommand = ReactiveCommand.CreateFromTask<Unit>(_ => fileState.RefreshDirectory());
            PlayRandomCommand = ReactiveCommand.CreateFromTask<Unit>(_ => fileState.PlayRandom());
            CacheAllCommand = ReactiveCommand.CreateFromTask<Unit>(_ => fileState.CacheAll());

            DirectoryTree = new(fileState.DirectoryTree)
            {
                DirectorySelectedCommand = ReactiveCommand.CreateFromTask<DirectoryNodeViewModel>(async (directory) =>
                await _fileState.LoadDirectory(directory.Path), outputScheduler: RxApp.MainThreadScheduler)
            };

            Paging = new(fileState); 
        }
    }
}