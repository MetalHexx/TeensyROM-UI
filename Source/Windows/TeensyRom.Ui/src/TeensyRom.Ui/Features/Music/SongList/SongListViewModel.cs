using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Features.Common.Models;
using TeensyRom.Ui.Features.Music.State;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Music.SongList
{
    public class SongListViewModel: ReactiveObject
    {
        [ObservableAsProperty] public ObservableCollection<IStorageItem>? DirectoryContent { get; }
        public ReactiveCommand<SongItem, Unit> PlayCommand { get; set; }
        public ReactiveCommand<SongItem, Unit> SaveFavoriteCommand { get; set; }
        public ReactiveCommand<IFileItem, Unit> DeleteCommand { get; set; }
        public ReactiveCommand<DirectoryItem, Unit> LoadDirectoryCommand { get; set; }

        private readonly IMusicState _musicState;
        private readonly IAlertService _alert;
        private readonly IDialogService _dialog;

        public SongListViewModel(IMusicState musicState, IAlertService alert, IDialogService dialog)
        {
            _musicState = musicState;
            _alert = alert;
            _dialog = dialog;

            PlayCommand = ReactiveCommand.CreateFromTask<SongItem>(
                execute: song => musicState.LoadSong(song), 
                outputScheduler: RxApp.MainThreadScheduler);

            SaveFavoriteCommand = ReactiveCommand.CreateFromTask<SongItem>(
                musicState.SaveFavorite, 
                outputScheduler: RxApp.MainThreadScheduler);

            DeleteCommand = ReactiveCommand.CreateFromTask<IFileItem>(
                execute: HandleDelete, 
                outputScheduler: RxApp.MainThreadScheduler);

            LoadDirectoryCommand = ReactiveCommand.CreateFromTask<DirectoryItem>(directory =>
                musicState.LoadDirectory(directory.Path), outputScheduler: RxApp.MainThreadScheduler);

            _musicState.DirectoryContent.ToPropertyEx(this, x => x.DirectoryContent);
        }

        private async Task<Unit> HandleDelete(IFileItem fileItem)
        {
            var confirmed = await _dialog.ShowConfirmation($"Are you sure you want to delete {fileItem.Path}?");

            if (!confirmed) return Unit.Default;

            await _musicState.DeleteFile(fileItem);

            _alert.Publish($"{fileItem.Path} has been deleted.");

            return Unit.Default;
        }
    }
}