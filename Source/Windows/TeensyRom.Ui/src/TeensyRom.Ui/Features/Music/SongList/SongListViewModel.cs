using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Features.Common.Models;
using TeensyRom.Ui.Features.Music.State;
using TeensyRom.Ui.Features.NavigationHost;

namespace TeensyRom.Ui.Features.Music.SongList
{
    public class SongListViewModel: ReactiveObject, IDisposable
    {
        private readonly IMusicState _musicState;
        private readonly ISerialPortState _serialState;
        private IDisposable _loadSongsSubscription;

        [Reactive] public string CurrentPath { get; set; }
        public ObservableCollection<StorageItemVm> DirectoryContent { get; } = new();
        public ReactiveCommand<SongItemVm, Unit> PlayCommand { get; set; }
        public ReactiveCommand<DirectoryItemVm, Unit> LoadDirectoryCommand { get; set; }

        public SongListViewModel(ISettingsService settingsService, IMusicState musicState, ISerialPortState serialState, INavigationService nav)
        {
            _musicState = musicState;
            _serialState = serialState;
           
            PlayCommand = ReactiveCommand.Create<SongItemVm, Unit>(song => HandlePlayCommand(song));
            LoadDirectoryCommand = ReactiveCommand.Create<DirectoryItemVm, Unit>(directory => HandleLoadDirectoryCommand(directory));

            _loadSongsSubscription = _serialState.IsConnected
                .Where(isConnected => isConnected is true)
                .CombineLatest(settingsService.Settings, (isConnected, settings) => settings)
                .Do(settings =>
                {
                    CurrentPath = settings.TargetRootPath;
                })
                .CombineLatest(nav.SelectedNavigationView, (settings, currentNav) => (settings, currentNav))
                .Where(sn => sn.currentNav?.Type == NavigationLocation.Music)
                .Where(_ => DirectoryContent.Count == 0)
                .Subscribe(sn => LoadSongs(sn.settings.GetFileTypePath(TeensyFileType.Sid)));

            _musicState.DirectoryContent.Subscribe(dc => OnDirectoryContentChanged(dc));

        }

        private Unit OnDirectoryContentChanged(IEnumerable<StorageItemVm> dc)
        {
            DirectoryContent.Clear();
            DirectoryContent.AddRange(dc);
            return Unit.Default;
        }

        public Unit LoadSongs(string path)
        {
            _musicState.LoadDirectory(path);
            return Unit.Default;
        }

        public Unit HandlePlayCommand(SongItemVm song)
        {
            _musicState.LoadSong(song);
            return Unit.Default;
        }

        public Unit HandleLoadDirectoryCommand(DirectoryItemVm directory)
        {
            _musicState.LoadDirectory(directory.Path);
            return Unit.Default;
        }

        public void Dispose()
        {
            _loadSongsSubscription?.Dispose();
        }
    }
}