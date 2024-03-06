using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Features.Files.DirectoryContent;
using TeensyRom.Ui.Features.Common.Models;
using FileCopyItem = TeensyRom.Ui.Features.Files.DirectoryContent.FileCopyItem;

namespace TeensyRom.Ui.Features.Files.State
{
    public interface IFileState
    {   
        IObservable<DirectoryNodeViewModel> DirectoryTree { get; }
        IObservable<ObservableCollection<IStorageItem>> DirectoryContent { get; }
        IObservable<ILaunchableItem> FileLaunched { get; }
        IObservable<int> CurrentPage { get; }
        IObservable<int> TotalPages { get; }
        IObservable<bool> PagingEnabled { get; }

        Task LoadDirectory(string path, string? filePathToSelect = null);
        Task StoreFiles(IEnumerable<FileCopyItem> files);
        Task LaunchFile(ILaunchableItem file);
        Task RefreshDirectory(bool bustCache = true);
        Task SaveFavorite(ILaunchableItem file);
        Task DeleteFile(IFileItem file);
        Task PlayRandom();
        Task NextPage();
        Task PreviousPage();
        Task SetPageSize(int pageSize);
        Task CacheAll();
        Unit SearchFiles(string keyword);
        Task ClearSearch();
    }
}
