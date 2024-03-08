using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Settings;

namespace TeensyRom.Ui.Controls.LibraryFilter
{
    public class LibraryFilterViewModel : ReactiveObject
    {
        [Reactive] public ObservableCollection<TeensyLibrary> Libraries { get; set; }
        [Reactive] public TeensyLibrary SelectedLibrary { get; set; }
        public ReactiveCommand<TeensyLibrary, Unit> FilterCommand { get; }
        public LibraryFilterViewModel(IEnumerable<TeensyLibrary> libraries, Func<TeensyLibrary, Task> filterFunc)
        {
            Libraries = new ObservableCollection<TeensyLibrary>(libraries);
            FilterCommand = ReactiveCommand.CreateFromTask<TeensyLibrary, Unit>(async lib => 
            {
                SelectedLibrary = lib;
                await filterFunc(lib);
                return Unit.Default;
            });
        }
    }
}
