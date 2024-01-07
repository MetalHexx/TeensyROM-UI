using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Ui.Features.Files.State;

namespace TeensyRom.Ui.Features.Files.Paging
{
    public class PagingViewModel : ReactiveObject
    {
        [ObservableAsProperty] public int CurrentPage { get; }
        [ObservableAsProperty] public int TotalPages { get; }
        [Reactive] public int PageSize { get; set; } = 250;
        public ObservableCollection<int> PageSizes { get; } = [100, 250, 500, 1000, 2000, 5000];

        public ReactiveCommand<Unit, Unit> NextPageCommand { get; set; }
        public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; set; }
        public ReactiveCommand<int, Unit> PageSizeCommand { get; set; }

        public PagingViewModel(IFileState fileState)
        {
            fileState.CurrentPage.ToPropertyEx(this, vm => vm.CurrentPage);
            fileState.TotalPages.ToPropertyEx(this, vm => vm.TotalPages);


            NextPageCommand = ReactiveCommand.CreateFromTask(fileState.NextPage);
            PreviousPageCommand = ReactiveCommand.CreateFromTask(fileState.PreviousPage);
            PageSizeCommand = ReactiveCommand.CreateFromTask<int>(fileState.SetPageSize);

            this.WhenAnyValue(x => x.PageSize)
                .Skip(1)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(async size => await fileState.SetPageSize(size));
        }
    }
}