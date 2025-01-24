using TeensyRom.Core.Player;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Cli.Services
{
    internal record PlayerState
    {
        public TeensyStorageType StorageType { get; set; } = TeensyStorageType.SD;
        public ILaunchableItem? CurrentItem = null;
        public Player.PlayerState PlayState { get; set; } = Player.PlayerState.Stopped;
        public PlayMode? PlayMode { get; set; } = Core.Player.PlayMode.Random;
        public TeensyFilterType FilterType { get; set; } = TeensyFilterType.All;
        public string ScopePath { get; set; } = "/";
        public StorageScope Scope { get; set; } = StorageScope.DirDeep;
        public TimeSpan? PlayTimer { get; set; } = null;
        public SidTimer SidTimer { get; set; } = SidTimer.SongLength;
        public string? SearchQuery { get; set; } = null;
        public bool RandomInitialized { get; set; } = false;
    }
}
