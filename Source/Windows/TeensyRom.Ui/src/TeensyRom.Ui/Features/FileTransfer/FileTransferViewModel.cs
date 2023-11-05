using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Core.Settings.Services;
using TeensyRom.Core.Serial.Services;
using TeensyRom.Core.Logging;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Common;
using INavigationService = TeensyRom.Ui.Features.NavigationHost.INavigationService;
using TeensyRom.Core.Storage.Extensions;
using DynamicData.Binding;
using System.Threading.Tasks;
using TeensyRom.Ui.Features.FileTransfer.Models;
using System.Threading;
using System.Windows;
using System.Reactive.Subjects;

namespace TeensyRom.Ui.Features.FileTransfer
{
    public class FileTransferViewModel : ReactiveObject
    {
        public ObservableCollection<StorageItemVm> SourceItems { get; set; } = new();
        public ObservableCollection<StorageItemVm> TargetItems { get; set; } = new();

        [ObservableAsProperty]
        public bool IsTargetItemsEmpty { get; }

        [ObservableAsProperty]
        public bool CanExecuteTargetLoadCommand { get; } = true;

        [ObservableAsProperty]
        public bool IsLoadingFiles { get; } = false;


        [Reactive]
        public DirectoryContent? CurrentDirectory { get; set; }


        [Reactive]
        public string Logs { get; set; } = string.Empty;

        public ReactiveCommand<Unit, Unit> TestFileCopyCommand { get; set; }
        public ReactiveCommand<Unit, Unit> LoadParentDirectoryContentCommand { get; set; }
        public ReactiveCommand<Unit, Unit> TestDirectoryListCommand { get; set; }
        public ReactiveCommand<DirectoryItemVm, Unit> LoadDirectoryContentCommand { get; set; }

        private BehaviorSubject<bool> _isLoadingFiles = new BehaviorSubject<bool>(false);

        private readonly ITeensyDirectoryService _directoryService;
        private readonly ILoggingService _logService;
        private readonly ISnackbarService _snackbar;
        private readonly StringBuilder _logBuilder = new StringBuilder();
        
        private const uint _take = 20;
        public FileTransferViewModel(ITeensyDirectoryService directoryService, ISettingsService settingsService, ITeensyObservableSerialPort teensyPort, INavigationService nav, ILoggingService logService, ISnackbarService snackbar) 
        {
            _directoryService = directoryService;
            _logService = logService;
            _snackbar = snackbar;
            SourceItems = new ObservableCollection<StorageItemVm> { FileTreeTestData.InitializeTestStorageItems() };

            TestFileCopyCommand = ReactiveCommand.Create<Unit, Unit>(_ =>
                TestFileCopy(), outputScheduler: ImmediateScheduler.Instance);

            TestDirectoryListCommand = ReactiveCommand.Create<Unit, Unit>(_ => TestDirectoryListAsync(), outputScheduler: ImmediateScheduler.Instance);
            LoadParentDirectoryContentCommand = ReactiveCommand.Create<Unit, Unit>(_ => LoadParentDirectory(), outputScheduler: ImmediateScheduler.Instance);
            LoadDirectoryContentCommand = ReactiveCommand.Create<DirectoryItemVm, Unit>(directory => LoadNewDirectoryAsync(directory), outputScheduler: ImmediateScheduler.Instance);

            _logService.Logs.Subscribe(log =>
            {
                _logBuilder.AppendLine(log);
                Logs = _logBuilder.ToString();
            });

            teensyPort.IsConnected
                .Where(isConnected => isConnected is true)
                .CombineLatest(settingsService.Settings, (isConnected, settings) => settings)
                .Do(settings => 
                { 
                    CurrentDirectory ??= new() 
                    { 
                        Path = settings.TargetRootPath 
                    }; 
                })
                .CombineLatest(nav.SelectedNavigationView, (settings, currentNav) => (settings, currentNav))
                .Where(sn => sn.currentNav?.Type == NavigationLocation.FileTransfer)
                .Where(_ => TargetItems.Count == 0)
                .Subscribe(_ => LoadCurrentDirectory());

            TargetItems.ObserveCollectionChanges()
                .Select(targetCol => TargetItems.Count == 0)
                .ToPropertyEx(this, x => x.IsTargetItemsEmpty);

            _isLoadingFiles.ToPropertyEx(this, x => x.IsLoadingFiles);

            Observable.Merge(
                    TestFileCopyCommand.IsExecuting,
                    LoadParentDirectoryContentCommand.IsExecuting,
                    TestDirectoryListCommand.IsExecuting,
                    LoadDirectoryContentCommand.IsExecuting)
                .Select(x => !x)
                .Throttle(TimeSpan.FromMilliseconds(100))
                .ToPropertyEx(this, x => x.CanExecuteTargetLoadCommand);

        }

