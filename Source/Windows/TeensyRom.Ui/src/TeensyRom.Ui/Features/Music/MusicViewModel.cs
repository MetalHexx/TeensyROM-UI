using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Games;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Controls.Explorer;
using TeensyRom.Ui.Controls.PlayToolbar;
using TeensyRom.Ui.Features.Common.Models;
using TeensyRom.Ui.Features.Common.State.Progress;
using TeensyRom.Ui.Features.Files.State;
using TeensyRom.Ui.Features.Games.State;
using TeensyRom.Ui.Features.Global;
using TeensyRom.Ui.Features.Music.Search;
using TeensyRom.Ui.Features.Music.SongList;
using TeensyRom.Ui.Features.Music.State;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Helpers.ViewModel;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Music
{
    public class MusicViewModel(IMusicPlayerContext player, IGameMetadataService metadata, IGlobalState globalState, IDialogService dialog, IAlertService alert, ISettingsService settingsService, IMusicViewConfig config, IProgressTimer timer) : ReactiveObject
    {
        [Reactive] public ExplorerViewModel Explorer { get; set; } = new("Music", player, globalState, dialog, alert, settingsService, metadata, config, timer);
    }
}
