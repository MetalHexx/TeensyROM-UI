using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Features.Files.DirectoryContent;

namespace TeensyRom.Ui.Features.Files.State
{
    public interface IFileState
    {
        IObservable<DirectoryNodeViewModel> DirectoryTree { get; }
        IObservable<ObservableCollection<StorageItem>> DirectoryContent { get; }
        IObservable<bool> DirectoryLoading { get; }
        Task LoadDirectory(string path);
        Task StoreFiles(IEnumerable<FileCopyItem> files);
        Task RefreshDirectory(bool bustCache = true);
    }
}
