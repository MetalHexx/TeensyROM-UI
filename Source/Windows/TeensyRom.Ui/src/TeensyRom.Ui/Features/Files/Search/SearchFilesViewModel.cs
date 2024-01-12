using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using TeensyRom.Ui.Features.Music.State;
using TeensyRom.Ui.Features.Files.State;

namespace TeensyRom.Ui.Features.Files.Search
{
    public class SearchFilesViewModel : ReactiveObject, IDisposable
    {
        [Reactive] public string SearchText { get; set; } = string.Empty;
        [Reactive] public bool ShowClearSearch { get; set; } = false;
        public ReactiveCommand<Unit, Unit> ClearSearchCommand { get; private set; }
        private IDisposable? _searchSubscription;
        private IDisposable? _clearSearchSubscription;

        public SearchFilesViewModel(IFileState fileState)
        {
            ClearSearchCommand = ReactiveCommand.CreateFromTask(() =>
            {
                SearchText = string.Empty;
                return fileState.ClearSearch();
            });

            _searchSubscription = this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Where(searchText => !string.IsNullOrWhiteSpace(searchText) && searchText.Length > 3)
                .Subscribe(searchText =>
                {
                    ShowClearSearch = true;
                    fileState.SearchFiles(searchText);
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
