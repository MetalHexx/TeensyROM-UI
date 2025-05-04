using RadEndpoints;
using System.Runtime.CompilerServices;
using TeensyRom.Core.Games;
using TeensyRom.Core.Music.Sid;
using TeensyRom.Core.Serial.State;
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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRadEndpoints(typeof(Program));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ILoggingService, LoggingService>();
builder.Services.AddSingleton<IAlertService, AlertService>();
builder.Services.AddSingleton<ISettingsService, SettingsService>();
builder.Services.AddSingleton<IFwVersionChecker, FwVersionChecker>();
builder.Services.AddSingleton<ICachedStorageService, CachedStorageService>();
builder.Services.AddSingleton<ICartFinder, CartFinder>();
builder.Services.AddSingleton<IDeviceConnectionManager, DeviceConnectionManager>();
builder.Services.AddSingleton<IStorageCache, StorageCache>();
builder.Services.AddSingleton<IObservableSerialPort, ObservableSerialPort>();
builder.Services.AddSingleton<ISerialStateContext, SerialStateContext>();
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

public partial class Program;