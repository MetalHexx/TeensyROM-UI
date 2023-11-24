using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Storage.Entities;
using System.Reactive;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace TeensyRom.Ui.Controls.DirectoryTree
{
    public class DirectoryTreeViewModel : ReactiveObject
    {        
        [ObservableAsProperty] public DirectoryItem DirectoryTree { get; }
        public ReactiveCommand<DirectoryItem, Unit> DirectorySelectedCommand { get; set; }

        public DirectoryTreeViewModel(IObservable<DirectoryItem> directoryTreeObservable)
        {
            directoryTreeObservable.ToPropertyEx(this, x => x.DirectoryTree);
        }
    }
}
