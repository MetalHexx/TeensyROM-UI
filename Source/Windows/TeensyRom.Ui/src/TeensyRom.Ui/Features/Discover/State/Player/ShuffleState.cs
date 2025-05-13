using MediatR;
using System;
using System.Threading.Tasks;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Features.Discover.State.Directory;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Discover.State.Player
{
    public class ShuffleState : PlayerState
    {
        public ShuffleState(PlayerContext playerContext, IMediator mediator, ICachedStorageService storage, ISettingsService settingsService, ILaunchHistory launchHistory, ISnackbarService alert, ISerialStateContext serialContext, INavigationService nav, IDirectoryTreeState tree) : base(playerContext, mediator, storage, settingsService, launchHistory, alert) { }

        public override bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(NormalPlayState)
                || nextStateType == typeof(SearchState);
        }

        public override async Task<ILaunchableItem?> GetNext(ILaunchableItem currentFile, TeensyFilterType filter, DirectoryState directoryState)
        {
            var fileTypes = _playerContext.GetFileTypes();

            var nextFile = _launchHistory.GetNext(fileTypes);

            if (nextFile is not null)
            {
                await _playerContext.LoadDirectory(nextFile.Path.GetUnixParentPath(), nextFile.Path);
                return nextFile;
            }
            var currentScope = _playerContext.GetScope();
            var currentScopePath = currentScope switch 
            { 
                StorageScope.DirShallow => _playerContext.GetScopePath(), 
                StorageScope.DirDeep => _playerContext.GetScopePath(), 
                _ => StorageHelper.Remote_Path_Root 
            };
            var randomFile = _storage.GetRandomFile(currentScope, currentScopePath, fileTypes);

            if (randomFile is not null)
            {
                _playerContext.UpdateHistory(randomFile);

                if (_settings.NavToDirOnLaunch)
                {
                    await _playerContext.LoadDirectory(randomFile.Path.GetUnixParentPath(), randomFile.Path);
                }
            }
            return randomFile;
        }

        public override async Task<ILaunchableItem?> GetPrevious(ILaunchableItem currentFile, TeensyFilterType filter, DirectoryState directoryState)
        {
            var file = _launchHistory.GetPrevious(_playerContext.GetFileTypes());

            if (file is not null && _settings.NavToDirOnLaunch)
            {
                await _playerContext.LoadDirectory(file.Path.GetUnixParentPath(), file.Path);
                return file;
            }
            return currentFile;
        }
    }
}
