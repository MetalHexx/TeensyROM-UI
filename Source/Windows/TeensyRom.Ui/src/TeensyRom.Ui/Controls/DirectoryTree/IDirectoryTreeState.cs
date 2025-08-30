using System;
using System.Collections.Generic;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Ui.Controls.DirectoryTree
{
    public interface IDirectoryTreeState
    {
        IObservable<DirectoryNodeViewModel> DirectoryTree { get; }
        void ResetDirectoryTree(DirectoryPath rootPath);
        void SelectDirectory(DirectoryPath path);
        void Insert(IEnumerable<DirectoryItem> newDirectories);
        
    }
}