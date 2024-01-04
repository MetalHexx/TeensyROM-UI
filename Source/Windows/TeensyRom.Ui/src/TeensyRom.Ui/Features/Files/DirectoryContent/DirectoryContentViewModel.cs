using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Features.Music.State;
using TeensyRom.Ui.Features.Files.State;

namespace TeensyRom.Ui.Features.Files.DirectoryContent
{
    public class DirectoryContentViewModel : ReactiveObject, IDisposable
    {
        [ObservableAsProperty] public ObservableCollection<StorageItem> DirectoryContent { get; }
        [ObservableAsProperty] public bool ShowProgress { get; }
        public ReactiveCommand<SongItem, bool> PlayCommand { get; set; }
        public ReactiveCommand<SongItem, bool> SaveFavoriteCommand { get; set; }
        public ReactiveCommand<DirectoryItem, Unit> LoadDirectoryCommand { get; set; }

        private readonly IFileState _fileState;
        private IDisposable _directoryTreeSubscription;

        public DirectoryContentViewModel(IFileState fileState)
        {
            _fileState = fileState;

            _fileState.DirectoryLoading
                .ToPropertyEx(this, x => x.ShowProgress);

            //PlayCommand = ReactiveCommand.CreateFromTask<SongItem, bool>(song =>
            //    fileState.LoadSong(song), outputScheduler: RxApp.MainThreadScheduler);

            //SaveFavoriteCommand = ReactiveCommand.CreateFromTask<SongItem, bool>(
            //    fileState.SaveFavorite,
            //    outputScheduler: RxApp.MainThreadScheduler);

            LoadDirectoryCommand = ReactiveCommand.CreateFromTask<DirectoryItem>(directory =>
                fileState.LoadDirectory(directory.Path), outputScheduler: RxApp.MainThreadScheduler);

            _fileState.DirectoryContent.ToPropertyEx(this, x => x.DirectoryContent);
        }
        public void Dispose()
        {
            _directoryTreeSubscription?.Dispose();
        }
    }
}
