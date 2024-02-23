using MaterialDesignColors;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TeensyRom.Core.Common;
using TeensyRom.Core.Games;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Settings;
using TeensyRom.Ui.Controls.CornerToolbar;
using TeensyRom.Ui.Controls.DirectoryChips;
using TeensyRom.Ui.Controls.DirectoryList;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Controls.Explorer;
using TeensyRom.Ui.Controls.FeatureTitle;
using TeensyRom.Ui.Controls.Paging;
using TeensyRom.Ui.Controls.PlayToolbar;
using TeensyRom.Ui.Controls.Search;
using TeensyRom.Ui.Controls.SearchResultsToolbar;
using TeensyRom.Ui.Features.Common.State;
using TeensyRom.Ui.Features.Files.State;
using TeensyRom.Ui.Features.Games.GameInfo;
using TeensyRom.Ui.Features.Games.State;
using TeensyRom.Ui.Features.Global;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Helpers.ViewModel;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Games
{
    public class GamesViewModel(IGamePlayerContext player, IGameMetadataService metadata, IGlobalState globalState, IDialogService dialog, IAlertService alert, ISettingsService settingsService, IGameViewConfig config) : ReactiveObject
    {
        [Reactive] public ExplorerViewModel Explorer { get; set; } = new("Games", player, globalState, dialog, alert, settingsService, metadata, config);
    }
}