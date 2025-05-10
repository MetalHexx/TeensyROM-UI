using MediatR;
using System;
using System.Linq;
using System.Threading.Tasks;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Features.Discover.State.Directory;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Discover.State.Player
{
    public class SearchState : PlayerState
    {
        public SearchState(PlayerContext playerContext, IMediator mediator, ICachedStorageService storage, ISettingsService settingsService, ILaunchHistory launchHistory, ISnackbarService alert, ISerialStateContext serialContext, INavigationService nav, IDirectoryTreeState tree) : base(playerContext, mediator, storage, settingsService, launchHistory, alert) { }

        public override bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(NormalPlayState)
                || nextStateType == typeof(ShuffleState);
        }

        public override async Task<ILaunchableItem?> GetNext(ILaunchableItem currentFile, TeensyFilterType filter, DirectoryState directoryState)
        {
            await Task.CompletedTask;

            var currentIndex = directoryState.DirectoryContent
                .OfType<ILaunchableItem>()
                .ToList()
                .IndexOf(currentFile);

            var nextFile = directoryState.DirectoryContent.Count == currentIndex + 1
                ? directoryState.DirectoryContent.First()
                : directoryState.DirectoryContent[++currentIndex];

            if (nextFile.Path == currentFile.Path) return null;

            if (nextFile is ILaunchableItem f)
            {
                return f;
            }
            return currentFile;
        }

        public override async Task<ILaunchableItem?> GetPrevious(ILaunchableItem currentFile, TeensyFilterType filter, DirectoryState directoryState)
        {
            await Task.CompletedTask;

            var currentIndex = directoryState.DirectoryContent
                .OfType<ILaunchableItem>()
                .ToList()
                .IndexOf(currentFile);

            IStorageItem file;

            if (directoryState.DirectoryContent.Count == 0) return null;

            file = currentIndex > 0
                ? directoryState.DirectoryContent[--currentIndex]
                : directoryState.DirectoryContent.Last();

            if (file is ILaunchableItem f)
            {
                return f;
            }
            return currentFile;
        }
    }
}
