using System;
using System.Collections.Generic;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Ui.Controls.DirectoryTree
{
    public interface IDirectoryTreeState
    {
        IObservable<DirectoryNodeViewModel> DirectoryTree { get; }
        void ResetDirectoryTree(string rootPath);
        void SelectDirectory(string path);
        void Insert(IEnumerable<DirectoryItem> newDirectories);
        
    }
}