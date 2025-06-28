using MediatR;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.RateLimiting;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using TeensyRom.Api.Endpoints.GetDeviceEvents;
using TeensyRom.Api.Endpoints.Serial.GetLogs;
using TeensyRom.Api.Http;
using TeensyRom.Api.Services;
using TeensyRom.Api.Startup;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Assets;
using TeensyRom.Core.Commands.MuteSidVoices;
using TeensyRom.Core.Commands.PlaySubtune;
using TeensyRom.Core.Commands.SetMusicSpeed;
using TeensyRom.Core.Common;
using TeensyRom.Core.Device;
using TeensyRom.Core.Games;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Music;
using TeensyRom.Core.Music.Sid;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.Commands.Behaviors;
using TeensyRom.Core.Serial.Commands.ToggleMusic;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage;

AssetStartupHelper.UnpackAssets();

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
builder.Logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);

builder.Services.AddSignalR();

builder.Services
    .AddApiDocs()
    .AddTeensyRomMediatR()
    .AddUiCors()
    .AddTeensyRomServices()
    .AddStrictRateLimiting()
    .AddProblemDetailsWithLogging()
    .AddRadEndpoints(typeof(Program));

builder.Services.AddSignalR().AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

app.UseUiCors();
app.UseRateLimiter();
app.MapApiDocs();
app.MapRadEndpoints();
app.MapHub<LogsHub>("/logHub");
app.MapHub<DeviceEventHub>("/deviceEventHub");

app.Run();

//TODO: Put this somewhere else.
AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
{
    Console.WriteLine("UNHANDLED: " + args.ExceptionObject);
};

public partial class Program;