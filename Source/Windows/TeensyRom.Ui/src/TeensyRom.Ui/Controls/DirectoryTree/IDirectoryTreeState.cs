using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Ui.Core.Storage.Entities;

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