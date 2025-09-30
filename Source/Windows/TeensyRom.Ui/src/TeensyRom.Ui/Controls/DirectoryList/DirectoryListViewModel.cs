using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Ui.Services;
using TeensyRom.Ui.Controls.Paging;
using File = System.IO.File;
using DragNDropFile = TeensyRom.Ui.Features.Common.Models.DragNDropFile;
using TeensyRom.Core.Midi;
using System.Reactive.Linq;
using DynamicData;
using TeensyRom.Core.ValueObjects;
using System.Runtime.CompilerServices;
using TeensyRom.Core.Common;
using TeensyRom.Core.Storage;

namespace TeensyRom.Ui.Controls.DirectoryList
{
    public class DirectoryListViewModel : ReactiveObject
    {
        public IObservable<MidiEvent> MidiEvents { get; set; }
        [ObservableAsProperty] public ObservableCollection<StorageItem>? DirectoryContent { get; }
        [ObservableAsProperty] public bool ShowPaging { get; }
        [Reactive] public PagingViewModel Paging { get; set; }
        public ReactiveCommand<LaunchableItem, Unit> PlayCommand { get; set; }
        public ReactiveCommand<LaunchableItem, Unit> SelectCommand { get; set; }
        public ReactiveCommand<LaunchableItem, Unit> SaveFavoriteCommand { get; set; }
        public ReactiveCommand<LaunchableItem, Unit> RemoveFavoriteCommand { get; set; }
        public ReactiveCommand<FileItem, Unit> DeleteCommand { get; set; }
        public ReactiveCommand<DragEventArgs, Unit> DropCommand { get; private set; }
        public ReactiveCommand<DragEventArgs, Unit> DragOverCommand { get; }
        public ReactiveCommand<DirectoryItem, Unit> LoadDirectoryCommand { get; set; }

        private readonly IMidiService _midiService;
        private readonly Func<List<FileItem>, Task> _reorderFunc;
        private readonly IAlertService _alert;
        private readonly IProgressService _progress;
        private readonly IFileTransferService _fileTransfer;

        private DirectoryPath _currentDirectory = new(string.Empty);
        private TeensyStorageType _currentStorage = TeensyStorageType.SD;        

        public DirectoryListViewModel
        (
            IObservable<DirectoryPath> currentDirectory,
            IObservable<TeensyStorageType> currentStorage,
            IObservable<ObservableCollection<StorageItem>> directoryContent,
            IObservable<bool> pagingEnabled,
            IObservable<int> currentPage,
            IObservable<int> totalPages,
            Func<LaunchableItem, Task> launchGameFunc, 
            Func<LaunchableItem, Unit> setSelectedFunc,
            Func<LaunchableItem, Task> saveFavFunc,
            Func<LaunchableItem, Task> removeFavFunc,
            Func<List<FileItem>, Task> reorderFunc,
            Func<FileItem, Task> deleteFunc,
            Func<DirectoryPath, FilePath, Task> loadDirFunc,
            Func<Unit> nextPageFunc,
            Func<Unit> prevPageFunc,
            Func<int, Unit> setPageSizeFunc,
            IAlertService alert, 
            IDialogService dialog,
            IProgressService progress,
            IMidiService midiService,
            IFileTransferService fileTransfer
        )
        {
            _reorderFunc = reorderFunc;
            _fileTransfer = fileTransfer;
            _alert = alert;
            _progress = progress;

            currentDirectory.Subscribe(d => _currentDirectory = d);
            currentStorage.Subscribe(s => _currentStorage = s);

            directoryContent
                .Select(d => 
                {
                    var files = d.OfType<FileItem>();
                    var directories = d.OfType<DirectoryItem>();

                    var orderedFiles = files
                        .OrderBy(f => f.Custom == null)          
                        .ThenBy(f => f.Custom?.Order)            
                        .ThenBy(f => f.Custom == null ? f.Name : null)
                        .ThenBy(f => f.Name);

                    var newCollection = new ObservableCollection<StorageItem>();
                    newCollection.AddRange(directories);
                    newCollection.AddRange(orderedFiles);
                    return newCollection;
                })
                .ToPropertyEx(this, x => x.DirectoryContent);

            pagingEnabled.ToPropertyEx(this, x => x.ShowPaging);
            MidiEvents = midiService.MidiEvents; 

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

            RemoveFavoriteCommand = ReactiveCommand.CreateFromTask(
                execute: removeFavFunc,
                outputScheduler: RxApp.MainThreadScheduler);

            LoadDirectoryCommand = ReactiveCommand.CreateFromTask<DirectoryItem>(
                execute: directory => loadDirFunc(directory.Path, null!),
                outputScheduler: RxApp.MainThreadScheduler);

            DeleteCommand = ReactiveCommand.CreateFromTask<FileItem, Unit>(
                execute: async file => 
                {
                    var confirmed = await dialog.ShowConfirmation($"Are you sure you want to delete {file.Path}?");

                    if (!confirmed) return Unit.Default;

                    await deleteFunc(file);

                    alert.Publish($"{file.Path} has been deleted.");

                    return Unit.Default;
                },
                outputScheduler: RxApp.MainThreadScheduler);

            DragOverCommand = ReactiveCommand.Create
            (
                execute: (DragEventArgs e) =>
                {
                    e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
                    e.Handled = true;
                },
                outputScheduler: RxApp.MainThreadScheduler
            );

            DropCommand = ReactiveCommand.CreateFromTask
            (
                execute: (DragEventArgs e) =>
                {
                    if (e.Data.GetDataPresent(typeof(StorageItem)))
                    {
                        return HandleInternalDrop(e);
                    }

                    return HandleFileDrop(e);
                },
                outputScheduler: RxApp.MainThreadScheduler
            );            
        }

