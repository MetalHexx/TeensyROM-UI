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
using System.CodeDom.Compiler;
using TeensyRom.Ui.Controls.Paging;
using TeensyRom.Ui.Features.Games.State;

namespace TeensyRom.Ui.Features.Games.GameList
{
    public class GameListViewModel : ReactiveObject, IDisposable
    {
        [ObservableAsProperty] public ObservableCollection<StorageItem> DirectoryContent { get; }
        [ObservableAsProperty] public bool ShowPaging { get; }
        [Reactive] public PagingViewModel Paging { get; set; }
        public ReactiveCommand<GameItem, Unit> PlayCommand { get; set; }
        public ReactiveCommand<GameItem, Unit> SelectCommand { get; set; }
        public ReactiveCommand<GameItem, Unit> SaveFavoriteCommand { get; set; }
        public ReactiveCommand<GameItem, Unit> DeleteCommand { get; set; }
        public ReactiveCommand<DirectoryItem, Unit> LoadDirectoryCommand { get; set; }

        private readonly IPlayerContext _gameState;
        private readonly IAlertService _alert;
        private readonly IDialogService _dialog;
        private IDisposable _directoryTreeSubscription;

        public GameListViewModel(IPlayerContext gameState, IAlertService alert, IDialogService dialog)
        {
            _gameState = gameState;
            _alert = alert;
            _dialog = dialog;

            PlayCommand = ReactiveCommand.CreateFromTask<GameItem>(
                execute: gameState.PlayGame, 
                outputScheduler: RxApp.MainThreadScheduler);

            SelectCommand = ReactiveCommand.Create<GameItem, Unit>(
                execute: gameState.SetSelectedGame, 
                outputScheduler: RxApp.MainThreadScheduler);

            SaveFavoriteCommand = ReactiveCommand.CreateFromTask<GameItem>(
                execute: gameState.SaveFavorite,
                outputScheduler: RxApp.MainThreadScheduler);

            DeleteCommand = ReactiveCommand.CreateFromTask<GameItem>(
                execute:  HandleDelete,
                outputScheduler: RxApp.MainThreadScheduler);

            LoadDirectoryCommand = ReactiveCommand.CreateFromTask<DirectoryItem>(
                execute: directory => gameState.LoadDirectory(directory.Path), 
                outputScheduler: RxApp.MainThreadScheduler);

            _gameState.DirectoryContent.ToPropertyEx(this, x => x.DirectoryContent);
            gameState.PagingEnabled.ToPropertyEx(this, x => x.ShowPaging);

            Paging = new(gameState.CurrentPage, gameState.TotalPages)
            {
                NextPageCommand = ReactiveCommand.Create<Unit, Unit>(_ => gameState.NextPage()),
                PreviousPageCommand = ReactiveCommand.Create<Unit, Unit>(_ => gameState.PreviousPage()),
                PageSizeCommand = ReactiveCommand.Create<int, Unit>(gameState.SetPageSize)
            };
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