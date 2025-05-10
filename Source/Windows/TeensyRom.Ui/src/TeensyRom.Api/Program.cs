using TeensyRom.Core.Games;
using TeensyRom.Core.Music.Sid;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Storage;
using TeensyRom.Core.Settings;
using MediatR;
using TeensyRom.Core.Serial.Commands.Behaviors;
using TeensyRom.Api.Services;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Commands.MuteSidVoices;
using TeensyRom.Core.Commands.PlaySubtune;
using TeensyRom.Core.Commands.SetMusicSpeed;
using TeensyRom.Core.Serial.Commands.ToggleMusic;
using TeensyRom.Core.Device;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Assets;
using TeensyRom.Core.Common;
using TeensyRom.Core.Music;

UnpackAssets();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRadEndpoints(typeof(Program));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ILoggingService, LoggingService>();
builder.Services.AddSingleton<IAlertService, AlertService>();
builder.Services.AddSingleton<ISettingsService, SettingsService>();
builder.Services.AddSingleton<IFwVersionChecker, FwVersionChecker>();
builder.Services.AddSingleton<ISerialStateContext, SerialStateContext>();
builder.Services.AddSingleton<ICartFinder, CartFinder>();
builder.Services.AddSingleton<ICartTagger, CartTagger>();
builder.Services.AddSingleton<IDeviceConnectionManager, DeviceConnectionManager>();
builder.Services.AddSingleton<IObservableSerialPort, ObservableSerialPort>();
builder.Services.AddSingleton<ISerialFactory, SerialFactory>();
builder.Services.AddSingleton<IStorageFactory, StorageFactory>();
builder.Services.AddSingleton<IGameMetadataService, GameMetadataService>();
builder.Services.AddSingleton<ISidMetadataService, SidMetadataService>();


builder.Services.AddSingleton<IMuteSidVoicesSerialRoutine, MuteSidVoicesSerialRoutine>();
builder.Services.AddSingleton<IToggleMusicSerialRoutine, ToggleMusicSerialRoutine>();
builder.Services.AddSingleton<ISetMusicSpeedSerialRoutine, SetMusicSpeedSerialRoutine>();
builder.Services.AddSingleton<IPlaySubtuneSerialRoutine, PlaySubtuneSerialRoutine>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CoreSerialAssemblyMarker>());
builder.Services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ExceptionBehavior<,>));
builder.Services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(SerialBehavior<,>));


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapRadEndpoints();
app.Run();

void UnpackAssets()
{
    AssetHelper.UnpackAssets(GameConstants.Game_Image_Local_Path, "OneLoad64.zip");
    AssetHelper.UnpackAssets(MusicConstants.Musician_Image_Local_Path, "Composers.zip");
    AssetHelper.UnpackAssets(AssetConstants.VicePath, "vice-bins.zip");
}

public partial class Program;