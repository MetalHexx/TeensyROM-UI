using DynamicData;
using MediatR;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
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
    public class NormalPlayState : PlayerState
    {
        public NormalPlayState(PlayerContext playerContext, IMediator mediator, ICachedStorageService storage, ISettingsService settingsService, ILaunchHistory launchHistory, ISnackbarService alert, ISerialStateContext serialContext, INavigationService nav, IDirectoryTreeState tree) : base(playerContext, mediator, storage, settingsService, launchHistory, alert) { }

        public override bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(ShuffleState)
                || nextStateType == typeof(SearchState);
        }

        public override async Task<ILaunchableItem?> GetNext(ILaunchableItem currentLaunchable, TeensyFilterType filter, DirectoryState directoryState)
        {
            var parentPath = currentLaunchable.Path.GetUnixParentPath();
            var directoryResult = await _storage.GetDirectory(parentPath);

            if (directoryResult is null) return null;

            var launchableItems = directoryResult.Files
                .OfType<ILaunchableItem>()
                .ToList();

            var currentIndex = launchableItems.IndexOf(currentLaunchable);

            var nextFile = launchableItems.Count == currentIndex + 1
                ? launchableItems.First()
                : launchableItems[++currentIndex];

            if (nextFile is ILaunchableItem f)
            {
                if (f.Path == currentLaunchable.Path) return null;

                await _playerContext.LoadDirectory(f.Path.GetUnixParentPath(), f.Path);
                return f;
            }
            return null;
        }

        public override async Task<ILaunchableItem?> GetPrevious(ILaunchableItem currentFile, TeensyFilterType filter, DirectoryState directoryState)
        {
            var parentPath = currentFile.Path.GetUnixParentPath();
            var directoryResult = await _storage.GetDirectory(parentPath);
            var launchableItems = directoryResult?.Files.OfType<ILaunchableItem>().ToList();

            if (launchableItems is null)
            {
                return currentFile;                
            }
            var index = launchableItems.IndexOf(currentFile);

            var nextFile = index == 0
                ? launchableItems.Last()
                : launchableItems[--index];

            if (nextFile is null) return null;

            await _playerContext.LoadDirectory(nextFile.Path.GetUnixParentPath(), nextFile.Path);            
            return nextFile;            
        }
    }
}
