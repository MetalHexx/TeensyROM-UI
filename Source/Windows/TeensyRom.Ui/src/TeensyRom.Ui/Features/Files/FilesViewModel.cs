using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Controls.Paging;
using TeensyRom.Ui.Features.Files.DirectoryContent;
using TeensyRom.Ui.Features.Files.Search;
using TeensyRom.Ui.Features.Files.State;
using TeensyRom.Ui.Features.Global;
using TeensyRom.Ui.Features.Music.Search;
using TeensyRom.Ui.Features.Music.State;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Helpers.ViewModel;
using TeensyRom.Ui.Services;

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
        private readonly IDialogService _dialog;
        private TeensySettings _settings = null!;

        public FilesViewModel(IGlobalState globalState, IFileState fileState, ISettingsService settingsService, IDialogService dialog, DirectoryContentViewModel directoryContent, SearchFilesViewModel search)
        {
            FeatureTitle = "File Explorer";            
            DirectoryContent = directoryContent;
            Search = search;
            _dialog = dialog;
            _fileState = fileState;
            
            settingsService.Settings.Subscribe(s => _settings = s);
            globalState.SerialConnected.ToPropertyEx(this, x => x.FilesAvailable);
            fileState.PagingEnabled.ToPropertyEx(this, x => x.PagingEnabled);

            RefreshCommand = ReactiveCommand.CreateFromTask(_ => fileState.RefreshDirectory());
            PlayRandomCommand = ReactiveCommand.CreateFromTask(_ => fileState.PlayRandom());
            CacheAllCommand = ReactiveCommand.CreateFromTask(HandleCacheAll);

            DirectoryTree = new(fileState.DirectoryTree)
            {
                DirectorySelectedCommand = ReactiveCommand.CreateFromTask<DirectoryNodeViewModel>(async (directory) =>
                await _fileState.LoadDirectory(directory.Path), outputScheduler: RxApp.MainThreadScheduler)
            };

            Paging = new(fileState.CurrentPage, fileState.TotalPages)
            {
                NextPageCommand = ReactiveCommand.CreateFromTask(_ => fileState.NextPage()),
                PreviousPageCommand = ReactiveCommand.CreateFromTask(_ => fileState.PreviousPage()),
                PageSizeCommand = ReactiveCommand.CreateFromTask<int>(size => fileState.SetPageSize(size))
            };
        }

        private async Task HandleCacheAll()
        {
            var confirm = await _dialog.ShowConfirmation($"This will read all the files on your {_settings.TargetType} and save them to a local cache. Doing this will enable rich discovery of music and programs as it index all your files for search, random play and shuffle features.\r\rThis may take a few minutes if you have a lot of files from libraries like OneLoad64 or HSVC on your {_settings.TargetType} storage.\r\rProceed?");
            
            if (!confirm) return;

            await _fileState.CacheAll();
        }
    }
}