using MediatR;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Features.Common.State.Directory;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Common.State.Player
{
    public class ShuffleState : PlayerState
    {
        public ShuffleState(PlayerContext playerContext, IMediator mediator, ICachedStorageService storage, ISettingsService settingsService, ILaunchHistory launchHistory, ISnackbarService alert, ISerialStateContext serialContext, INavigationService nav, IDirectoryTreeState tree) : base(playerContext, mediator, storage, settingsService, launchHistory, alert) { }

        public override bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(NormalPlayState)
                || nextStateType == typeof(SearchState);
        }

        public override async Task<ILaunchableItem?> GetNext(ILaunchableItem currentFile, TeensyLibraryType libraryType, DirectoryState directoryState)
        {
            var nextFile = _launchHistory.GetNext(_playerContext.GetFileTypes());

            if (nextFile is not null)
            {
                await _playerContext.LoadDirectory(nextFile.Path.GetUnixParentPath(), nextFile.Path);
                return nextFile;
            }
            var libraryPath = _settings.GetLibraryPath(libraryType);
            
            var randomFile = _storage.GetRandomFile(libraryPath, _playerContext.GetFileTypes());
            
            if(randomFile is not null)
            {
                _playerContext.UpdateHistory(randomFile);
                await _playerContext.LoadDirectory(randomFile.Path.GetUnixParentPath(), randomFile.Path);
            }
            return randomFile;            
        }

        public override async Task<ILaunchableItem?> GetPrevious(ILaunchableItem currentFile, TeensyLibraryType libraryType, DirectoryState directoryState)
        {
            var file = _launchHistory.GetPrevious(_playerContext.GetFileTypes());

            if (file is not null)
            {
                await _playerContext.LoadDirectory(file.Path.GetUnixParentPath(), file.Path);
                return file;
            }
            return currentFile;
        }
    }
}
