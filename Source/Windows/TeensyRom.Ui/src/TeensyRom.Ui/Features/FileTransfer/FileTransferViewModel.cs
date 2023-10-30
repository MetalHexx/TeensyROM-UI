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

namespace TeensyRom.Ui.Features.FileTransfer
{
    public class FileTransferViewModel : ReactiveObject
    {
        public ObservableCollection<StorageItemVm> SourceItems { get; set; } = new();
        public ObservableCollection<StorageItemVm> TargetItems { get; set; } = new();

        [Reactive]
        public DirectoryItemVm? CurrentDirectory { get; set; }


        [Reactive]
        public string Logs { get; set; } = string.Empty;

        public ReactiveCommand<Unit, Unit> TestFileCopyCommand { get; set; }
        public ReactiveCommand<Unit, Unit> TestDirectoryListCommand { get; set; }
        public ReactiveCommand<DirectoryItemVm, Unit> LoadDirectoryContentCommand { get; set; }


        private readonly ITeensyFileService _fileService;
        private readonly ITeensyDirectoryService _directoryService;
        private readonly ILoggingService _logService;
        private readonly ISnackbarService _snackbar;
        private readonly StringBuilder _logBuilder = new StringBuilder();

        public FileTransferViewModel(ITeensyFileService fileService, ITeensyDirectoryService directoryService, ISettingsService settingsService, ITeensyObservableSerialPort teensyPort, INavigationService nav, ILoggingService logService, ISnackbarService snackbar) 
        {
            _fileService = fileService;
            _directoryService = directoryService;
            _logService = logService;
            _snackbar = snackbar;
            SourceItems = new ObservableCollection<StorageItemVm> { FileTreeTestData.InitializeTestStorageItems() };

            TestFileCopyCommand = ReactiveCommand.Create<Unit, Unit>(n =>
                TestFileCopy(), outputScheduler: ImmediateScheduler.Instance);

            TestDirectoryListCommand = ReactiveCommand.Create<Unit, Unit>(n =>
                TestDirectoryList(), outputScheduler: ImmediateScheduler.Instance);

            LoadDirectoryContentCommand = ReactiveCommand.Create<DirectoryItemVm>(LoadDirectoryContent);


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
                        Name = settings.TargetRootPath, 
                        Path = settings.TargetRootPath 
                    }; 
                })
                .CombineLatest(nav.SelectedNavigationView, (settings, currentNav) => (settings, currentNav))
                .Where(sn => sn.currentNav?.Type == NavigationHost.NavigationLocation.FileTransfer)
                .Where(_ => TargetItems.Count == 0)
                .Subscribe(_ => LoadCurrentDirectoryContent());
        }

        private void LoadDirectoryContent(DirectoryItemVm directoryVm)
        {
            LoadDirectoryContent(directoryVm.Path);
        }

        private void LoadCurrentDirectoryContent()
        {
            LoadDirectoryContent(CurrentDirectory.Path);
        }

        private void LoadDirectoryContent(string path)
        {
            var directoryContent = new DirectoryContent();

            try
            {
                directoryContent = _directoryService.GetDirectoryContent(path);
            }
            catch (TeensyException ex)
            {
                _snackbar.Enqueue(ex.Message);
                return;
            }
            if (directoryContent is null)
            {
                _snackbar.Enqueue("Error receiving directory contents");
            }

            var directoryItems = directoryContent.Directories.Select(d => new DirectoryItemVm
            {
                Name = d.Name,
                Path = d.Path
            }).ToList();

            var fileNodes = directoryContent.Files.Select(f => new FileItemVm
            {
                Name = f.Name,
                Path = f.Path,
                Size = f.Size
            }).ToList();

            TargetItems.Clear();
            TargetItems.AddRange(directoryItems);
            TargetItems.AddRange(fileNodes);
        }

        //private void LoadDirectoryContent(DirectoryNode directoryNode)
        //{
        //    if (directoryNode.Children.Count > 1) return;

        //    var directoryContent = _fileService.GetDirectoryContent(directoryNode.Path);

        //    if (directoryContent is null) return;

        //    var directoryNodes = directoryContent.Directories.Select(d => new DirectoryNode
        //    {
        //        Name = d.Name,
        //        Path = d.Path
        //    }).ToList();                

        //    var fileNodes = directoryContent.Files.Select(f => new FileNode 
        //    { 
        //        Name = f.Name, 
        //        Path = f.Path, 
        //        Size = f.Size 
        //    }).ToList();

        //    var nodes = new List<NodeBase> { directoryNodes, fileNodes };

        //    if(nodes.Count > 0)
        //    {
        //        directoryNode.Children.Clear();
        //        directoryNode.Children.AddRange(nodes);
        //    }
        //}

        private Unit TestDirectoryList()
        {
            _directoryService.GetDirectoryContent("/");
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