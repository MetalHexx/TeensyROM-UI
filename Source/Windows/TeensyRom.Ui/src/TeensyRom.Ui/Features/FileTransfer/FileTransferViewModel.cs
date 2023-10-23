using DynamicData;
using GongSolutions.Wpf.DragDrop;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using TeensyRom.Core.Files.Abstractions;
using TeensyRom.Core.Settings;

namespace TeensyRom.Ui.Features.FileTransfer
{
    public class FileTransferViewModel : ReactiveObject
    {
        public ObservableCollection<DirectoryNode> SourceItems { get; set; }
        public ObservableCollection<DirectoryNode> TargetItems { get; set; }


        [Reactive]
        public string Logs { get; set; } = string.Empty;

        public ReactiveCommand<Unit, Unit> TestFileCopyCommand { get; set; }
        public ReactiveCommand<Unit, Unit> TestDirectoryListCommand { get; set; }
        public ReactiveCommand<DirectoryNode, Unit> LoadDirectoryContentCommand { get; set; }


        private readonly ITeensyFileService _fileService;
        private readonly ISettingsService _settingsService;
        private TeensySettings _settings;
        private readonly StringBuilder _logBuilder = new StringBuilder();

        public FileTransferViewModel(ITeensyFileService fileService, ISettingsService settingsService) 
        {
            _fileService = fileService;
            _settingsService = settingsService;

            TargetItems = new ObservableCollection<DirectoryNode>();
            
            SourceItems = new ObservableCollection<DirectoryNode> { FileTreeTestData.InitializeSourceItems() };

            TestFileCopyCommand = ReactiveCommand.Create<Unit, Unit>(n =>
                TestFileCopy(), outputScheduler: ImmediateScheduler.Instance);

            TestDirectoryListCommand = ReactiveCommand.Create<Unit, Unit>(n =>
                TestDirectoryList(), outputScheduler: ImmediateScheduler.Instance);

            LoadDirectoryContentCommand = ReactiveCommand.Create<DirectoryNode>(LoadDirectoryContent);


            _fileService.Logs.Subscribe(log =>
            {
                _logBuilder.AppendLine(log);
                Logs = _logBuilder.ToString();
            });

            _settingsService.Settings.Subscribe(settings => 
            {
                _settings = settings;

                TargetItems.Clear();

                TargetItems.Add(new()
                {
                    Name = _settings.TargetRootPath,
                    Path = _settings.TargetRootPath
                }); 
            });
        }

        private void LoadDirectoryContent(DirectoryNode directoryNode)
        {
            if (directoryNode.Children.Count > 1) return;

            var directoryContent = _fileService.GetDirectoryContent(directoryNode.Path);

            if (directoryContent is null) return;

            var directoryNodes = directoryContent.Directories.Select(d => new DirectoryNode
            {
                Name = d.Name,
                Path = d.Path
            }).ToList();                

            var fileNodes = directoryContent.Files.Select(f => new FileNode 
            { 
                Name = f.Name, 
                Path = f.Path, 
                Size = f.Size 
            }).ToList();

            var nodes = new List<NodeBase> { directoryNodes, fileNodes };

            if(nodes.Count > 0)
            {
                directoryNode.Children.Clear();
                directoryNode.Children.AddRange(nodes);
            }
        }

        private Unit TestDirectoryList()
        {
            _fileService.GetDirectoryContent("/");
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

        public void DragOver(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data as DirectoryNode;

            if (sourceItem != null)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data as DirectoryNode;

            if (sourceItem != null)
            {
                TargetItems.Add(sourceItem);
                SourceItems.Remove(sourceItem);
            }
        }
    }
}