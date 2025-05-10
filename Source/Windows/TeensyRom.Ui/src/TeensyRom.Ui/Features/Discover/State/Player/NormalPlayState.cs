﻿using MediatR;
using System;
using System.Linq;
using System.Reactive.Linq;
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
                .Where(f => _playerContext.GetFileTypes().Any(type => f.FileType == type))                
                .ToList();

            var currentFile = launchableItems.FirstOrDefault(s => s.Id == currentLaunchable.Id);

            if (currentFile is null) return null;

            var currentIndex = launchableItems.IndexOf(currentFile);

            var nextFile = launchableItems.Count == currentIndex + 1
                ? launchableItems.First()
                : launchableItems[++currentIndex];

            if (nextFile is not ILaunchableItem) return null;

            if (nextFile.Path == currentLaunchable.Path) return currentLaunchable;

            if (_settings.NavToDirOnLaunch)
            {
                await _playerContext.LoadDirectory(nextFile.Path.GetUnixParentPath(), nextFile.Path);
            }
            return nextFile;
        }

        public override async Task<ILaunchableItem?> GetPrevious(ILaunchableItem currentFile, TeensyFilterType filter, DirectoryState directoryState)
        {
            var parentPath = currentFile.Path.GetUnixParentPath();
            var directoryResult = await _storage.GetDirectory(parentPath);

            var launchableItems = directoryResult?.Files
                .OfType<ILaunchableItem>()
                .Where(f => _playerContext.GetFileTypes().Any(type => f.FileType == type))
                .ToList();

            if (launchableItems is null)
            {
                return currentFile;
            }
            var index = launchableItems.IndexOf(currentFile);

            index = index < 0 ? 0 : index;

            if (!launchableItems.Any()) return null;

            var nextFile = index == 0
                ? launchableItems.Last()
                : launchableItems[--index];

            if (nextFile is null) return null;

            if (_settings.NavToDirOnLaunch)
            {
                await _playerContext.LoadDirectory(nextFile.Path.GetUnixParentPath(), nextFile.Path);
            }
            return nextFile;
        }
    }
}
