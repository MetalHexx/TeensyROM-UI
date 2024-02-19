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

namespace TeensyRom.Ui.Controls.DirectoryList
{
    public class DirectoryListViewModel : ReactiveObject
    {
        [ObservableAsProperty] public ObservableCollection<IStorageItem>? DirectoryContent { get; }
        [ObservableAsProperty] public bool ShowPaging { get; }
        [Reactive] public PagingViewModel Paging { get; set; }
        public ReactiveCommand<ILaunchableItem, Unit> PlayCommand { get; set; }
        public ReactiveCommand<ILaunchableItem, Unit> SelectCommand { get; set; }
        public ReactiveCommand<ILaunchableItem, Unit> SaveFavoriteCommand { get; set; }
        public ReactiveCommand<IFileItem, Unit> DeleteCommand { get; set; }
        public ReactiveCommand<DirectoryItem, Unit> LoadDirectoryCommand { get; set; }

        public DirectoryListViewModel
        (
            IObservable<ObservableCollection<IStorageItem>> directoryContent,
            IObservable<bool> pagingEnabled,
            IObservable<int> currentPage,
            IObservable<int> totalPages,
            Func<ILaunchableItem, Task> launchGameFunc, 
            Func<ILaunchableItem, Unit> setSelectedFunc, 
            Func<ILaunchableItem, Task> saveFavFunc,
            Func<IFileItem, Task> deleteFunc,
            Func<string, string, Task> loadDirFunc,
            Func<Unit> nextPageFunc,
            Func<Unit> prevPageFunc,
            Func<int, Unit> setPageSizeFunc,
            IAlertService alert, 
            IDialogService dialog
        )
        {
            directoryContent.ToPropertyEx(this, x => x.DirectoryContent);
            pagingEnabled.ToPropertyEx(this, x => x.ShowPaging);

            Paging = new(currentPage, totalPages)
            {
                NextPageCommand = ReactiveCommand.Create<Unit, Unit>(_ => nextPageFunc()),
                PreviousPageCommand = ReactiveCommand.Create<Unit, Unit>(_ => prevPageFunc()),
                PageSizeCommand = ReactiveCommand.Create(setPageSizeFunc)
            };

            PlayCommand = ReactiveCommand.CreateFromTask(
                execute: launchGameFunc,
                outputScheduler: RxApp.MainThreadScheduler);

            SelectCommand = ReactiveCommand.Create(
                execute: setSelectedFunc,
                outputScheduler: RxApp.MainThreadScheduler);

            SaveFavoriteCommand = ReactiveCommand.CreateFromTask(
                execute: saveFavFunc,
                outputScheduler: RxApp.MainThreadScheduler);

            LoadDirectoryCommand = ReactiveCommand.CreateFromTask<DirectoryItem>(
                execute: directory => loadDirFunc(directory.Path, null!),
                outputScheduler: RxApp.MainThreadScheduler);

            DeleteCommand = ReactiveCommand.CreateFromTask<IFileItem, Unit>(
                execute: async file => 
                {
                    var confirmed = await dialog.ShowConfirmation($"Are you sure you want to delete {file.Path}?");

                    if (!confirmed) return Unit.Default;

                    await deleteFunc(file);

                    alert.Publish($"{file.Path} has been deleted.");

                    return Unit.Default;
                },
                outputScheduler: RxApp.MainThreadScheduler);
        }
    }
}