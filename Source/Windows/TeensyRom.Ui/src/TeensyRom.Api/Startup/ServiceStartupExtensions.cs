using TeensyRom.Api.Services;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Device;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Games;
using TeensyRom.Core.Music.Sid;
using TeensyRom.Core.Storage;
using TeensyRom.Core.Commands.MuteSidVoices;
using TeensyRom.Core.Commands.SetMusicSpeed;
using TeensyRom.Core.Commands.PlaySubtune;
using TeensyRom.Core.Serial.Commands.ToggleMusic;
using TeensyRom.Core.Serial;

namespace TeensyRom.Api.Startup
{
    public static class ServiceStartupExtensions
    {
        /// <summary>
        /// Registers TeensyROM application services as singletons.
        /// </summary>
        public static IServiceCollection AddTeensyRomServices(this IServiceCollection services)
        {
            services.AddSingleton<ILoggingService, LoggingService>();
            services.AddSingleton<IAlertService, AlertService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IFwVersionChecker, FwVersionChecker>();
            services.AddSingleton<ISerialStateContext, SerialStateContext>();
            services.AddSingleton<ICartFinder, CartFinder>();
            services.AddSingleton<ICartTagger, CartTagger>();
            services.AddSingleton<IDeviceConnectionManager, DeviceConnectionManager>();
            services.AddSingleton<IObservableSerialPort, ObservableSerialPort>();
            services.AddSingleton<ISerialFactory, SerialFactory>();
            services.AddSingleton<IStorageFactory, StorageFactory>();
            services.AddSingleton<IGameMetadataService, GameMetadataService>();
            services.AddSingleton<ISidMetadataService, SidMetadataService>();
            services.AddSingleton<IMuteSidVoicesSerialRoutine, MuteSidVoicesSerialRoutine>();
            services.AddSingleton<IToggleMusicSerialRoutine, ToggleMusicSerialRoutine>();
            services.AddSingleton<ISetMusicSpeedSerialRoutine, SetMusicSpeedSerialRoutine>();
            services.AddSingleton<IPlaySubtuneSerialRoutine, PlaySubtuneSerialRoutine>();
            return services;
        }
    }
}