        private Task HandleFileDrop(DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return Task.CompletedTask;
            }

            return Task.Run(async () =>
            {
                _progress.EnableProgress();
                _alert.Publish("Transferring files.");

                List<FileTransferItem> fileTransferItems = ProcessDrop(e);

                await _fileTransfer.Send(fileTransferItems);
                _progress.DisableProgress();
            });
        }
        
        private List<FileTransferItem> ProcessDrop(DragEventArgs e)
        {
            var items = (string[])e.Data.GetData(DataFormats.FileDrop);

            var singleDirectoryCopy = items.Length == 1 && !File.Exists(items[0]) && Directory.Exists(items[0]);

            var depth = singleDirectoryCopy ? 1 : 0;

            List<DragNDropFile> filePaths = [];

            foreach (var item in items)
            {
                if (File.Exists(item))
                {
                    filePaths.Add(new DragNDropFile(item));
                }
                else if (Directory.Exists(item))
                {
                    ProcessDirectoryAsync(item, filePaths, depth);
                }
            }

            var commonDirectory = filePaths
                .Select(f => f.File.FullName)
                .GetCommonBasePath();

            var baseDirectoryName = commonDirectory.GetLastDirectoryFromPath();

            return filePaths.Select(f =>
            {
                var fullPath = f.File.FullName;
                var relativePath = Path.GetRelativePath(commonDirectory, fullPath);

                var targetFilePath = singleDirectoryCopy
                    ? Path.Combine(baseDirectoryName, relativePath)
                    : relativePath;

                if (targetFilePath.Equals(".")) 
                {
                    targetFilePath = string.Empty;
                }
                targetFilePath = Path.Combine(targetFilePath, f.File.Name);

                var targetPath = new FilePath(targetFilePath.ToUnixPath()).Combine(_currentDirectory);

                return new FileTransferItem(
                    fileInfo: f.File,
                    targetFilePath: targetPath,
                    targetStorage: _currentStorage
                );
            })
            .ToList();
        }

        private Task HandleInternalDrop(DragEventArgs e)
        {
            var x = DirectoryContent;

            var files = DirectoryContent?.OfType<FileItem>().ToList();

            if (files is null) return Task.CompletedTask;

            return Task.Run(async () =>
            {

                await _reorderFunc(files);
            });
        }

        private static void ProcessDirectoryAsync(string directoryPath, List<DragNDropFile> filePaths, int directoryLevel = 0)
        {
            directoryLevel++;

            filePaths.AddRange(Directory
                .GetFiles(directoryPath)
                .Select(path => new DragNDropFile(path, inSubdirectory: directoryLevel > 1)));

            foreach (var subdirectory in Directory.GetDirectories(directoryPath))
            {
                ProcessDirectoryAsync(subdirectory, filePaths, directoryLevel);
            }
        }
    }
}