using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TeensyRom.Core.Commands.MuteSidVoices;
using TeensyRom.Core.Commands.PlaySubtune;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Music;
using TeensyRom.Core.Serial.Commands.Composite.StartSeek;
using TeensyRom.Core.Settings;
using TeensyRom.Core.ValueObjects;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Features.Common.Models;

namespace TeensyRom.Ui.Features.Discover.State.Player
{
    public interface IPlayerContext
    {
        TeensyFileType[] GetFileTypes();
        IObservable<DirectoryPath> CurrentPath { get; }
        IObservable<int> CurrentPage { get; }
        IObservable<PlayState> PlayingState { get; }
        IObservable<PlayerState> CurrentState { get; }
        IObservable<ObservableCollection<StorageItem>> DirectoryContent { get; }
        IObservable<DirectoryNodeViewModel?> DirectoryTree { get; }
        IObservable<bool> PagingEnabled { get; }
        IObservable<LaunchedFileResult> LaunchedFile { get; }
        IObservable<LaunchableItem> SelectedFile { get; }
        IObservable<int> TotalPages { get; }
        IObservable<StorageScope> CurrentScope { get; }
        IObservable<DirectoryPath> CurrentScopePath { get; }
        Task CacheAll();
        Task ClearSearch();
        Task DeleteFile(FileItem file);
        Task LoadDirectory(DirectoryPath path);
        Task LoadDirectory(DirectoryPath path, FilePath? filePathToSelect = null);
        Task PlayFile(LaunchableItem file);
        Task TogglePlay();
        Unit NextPage();
        Task PlayNext();
        Task PlayPrevious();
        Task<PlaySubtuneResult?> PlaySubtune(int subtuneIndex);
        Task<LaunchableItem?> PlayRandom();
        void UpdateHistory(LaunchableItem fileToLoad);
        Unit PreviousPage();
        Task RefreshDirectory(bool bustCache = true);
        Unit SearchFiles(string keyword);
        Unit SetPageSize(int pageSize);
        Unit SelectFile(LaunchableItem file);
        Task StopFile();
        Unit ToggleShuffleMode();
        bool TryTransitionTo(Type nextStateType);
        Task SwitchFilterAndLaunch(TeensyFilter filter);
        void SetScope(StorageScope scope);
        void SetScopePath(DirectoryPath path);
        DirectoryPath GetScopePath();
        Task SetSpeed(double percentage, MusicSpeedCurveTypes curveType);
        Task RestartSong();
        Task<PlaySubtuneResult?> RestartSubtune(int subtuneIndex);
        Task Mute(VoiceState voice1, VoiceState voice2, VoiceState voice3);
        Task StartSeek(int subtuneIndex, bool togglePlay, bool muteVoices, double seekSpeed, SeekDirection direction);
        Task EndSeek(bool enableVoices, double originalSpeed, MusicSpeedCurveTypes speedCurve);
        Task FastForward(bool togglePlay, bool muteVoices, double speed);
        Task EndFastForward(bool enableVoices, double originalSpeed, MusicSpeedCurveTypes originalSpeedCurve);

    }
}