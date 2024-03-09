using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace TeensyRom.Ui.Controls.Search
{
    public class SearchViewModel : ReactiveObject, IDisposable
    {
        [Reactive] public string SearchText { get; set; } = string.Empty;
        [ObservableAsProperty] public bool ShowClearSearch { get; }
        public ReactiveCommand<Unit, Unit>? ClearSearchCommand { get; set; }
        public ReactiveCommand<string, Unit>? SearchCommand { get; set; }
        private IDisposable? _searchSubscription;

        public SearchViewModel(IObservable<bool> searchEnabled)
        {
            searchEnabled
                .Do(enabled => SearchText = enabled ? SearchText : string.Empty)
                .ToPropertyEx(this, x => x.ShowClearSearch);

            _searchSubscription = this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Where(searchText => !string.IsNullOrWhiteSpace(searchText) && searchText.Length > 2)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(searchText => SearchCommand!.Execute(searchText).Subscribe());
        }

        public void Dispose()
        {
            _searchSubscription?.Dispose();
        }
    }
}