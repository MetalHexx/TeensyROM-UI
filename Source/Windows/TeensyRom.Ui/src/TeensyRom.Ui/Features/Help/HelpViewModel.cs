﻿using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Diagnostics;
using System.Security.Policy;
using TeensyRom.Ui.Controls.FeatureTitle;
using TeensyRom.Ui.Helpers.ViewModel;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Help
{
    public class HelpViewModel
    {
        private readonly ISetupService _setupService;

        public ReactiveCommand<Unit, Unit> NavigateToUiGithubCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToTrGithubCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToDiscordCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToEbayCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToTindieCommand { get; }        
        public ReactiveCommand<Unit, Unit> NavigateToOneLoad64Command { get; }
        public ReactiveCommand<Unit, Unit> NavigateToHvscCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToSidListCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToDeepSidCommand { get; }
        public ReactiveCommand<Unit, Unit> StartTutorialCommand { get; }
        public ReactiveCommand<Unit, Unit> StartDemoCommand { get; }


        public HelpViewModel(ISetupService setupService) 
        {
            _setupService = setupService;

            NavigateToUiGithubCommand = ReactiveCommand.Create(() => 
            {
                OpenBrowser("https://github.com/MetalHexx/TeensyROM-UI");
            });
            NavigateToTrGithubCommand = ReactiveCommand.Create(() => 
            {
                OpenBrowser("https://github.com/SensoriumEmbedded/TeensyROM");
            });
            NavigateToDiscordCommand = ReactiveCommand.Create(() =>
            {                
                OpenBrowser("https://discord.gg/ubSAb74S5U");
            });
            NavigateToEbayCommand = ReactiveCommand.Create(() => 
            {                
                OpenBrowser("https://www.ebay.com/usr/travster1");
            });
            NavigateToTindieCommand = ReactiveCommand.Create(() =>
            {
                OpenBrowser("https://www.tindie.com/products/travissmith/teensyrom-cartridge-for-c64128/");                
            });
            NavigateToOneLoad64Command = ReactiveCommand.Create(() =>
            {
                OpenBrowser("https://www.youtube.com/playlist?list=PLmN5cgEuNrpiCj1LfKBDUZS06ZBCjif5b");                
            });
            NavigateToHvscCommand = ReactiveCommand.Create(() =>
            {                
                OpenBrowser("https://www.hvsc.c64.org/");
            });
            NavigateToSidListCommand = ReactiveCommand.Create(() =>
            {                
                OpenBrowser("https://www.transbyte.org/SID/SIDlist.html");
            });
            NavigateToDeepSidCommand = ReactiveCommand.Create(() =>
            {
                OpenBrowser("https://github.com/Chordian/deepsid");
            });
            StartTutorialCommand = ReactiveCommand.Create(() =>
            {
                _setupService.ResetSetup();
                _setupService.StartSetup();
            });
            StartDemoCommand = ReactiveCommand.Create(() =>
            {
                OpenBrowser("https://www.youtube.com/playlist?list=PLEfbVPgE4gQKmuOmCOBz_qRZOzlppLdyz");
            });

        }

        private void OpenBrowser(string url)
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
    }
}
