using MediatR;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.RateLimiting;
using Scalar.AspNetCore;
using System.Threading.RateLimiting;
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


builder.Services
    .AddApiDocs()
    .AddTeensyRomMediatR()
    .AddUiCors()
    .AddTeensyRomServices()
    .AddStrictRateLimiting()
    .AddProblemDetailsWithLogging()
    .AddRadEndpoints(typeof(Program));

var app = builder.Build();

app.UseUiCors();
app.UseRateLimiter();
app.MapApiDocs();
app.MapRadEndpoints();

app.Run();

public partial class Program;