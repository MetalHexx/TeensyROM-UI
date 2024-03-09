using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Ui.Controls.Paging
{
    public class PagingViewModel : ReactiveObject
    {
        [Reactive] public int PageSize { get; set; } = 100;
        [ObservableAsProperty] public int CurrentPage { get; }
        [ObservableAsProperty] public int TotalPages { get; }
        [ObservableAsProperty] public bool ShowNext { get; }
        [ObservableAsProperty] public bool ShowPrevious { get; }
        public ObservableCollection<int> PageSizes { get; } = [100, 250, 500, 1000, 2000, 5000];

        public ReactiveCommand<Unit, Unit>? NextPageCommand { get; init; }
        public ReactiveCommand<Unit, Unit>? PreviousPageCommand { get; init; }
        public ReactiveCommand<int, Unit>? PageSizeCommand { get; init; }

        public PagingViewModel(IObservable<int> currentPage, IObservable<int> totalPages)
        {
            currentPage.ToPropertyEx(this, vm => vm.CurrentPage);
            totalPages.ToPropertyEx(this, vm => vm.TotalPages);

            currentPage.CombineLatest(totalPages, (current, total) => current < total)
                .ToPropertyEx(this, vm => vm.ShowNext);

            currentPage
                .Select(current => current > 1)
                .ToPropertyEx(this, vm => vm.ShowPrevious);

            this.WhenAnyValue(x => x.PageSize)
                .Skip(1)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(async size => await PageSizeCommand!.Execute(size));
        }
    }
}