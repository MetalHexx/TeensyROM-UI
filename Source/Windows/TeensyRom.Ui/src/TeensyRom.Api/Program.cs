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
using Scalar.AspNetCore;
using TeensyRom.Api.Startup;

AssetStartupHelper.UnpackAssets();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRadEndpoints(typeof(Program));
builder.Services.AddApiDocs();

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDevServer",
        builder =>
        {
            builder.WithOrigins("http://localhost:4200") // Angular dev server
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
        });
});

var app = builder.Build();

app.MapApiDocs();

app.UseCors("AllowAngularDevServer");
app.MapRadEndpoints();

app.Run();

public partial class Program;