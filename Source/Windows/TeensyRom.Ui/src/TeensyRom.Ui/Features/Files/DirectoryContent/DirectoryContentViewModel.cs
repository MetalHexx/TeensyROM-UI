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
using System.Windows;
using System.Windows.Input;
using System.IO;

namespace TeensyRom.Ui.Features.Files.DirectoryContent
{
    public class DirectoryContentViewModel : ReactiveObject, IDisposable
    {
        [ObservableAsProperty] public ObservableCollection<StorageItem> DirectoryContent { get; }
        [ObservableAsProperty] public bool ShowProgress { get; }
        public ReactiveCommand<DirectoryItem, Unit> LoadDirectoryCommand { get; set; }
        public ReactiveCommand<DragEventArgs, Unit> FileDropCommand { get; private set; }
        public ReactiveCommand<DragEventArgs, Unit> DragOverCommand { get; }


        private readonly IFileState _fileState;
        private IDisposable _directoryTreeSubscription;

        public DirectoryContentViewModel(IFileState fileState)
        {
            _fileState = fileState;

            _fileState.DirectoryLoading
                .ToPropertyEx(this, x => x.ShowProgress);

            LoadDirectoryCommand = ReactiveCommand.CreateFromTask<DirectoryItem>(directory =>
                fileState.LoadDirectory(directory.Path), outputScheduler: RxApp.MainThreadScheduler);

            FileDropCommand = ReactiveCommand.CreateFromTask<DragEventArgs>(OnFileDrop);
            DragOverCommand = ReactiveCommand.Create<DragEventArgs>(OnDragOver);

            _fileState.DirectoryContent.ToPropertyEx(this, x => x.DirectoryContent);
        }

        /// <summary>
        /// Ensures only file drops are draggable onto the UI
        /// </summary>
        private void OnDragOver(DragEventArgs e)
        {            
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private Task OnFileDrop(DragEventArgs e)
        {
            var filePaths = new List<FileCopyItem>();

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var items = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (var item in items)
                {
                    if (File.Exists(item))
                    {
                        filePaths.Add(new FileCopyItem { Path = item });
                    }
                    else if (Directory.Exists(item))
                    {
                        ProcessDirectoryAsync(item, filePaths);
                    }
                }
                return _fileState.StoreFiles(filePaths);
            }
            return Task.CompletedTask;
        }

        private static void ProcessDirectoryAsync(string directoryPath, List<FileCopyItem> filePaths)
        {
            filePaths.AddRange(Directory
                .GetFiles(directoryPath)
                .Select(path => new FileCopyItem 
                { 
                    Path = path, 
                    InSubdirectory = true 
                }));

            foreach (var subdirectory in Directory.GetDirectories(directoryPath))
            {
                ProcessDirectoryAsync(subdirectory, filePaths);
            }
        }

        public void Dispose()
        {
            _directoryTreeSubscription?.Dispose();
        }
    }

    public class FileCopyItem
    {
        public string Path { get; set; } = string.Empty;
        public bool InSubdirectory { get; set; }
    }
}
