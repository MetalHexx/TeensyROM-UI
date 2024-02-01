using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Services;
using TeensyRom.Ui.Features.Games.State;

namespace TeensyRom.Ui.Features.Games.GameList
{
    public class GameListViewModel : ReactiveObject, IDisposable
    {
        [ObservableAsProperty] public ObservableCollection<StorageItem> DirectoryContent { get; }
        public ReactiveCommand<GameItem, bool> PlayCommand { get; set; }
        public ReactiveCommand<GameItem, Unit> SelectCommand { get; set; }
        public ReactiveCommand<GameItem, bool> SaveFavoriteCommand { get; set; }
        public ReactiveCommand<GameItem, Unit> DeleteCommand { get; set; }
        public ReactiveCommand<DirectoryItem, Unit> LoadDirectoryCommand { get; set; }

        private readonly IGameState _gameState;
        private readonly IAlertService _alert;
        private readonly IDialogService _dialog;
        private IDisposable _directoryTreeSubscription;

        public GameListViewModel(IGameState gameState, IAlertService alert, IDialogService dialog)
        {
            _gameState = gameState;
            _alert = alert;
            _dialog = dialog;

            PlayCommand = ReactiveCommand.CreateFromTask<GameItem, bool>(game =>
                gameState.LoadGame(game), outputScheduler: RxApp.MainThreadScheduler);

            SelectCommand = ReactiveCommand.Create<GameItem, Unit>(gameState.SetSelectedGame, outputScheduler: RxApp.MainThreadScheduler);

            SaveFavoriteCommand = ReactiveCommand.CreateFromTask<GameItem, bool>(
                gameState.SaveFavorite,
                outputScheduler: RxApp.MainThreadScheduler);

            DeleteCommand = ReactiveCommand.CreateFromTask<GameItem>(
                execute: HandleDelete,
                outputScheduler: RxApp.MainThreadScheduler);

            LoadDirectoryCommand = ReactiveCommand.CreateFromTask<DirectoryItem>(directory =>
                gameState.LoadDirectory(directory.Path), outputScheduler: RxApp.MainThreadScheduler);

            _gameState.DirectoryContent.ToPropertyEx(this, x => x.DirectoryContent);
        }

        private async Task<Unit> HandleDelete(GameItem fileItem)
        {
            var confirmed = await _dialog.ShowConfirmation($"Are you sure you want to delete {fileItem.Path}?");

            if (!confirmed) return Unit.Default;

            await _gameState.DeleteFile(fileItem);

            _alert.Publish($"{fileItem.Path} has been deleted.");

            return Unit.Default;
        }
        public void Dispose()
        {
            _directoryTreeSubscription?.Dispose();
        }
    }
}