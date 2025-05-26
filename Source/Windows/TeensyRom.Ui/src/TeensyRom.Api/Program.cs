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
builder.Services.AddTeensyRomMediatR();
builder.Services.AddUiCors();
builder.Services.AddTeensyRomServices();

var app = builder.Build();

app.MapApiDocs();

app.UseUiCors();
app.MapRadEndpoints();

app.Run();

public partial class Program;