using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Features.Music.State;

namespace TeensyRom.Ui.Features.Music.Search
{
    public class SearchMusicViewModel : ReactiveObject, IDisposable
    {
        [Reactive] public string SearchText { get; set; } = string.Empty;
        [Reactive] public bool ShowClearSearch { get; set; } = false;
        public ReactiveCommand<Unit, Unit> ClearSearchCommand { get; private set; }
        private IDisposable? _searchSubscription;
        private IDisposable? _clearSearchSubscription;

        public SearchMusicViewModel(IMusicState musicState)
        {
            ClearSearchCommand = ReactiveCommand.CreateFromTask(() => 
            {
                SearchText = string.Empty;
                return musicState.ClearSearch();
            });

            _searchSubscription = this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Where(searchText => !string.IsNullOrWhiteSpace(searchText) && searchText.Length > 3)
                .Subscribe(searchText => 
                {
                    ShowClearSearch = true;
                    musicState.SearchMusic(searchText);
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
