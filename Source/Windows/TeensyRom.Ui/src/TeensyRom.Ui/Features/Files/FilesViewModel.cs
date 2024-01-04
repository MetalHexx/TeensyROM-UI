using ReactiveUI;
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
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Features.Files.DirectoryContent;
using TeensyRom.Ui.Features.Files.State;
using TeensyRom.Ui.Features.Music.State;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Helpers.ViewModel;

namespace TeensyRom.Ui.Features.Files
{
    public class FilesViewModel : FeatureViewModelBase
    {
        [Reactive] public DirectoryTreeViewModel DirectoryTree { get; set; }
        [Reactive] public DirectoryContentViewModel DirectoryContent { get; set; }

        private readonly IFileState _fileState;

        public FilesViewModel(ISettingsService settings, INavigationService nav, ISerialPortState serialState, IFileState fileState, DirectoryContentViewModel directoryContent)
        {
            FeatureTitle = "Files";            
            DirectoryContent = directoryContent;
            _fileState = fileState;

            serialState.IsConnected
                .Where(isConnected => isConnected is true)
                .CombineLatest(settings.Settings, (isConnected, settings) => settings)
                .CombineLatest(nav.SelectedNavigationView, (settings, currentNav) => (settings, currentNav))
                .Where(sn => sn.currentNav?.Type == NavigationLocation.Files)
                .Select(sn => sn.settings.TargetRootPath)
                .Take(1)
                .Subscribe(root => _fileState.LoadDirectory(root));

            DirectoryTree = new(fileState.DirectoryTree)
            {
                DirectorySelectedCommand = ReactiveCommand.CreateFromTask<DirectoryNodeViewModel>(async (directory) =>
                await _fileState.LoadDirectory(directory.Path), outputScheduler: RxApp.MainThreadScheduler)
            };
        }
    }
}