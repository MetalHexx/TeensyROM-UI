using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Subjects;

namespace TeensyRom.Ui.Controls.DirectoryTree
{
    public class DirectoryTreeViewModel : ReactiveObject
    {
        [ObservableAsProperty] public DirectoryNodeViewModel RootDirectory { get; }

        public ReactiveCommand<DirectoryNodeViewModel, Unit> DirectorySelectedCommand { get; set; }

        public DirectoryTreeViewModel(IObservable<DirectoryNodeViewModel> directoryTreeObservable)
        {
            directoryTreeObservable.ToPropertyEx(this, x => x.RootDirectory);
        }
    }
}