        private Unit LoadNewDirectoryAsync(DirectoryItemVm directoryVm)
        {            
            return LazyLoadDirectory(directoryVm.Path);
        }

        private Unit LoadCurrentDirectory()
        {
            if (CurrentDirectory is null) return Unit.Default;
            return LazyLoadDirectory(CurrentDirectory.Path);
        }

        private Unit LoadParentDirectory()
        {            
            if (CurrentDirectory is null) return Unit.Default;

            return LazyLoadDirectory(CurrentDirectory.Path.GetParentDirectory());
        }

        private Unit LazyLoadDirectory(string path)
        {
            _isLoadingFiles.OnNext(true);

            uint skip = 0;

            var directoryContent = LoadDirectoryContent(path, skip);

            if (directoryContent is null) return Unit.Default;

            Application.Current.Dispatcher.Invoke(() =>
            {
                MapDirectoryContentToView(directoryContent, clearItems: true);
            });

            CurrentDirectory = directoryContent;

            Task.Run(async () =>
            {
                while (directoryContent.TotalCount == _take)
                {
                    directoryContent = LoadDirectoryContent(path, skip);

                    Thread.Sleep(200);

                    if (directoryContent is null) return Unit.Default;

                    skip += _take;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MapDirectoryContentToView(directoryContent, clearItems: false);
                    });
                }
                _isLoadingFiles.OnNext(false);
                return Unit.Default;
            });
            return Unit.Default;
        }

        private DirectoryContent? LoadDirectoryContent(string path, uint skip)
        {

            DirectoryContent? directoryContent = null;

            try
            {
                directoryContent = _directoryService.GetDirectoryContent(path, skip, _take);
            }
            catch (TeensyException ex)
            {
                _snackbar.Enqueue(ex.Message);
                return null;
            }
            if (directoryContent is null)
            {                
                _snackbar.Enqueue("Error receiving directory contents");
            }
            return directoryContent;
        }

        private void MapDirectoryContentToView(DirectoryContent directoryContent, bool clearItems)
        {
            var newDirectories = directoryContent.Directories
                .Select(d => new DirectoryItemVm { Name = d.Name, Path = d.Path })
                .OrderBy(d => d.Name);

            var newFiles = directoryContent.Files
                .Select(f => new FileItemVm { Name = f.Name, Path = f.Path, Size = f.Size })
                .OrderBy(f => f.Name);

            if (clearItems) TargetItems.Clear();

            foreach (var dir in newDirectories)
            {
                var index = TargetItems
                    .TakeWhile(item => (item is DirectoryItemVm) && String.Compare(item.Name, dir.Name, StringComparison.OrdinalIgnoreCase) <= 0)
                    .Count();

                TargetItems.Insert(index, dir);
            }

            foreach (var file in newFiles)
            {
                var index = TargetItems
                    .TakeWhile(item => (item is DirectoryItemVm) || (item is FileItemVm && String.Compare(item.Name, file.Name, StringComparison.OrdinalIgnoreCase) <= 0))
                    .Count();

                TargetItems.Insert(index, file);
            }
        }

        private Unit TestDirectoryListAsync()
        {
            _directoryService.GetDirectoryContent("/", 0, 20);
            return Unit.Default;
        }

        private Unit TestFileCopy()
        {
            string sourcePath = @"C:\test\New folder (3)\Calliope_John_13.sid";
            string destinationPath = @$"C:\Users\Metal\Downloads\{Guid.NewGuid()}.sid";
            File.Copy(sourcePath, destinationPath, overwrite: true);
            Console.WriteLine($"File copied from {sourcePath} to {destinationPath}");
            return Unit.Default;
        }

        //public void DragOver(IDropInfo dropInfo)
        //{
        //    var sourceItem = dropInfo.Data as DirectoryNode;

        //    if (sourceItem != null)
        //    {
        //        dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
        //        dropInfo.Effects = DragDropEffects.Move;
        //    }
        //}

        //public void Drop(IDropInfo dropInfo)
        //{
        //    var sourceItem = dropInfo.Data as DirectoryNode;

        //    if (sourceItem != null)
        //    {
        //        TargetItems.Add(sourceItem);
        //        SourceItems.Remove(sourceItem);
        //    }
        //}
    }
}