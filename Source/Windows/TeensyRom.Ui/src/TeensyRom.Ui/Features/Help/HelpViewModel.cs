using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Diagnostics;
using System.Security.Policy;
using TeensyRom.Ui.Controls.FeatureTitle;
using TeensyRom.Ui.Helpers.ViewModel;

namespace TeensyRom.Ui.Features.Help
{
    public class HelpViewModel
    {
        public ReactiveCommand<Unit, Unit> NavigateToUiGithubCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToTrGithubCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToDiscordCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToEbayCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToTindieCommand { get; }        
        public ReactiveCommand<Unit, Unit> NavigateToOneLoad64Command { get; }
        public ReactiveCommand<Unit, Unit> NavigateToHvscCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToSidListCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToDeepSidCommand { get; }


        public HelpViewModel() 
        {
            NavigateToUiGithubCommand = ReactiveCommand.Create(() => 
            {
                Process.Start(new ProcessStartInfo("https://github.com/MetalHexx/TeensyROM-UI") { UseShellExecute = true });
            });
            NavigateToTrGithubCommand = ReactiveCommand.Create(() => 
            {
                Process.Start(new ProcessStartInfo("https://github.com/SensoriumEmbedded/TeensyROM") { UseShellExecute = true });
            });
            NavigateToDiscordCommand = ReactiveCommand.Create(() =>
            {
                Process.Start(new ProcessStartInfo("https://discord.gg/ubSAb74S5U") { UseShellExecute = true });
            });
            NavigateToEbayCommand = ReactiveCommand.Create(() => 
            {
                Process.Start(new ProcessStartInfo("https://www.ebay.com/usr/travster1") { UseShellExecute = true });
            });
            NavigateToTindieCommand = ReactiveCommand.Create(() =>
            {
                Process.Start(new ProcessStartInfo("https://www.tindie.com/products/travissmith/teensyrom-cartridge-for-c64128/") { UseShellExecute = true });
            });
            NavigateToOneLoad64Command = ReactiveCommand.Create(() =>
            {
                Process.Start(new ProcessStartInfo("https://www.youtube.com/watch?v=lz0CJbkplj0&list=PLmN5cgEuNrpiCj1LfKBDUZS06ZBCjif5b") { UseShellExecute = true });
            });
            NavigateToHvscCommand = ReactiveCommand.Create(() =>
            {
                Process.Start(new ProcessStartInfo("https://www.hvsc.c64.org/") { UseShellExecute = true });
            });
            NavigateToSidListCommand = ReactiveCommand.Create(() =>
            {
                Process.Start(new ProcessStartInfo("https://www.transbyte.org/SID/SIDlist.html") { UseShellExecute = true });
            });
            NavigateToDeepSidCommand = ReactiveCommand.Create(() =>
            {
                Process.Start(new ProcessStartInfo("https://github.com/Chordian/deepsid") { UseShellExecute = true });
            });
        }
    }
}
