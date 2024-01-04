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

namespace TeensyRom.Ui.Features.Files.DirectoryContent
{
    public class DirectoryContentViewModel : ReactiveObject, IDisposable
    {
        [ObservableAsProperty] public ObservableCollection<StorageItem> DirectoryContent { get; }
        [ObservableAsProperty] public bool ShowProgress { get; }
        public ReactiveCommand<SongItem, bool> PlayCommand { get; set; }
        public ReactiveCommand<SongItem, bool> SaveFavoriteCommand { get; set; }
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

            //PlayCommand = ReactiveCommand.CreateFromTask<SongItem, bool>(song =>
            //    fileState.LoadSong(song), outputScheduler: RxApp.MainThreadScheduler);

            //SaveFavoriteCommand = ReactiveCommand.CreateFromTask<SongItem, bool>(
            //    fileState.SaveFavorite,
            //    outputScheduler: RxApp.MainThreadScheduler);

            LoadDirectoryCommand = ReactiveCommand.CreateFromTask<DirectoryItem>(directory =>
                fileState.LoadDirectory(directory.Path), outputScheduler: RxApp.MainThreadScheduler);

            FileDropCommand = ReactiveCommand.CreateFromTask<DragEventArgs>(OnFileDrop);
            DragOverCommand = ReactiveCommand.Create<DragEventArgs>(OnDragOver);

            _fileState.DirectoryContent.ToPropertyEx(this, x => x.DirectoryContent);
        }

        private Task OnFileDrop(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                return _fileState.StoreFile(files[0]);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Ensures only file types are draggable onto the UI
        /// </summary>
        private void OnDragOver(DragEventArgs e)
        {
            var availableFormats = e.Data.GetFormats();
            System.Diagnostics.Debug.WriteLine("Available formats: " + string.Join(", ", availableFormats));

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }
        public void Dispose()
        {
            _directoryTreeSubscription?.Dispose();
        }
    }
}
