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
        IObservable<int> CurrentPage { get; }
        IObservable<int> TotalPages { get; }
        IObservable<int> PageSize { get; }
        IObservable<DirectoryNodeViewModel> DirectoryTree { get; }
        IObservable<ObservableCollection<StorageItem>> DirectoryContent { get; }
        IObservable<bool> DirectoryLoading { get; }
        IObservable<bool> PagingEnabled { get; }

        Task LoadDirectory(string path, string? filePathToSelect = null);
        Task StoreFiles(IEnumerable<FileCopyItem> files);
        Task LaunchFile(FileItem file);
        Task RefreshDirectory(bool bustCache = true);
        Task SaveFavorite(FileItem file);
        Task DeleteFile(FileItem file);
        Task PlayRandom();
        Task NextPage();
        Task PreviousPage();
        Task SetPageSize(int pageSize);
        Task CacheAll();
    }
}
