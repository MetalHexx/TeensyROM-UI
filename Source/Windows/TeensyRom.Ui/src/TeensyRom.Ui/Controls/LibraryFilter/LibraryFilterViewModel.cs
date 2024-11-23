using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Ui.Core.Settings;

namespace TeensyRom.Ui.Controls.LibraryFilter
{
    public class LibraryFilterViewModel : ReactiveObject
    {
        [Reactive] public ObservableCollection<TeensyFilter> Libraries { get; set; }
        [Reactive] public TeensyFilter? SelectedLibrary { get; set; }
        public ReactiveCommand<TeensyFilter, Unit> FilterCommand { get; }
        public ReactiveCommand<Unit, Unit> PlayRandomCommand { get; set; }
        public LibraryFilterViewModel(IEnumerable<TeensyFilter> libraries, TeensyFilter? selectedLibrary, IObservable<Unit> randomLaunched,  Func<TeensyFilter, Task> filterFunc, Func<Task> launchRandomFunc)
        {
            SelectedLibrary = selectedLibrary;
            Libraries = new ObservableCollection<TeensyFilter>(libraries);

            randomLaunched.Subscribe(_ => MessageBus.Current.SendMessage(new RandomLaunchMessage()));

            FilterCommand = ReactiveCommand.CreateFromTask<TeensyFilter, Unit>(async lib => 
            {
                SelectedLibrary = lib;
                await filterFunc(lib);
                return Unit.Default;
            });

            PlayRandomCommand = ReactiveCommand.CreateFromTask(
                execute: launchRandomFunc,
                outputScheduler: RxApp.MainThreadScheduler);
        }
    }
}
