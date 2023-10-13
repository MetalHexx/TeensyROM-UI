using GongSolutions.Wpf.DragDrop;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Text;
using System.Windows;
using TeensyRom.Core.Files.Abstractions;

namespace TeensyRom.Ui.Features.FileTransfer
{
    public class FileTransferViewModel : ReactiveObject
    {
        public ObservableCollection<DirectoryNode> SourceItems { get; set; } = new ObservableCollection<DirectoryNode>();
        public ObservableCollection<DirectoryNode> TargetItems { get; set; } = new ObservableCollection<DirectoryNode>();


        [Reactive]
        public string Logs { get; set; } = string.Empty;

        public ReactiveCommand<Unit, Unit> TestFileCopyCommand { get; set; }

        private readonly ITeensyFileService _fileService;
        private readonly StringBuilder _logBuilder = new StringBuilder();

        public FileTransferViewModel(ITeensyFileService fileService) 
        {
            _fileService = fileService;
            SourceItems = new ObservableCollection<DirectoryNode> { FileTreeTestData.InitializeSourceItems() };
            TargetItems = new ObservableCollection<DirectoryNode> { FileTreeTestData.InitializeTargetItems() };

            TestFileCopyCommand = ReactiveCommand.Create<Unit, Unit>(n =>
                TestFileCopy(), outputScheduler: ImmediateScheduler.Instance);

            _fileService.Logs.Subscribe(log =>
            {
                _logBuilder.AppendLine(log);
                Logs = _logBuilder.ToString();
            });

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
