using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using TeensyRom.Cli.Commands.Main;
using TeensyRom.Cli.Commands.Main.Asid;
using TeensyRom.Cli.Commands.Main.Launcher;
using TeensyRom.Cli.Fonts;
using TeensyRom.Cli.Helpers;
using TeensyRom.Cli.Services;
using TeensyRom.Cli.Core;
using TeensyRom.Cli.Core.Common;
using TeensyRom.Cli.Core.Games;
using TeensyRom.Cli.Core.Logging;
using TeensyRom.Cli.Core.Music;
using TeensyRom.Cli.Core.Music.Sid;
using TeensyRom.Cli.Core.Progress;
using TeensyRom.Cli.Core.Serial;
using TeensyRom.Cli.Core.Serial.State;
using TeensyRom.Cli.Core.Settings;
using TeensyRom.Cli.Core.Storage.Services;
using AssemblyExtensions = TeensyRom.Cli.Core.Common.AssemblyExtensions;
using TeensyRom.Cli.Core.Commands.Behaviors;
using TeensyRom.Core.Assets;
public class Program
{
    private static int Main(string[] args)
    {
        AnsiConsole.WriteLine();
        RadHelper.RenderLogo("TeensyROM", FontConstants.FontPath);

        var services = new ServiceCollection();
        var logService = new CliLoggingService();
        var alertService = new CliAlertService();        
        var serial = new ObservableSerialPort(logService, alertService);
        var serialState = new SerialStateContext(serial);
        var settingsService = new SettingsService(logService);        
        var gameService = new GameMetadataService(logService);
        var sidService = new SidMetadataService(settingsService);        

        var settings = settingsService.GetSettings();
        logService.Enabled = settings.EnableDebugLogs;

        //UnpackAssets();

        services.AddSingleton<IObservableSerialPort>(serial);
        services.AddSingleton<ISerialStateContext>(serialState);
        services.AddSingleton<ILoggingService>(logService);
        services.AddSingleton<ICliLoggingService>(logService);
        services.AddSingleton<IAlertService>(alertService);
        services.AddSingleton<ISettingsService>(settingsService);
        services.AddSingleton<IGameMetadataService>(gameService);
        services.AddSingleton<ISidMetadataService>(sidService);
        services.AddSingleton<ICachedStorageService, CachedStorageService>();
        services.AddSingleton<IPlayerService, PlayerService>();
        services.AddSingleton<ITypeResolver, TypeResolver>();
        services.AddSingleton<IProgressTimer, ProgressTimer>();
        services.AddSingleton<ILaunchHistory, LaunchHistory>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CoreAssemblyMarker>());
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ExceptionBehavior<,>));
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(SerialBehavior<,>));

        var registrar = new TypeRegistrar(services);
        
        var app = new CommandApp(registrar);

        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.SetInterceptor(new CommandInterceptor(serialState));

            config.PropagateExceptions();

            config.SetApplicationName("TeensyROM.Cli".GetOsFriendlyPath());
            config.SetApplicationVersion($"Version: {AssemblyExtensions.GetVersion(Assembly.GetExecutingAssembly())}\r\nOS: {RuntimeInformation.OSDescription}\r\nArchitecture: { RuntimeInformation.OSArchitecture }");

            config.AddBranch<LaunchSettings>("launch", launch =>
            {
                launch.SetDefaultCommand<LaunchCommand>();
                config.AddExample(LaunchSettings.Example);
                launch.SetDescription(LaunchSettings.Description);
                launch.AddExample(LaunchSettings.Example);

                config.AddExample(RandomSettings.Example);
                launch.AddCommand<RandomCommand>("random")
                      .WithAlias("r")
                      .WithDescription(RandomSettings.Description)
                      .WithExample(RandomSettings.Example);

                config.AddExample(NavigateSettings.Example);
                launch.AddCommand<NavigateCommand>("nav")
                      .WithAlias("n")
                      .WithDescription(NavigateSettings.Description)
                      .WithExample(NavigateSettings.Example);

                config.AddExample(SearchSettings.Example);
                launch.AddCommand<SearchCommand>("search")
                      .WithAlias("s")
                      .WithDescription(SearchSettings.Description)
                      .WithExample(SearchSettings.Example);

                config.AddExample(FileLaunchSettings.Example);
                launch.AddCommand<FileLaunchCommand>("file")
                      .WithAlias("s")
                      .WithDescription(FileLaunchSettings.Description)
                      .WithExample(FileLaunchSettings.Example);

                launch.AddCommand<PlayerCommand>("player")
                      .IsHidden();
            });

            config.AddExample(IndexSettings.Example);
            config.AddCommand<IndexCommand>("index")
                  .WithAlias("i")
                  .WithDescription(IndexSettings.Description)
                  .WithExample(IndexSettings.Example);

            config.AddExample(PortListSettings.Example);
            config.AddCommand<PortListCommand>("ports")
                  .WithAlias("p")
                  .WithDescription(PortListSettings.Description)
                  .WithExample(PortListSettings.Example);

            config.AddExample(GeneratePresetsSettings.Example);
            config.AddCommand<GeneratePresetsCommand>("chipsynth")
                  .WithAlias("cs")
                  .WithDescription(GeneratePresetsSettings.Description)
                  .WithExample(GeneratePresetsSettings.Example);

            config.AddCommand<SettingsCommand>("settings")
                  .IsHidden();

            config.AddExample(CreditsSettings.Example);
            config.AddCommand<CreditsCommand>("credits")
                  .WithAlias("c")
                  .WithDescription(CreditsSettings.Description)
                  .WithExample(CreditsSettings.Example);

            var help = config.Settings.HelpProviderStyles;
            help!.Arguments!.Header = new Style(foreground: RadHelper.Theme.Secondary.Color);
            help!.Description!.Header = new Style(foreground: RadHelper.Theme.Secondary.Color);
            help!.Usage!.Header = new Style(foreground: RadHelper.Theme.Secondary.Color);
            help!.Options!.Header = new Style(foreground: RadHelper.Theme.Secondary.Color);
            help!.Examples!.Header = new Style(foreground: RadHelper.Theme.Secondary.Color);
            help!.Commands!.Header = new Style(foreground: RadHelper.Theme.Secondary.Color);
            config.Settings.HelpProviderStyles = help;

        });

        //logService.Logs.Subscribe(log => AnsiConsole.Markup($"{log}\r\n\r\n"));

        var resultCode = 0;

        while (true)
        {
            try
            {
                if (args.Contains("-h") || args.Contains("--help") || args.Contains("-v") || args.Contains("--version"))
                {
                    app.Run(args);
                    return -99;
                }

                if (args.Length > 0)
                {
                    var help = args.ToList();
                    help.Add("-h");
                    AnsiConsole.WriteLine();

                    resultCode = app.Run(help.ToArray());
                    resultCode = app.Run(args);
                    args = [];
                }

                var menuChoice = PromptHelper.ChoicePrompt("Choose wisely", ["Launch", "Settings", "Index Files", "List Ports", "Generate ASID Patches", "Credits", "Leave"]);

                AnsiConsole.WriteLine();

                if (menuChoice == "Leave") return 0;

                args = menuChoice switch
                {   
                    "Launch" => ["launch"],    
                    "Index Files" => ["index"],
                    "Settings" => ["settings"],
                    "List Ports" => ["ports"],
                    "Generate ASID Patches" => ["chipsynth"],
                    "Credits" => ["credits"],
                    _ => []
                };
                app.Run(args);

                args = [];
            }
            catch(CommandParseException ex)
            {
                RadHelper.WriteError(ex.Message);
                AnsiConsole.WriteLine();
                args = ["-h"];
            }
            catch (TeensyStateException ex)
            {
                RadHelper.WriteError(ex.Message);
                continue;
            }
            catch (TeensyBusyException ex)
            {
                RadHelper.WriteError(ex.Message);
                continue;
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex, ExceptionFormats.Default);
                LogExceptionToFile(ex);
                continue;
            }
            if (resultCode == -1) return resultCode;
        }
    }

    private static void UnpackAssets()
    {

        AssetHelper.UnpackAssets(GameConstants.Game_Image_Local_Path, "OneLoad64.zip");
        AssetHelper.UnpackAssets(MusicConstants.Musician_Image_Local_Path, "Composers.zip");
        AssetHelper.UnpackAssets(AssetConstants.VicePath, "vice-bins.zip");
    }

    private static readonly object _logFileLock = new object();

    private static void LogExceptionToFile(Exception ex)
    {
        string filePath = Path.Combine(Assembly.GetExecutingAssembly().GetPath(), @"Assets\System\Logs\UnhandledErrorLogs.txt");

        if (!Directory.Exists(Path.GetDirectoryName(filePath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        }

        try
        {
            lock (_logFileLock)
            {
                File.AppendAllText(filePath, $"{DateTime.Now}{Environment.NewLine}Exception: {ex}{Environment.NewLine}{Environment.NewLine}");
            }
        }
        catch (Exception logEx)
        {
            Debug.WriteLine("Failed to log exception: " + logEx.Message);
        }
    }
}