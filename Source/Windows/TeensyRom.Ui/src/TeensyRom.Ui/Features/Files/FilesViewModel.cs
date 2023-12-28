using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Features.Files.State;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Helpers.ViewModel;

namespace TeensyRom.Ui.Features.Files
{
    public class FilesViewModel : FeatureViewModelBase
    {
        private readonly IFileState _fileState;

        public FilesViewModel(ISettingsService settings, INavigationService nav, ISerialPortState serialState, IFileState fileState)
        {
            FeatureTitle = "Files";

            serialState.IsConnected
                .Where(isConnected => isConnected is true)
                .CombineLatest(settings.Settings, (isConnected, settings) => settings)
                .CombineLatest(nav.SelectedNavigationView, (settings, currentNav) => (settings, currentNav))
                .Where(sn => sn.currentNav?.Type == NavigationLocation.Files)
                .Take(1)
                .Subscribe(sn => LoadDirectory(sn.settings.TargetRootPath));
            _fileState = fileState;
        }

        public Unit LoadDirectory(string path)
        {
            _fileState.LoadDirectory(path);
            return Unit.Default;
        }
    }
}
