using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using TeensyRom.Ui.Features.Games.State;

namespace TeensyRom.Ui.Features.Games.Search
{
    public class SearchGamesViewModel : ReactiveObject, IDisposable
    {
        [Reactive] public string SearchText { get; set; } = string.Empty;
        [Reactive] public bool ShowClearSearch { get; set; } = false;
        public ReactiveCommand<Unit, Unit> ClearSearchCommand { get; private set; }
        private IDisposable? _searchSubscription;
        private IDisposable? _clearSearchSubscription;

        public SearchGamesViewModel(IGameState gameState)
        {
            ClearSearchCommand = ReactiveCommand.CreateFromTask(() =>
            {
                SearchText = string.Empty;
                return gameState.ClearSearch();
            });

            _searchSubscription = this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Where(searchText => !string.IsNullOrWhiteSpace(searchText) && searchText.Length > 2)
                .Subscribe(searchText =>
                {
                    ShowClearSearch = true;
                    gameState.SearchGames(searchText);
                });

            _clearSearchSubscription = this.WhenAnyValue(x => x.SearchText)
                .Where(string.IsNullOrWhiteSpace)
                .Subscribe(_ => ShowClearSearch = false);
        }

        public void Dispose()
        {
            _searchSubscription?.Dispose();
            _clearSearchSubscription?.Dispose();
        }
    }
}