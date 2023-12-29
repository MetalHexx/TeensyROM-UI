using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Logging;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Common;
using INavigationService = TeensyRom.Ui.Features.NavigationHost.INavigationService;
using DynamicData.Binding;
using System.Threading.Tasks;
using System.Reactive.Subjects;
using System.Windows.Threading;
using System.Collections.Generic;
using TeensyRom.Ui.Helpers.Messages;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Settings;
using TeensyRom.Ui.Helpers.ViewModel;
using TeensyRom.Ui.Features.Common.Models;
using MediatR;

namespace TeensyRom.Ui.Features.FileTransfer
{

    public class FileTransferViewModel : FeatureViewModelBase
    {
        public ObservableCollection<StorageItem> SourceItems { get; set; } = new();
        public ObservableCollection<StorageItem> TargetItems { get; set; } = new();
        public ObservableCollection<int> PageSizes { get; } = new ObservableCollection<int> { 100, 250, 500, 1000, 2000, 5000 };

        [ObservableAsProperty] public bool IsTargetItemsEmpty { get; }
        [ObservableAsProperty] public bool CanExecuteTargetLoadCommand { get; } = true;
        [ObservableAsProperty] public bool IsLoadingFiles { get; } = false;
        [Reactive] public int CurrentPageNumber { get; set; } = 1;

        //private int _totalPages;
        //public int TotalPages 
        //{ 
        //    get => _totalPages; 
        //    set => this.RaiseAndSetIfChanged(ref _totalPages, value); 
        //}
        [Reactive] public int TotalPages { get; set; }
        [Reactive] public string? CurrentPath { get; set; }
        [Reactive] public string Logs { get; set; } = string.Empty;
        [Reactive] public int PageSize { get; set; } = 250;

        public ReactiveCommand<Unit, Unit> LoadParentDirectoryContentCommand { get; set; }
        public ReactiveCommand<Unit, Unit> TestDirectoryListCommand { get; set; }
        public ReactiveCommand<DirectoryItem, Unit> LoadDirectoryContentCommand { get; set; }
        public ReactiveCommand<int, Unit> ChangePageSizeCommand { get; set; }
        public ReactiveCommand<Unit, Unit> LoadNextPageCommand { get; set; }
        public ReactiveCommand<Unit, Unit> LoadPrevPageCommand { get; set; }
        public ReactiveCommand<FileItem, Unit> LaunchFileCommand { get; set; }

        private bool _isMoreItemsLoading = false;

        private BehaviorSubject<bool> _isLoadingFiles = new BehaviorSubject<bool>(false);
        private readonly IMediator _mediator;
        private readonly ISerialPortState _serialPortState;
        private readonly ILoggingService _logService;
        private readonly ISnackbarService _snackbar;
        private readonly Dispatcher _dispatcher;
        private readonly StringBuilder _logBuilder = new StringBuilder();
        private readonly List<StorageItem> _currentItems = new();
        
        private const int _take = 5000;
        public FileTransferViewModel(IMediator mediator, ISettingsService settingsService, ISerialPortState serialPortState, INavigationService nav, ILoggingService logService, ISnackbarService snackbar, Dispatcher dispatcher) 
        {
            FeatureTitle = "File Transfer";
            _mediator = mediator;
            _serialPortState = serialPortState;
            _logService = logService;
            _snackbar = snackbar;
            _dispatcher = dispatcher;
            SourceItems = new ObservableCollection<StorageItem> { FileTreeTestData.InitializeTestStorageItems() };

            TestDirectoryListCommand = ReactiveCommand.CreateFromTask(_ => TestDirectoryListAsync(), outputScheduler: ImmediateScheduler.Instance);
            LoadParentDirectoryContentCommand = ReactiveCommand.CreateFromTask(LoadParentDirectory, outputScheduler: RxApp.MainThreadScheduler);
            LoadDirectoryContentCommand = ReactiveCommand.CreateFromTask<DirectoryItem>(async directory => await LoadNewDirectoryAsync(directory), outputScheduler: RxApp.MainThreadScheduler);

            LoadNextPageCommand = ReactiveCommand.CreateFromTask(LoadNextItems, outputScheduler: RxApp.MainThreadScheduler);
            LoadPrevPageCommand = ReactiveCommand.CreateFromTask(LoadPrevItems, outputScheduler: RxApp.MainThreadScheduler);
            LaunchFileCommand = ReactiveCommand.CreateFromTask<FileItem>(file => _mediator.Send(new LaunchFileCommand(file.Path)), outputScheduler: ImmediateScheduler.Instance);

            _logService.Logs.Subscribe(log =>
            {
                _logBuilder.AppendLine(log);
                Logs = _logBuilder.ToString();
            });

            _serialPortState.IsConnected
                .Where(isConnected => isConnected is true)
                .CombineLatest(settingsService.Settings, (isConnected, settings) => settings)
                .Do(settings => 
                {
                    CurrentPath = settings.TargetRootPath;
                })
                .CombineLatest(nav.SelectedNavigationView, (settings, currentNav) => (settings, currentNav))
                .Where(sn => sn.currentNav?.Type == NavigationLocation.FileTransfer)
                .Where(_ => TargetItems.Count == 0)
                .Subscribe(_ => LoadCurrentDirectory());

            TargetItems.ObserveCollectionChanges()
                .Select(targetCol => TargetItems.Count == 0)
                .ToPropertyEx(this, x => x.IsTargetItemsEmpty);

            _isLoadingFiles.ToPropertyEx(this, x => x.IsLoadingFiles);

            Observable.Merge(
                    LoadParentDirectoryContentCommand.IsExecuting,
                    TestDirectoryListCommand.IsExecuting,
                    LoadDirectoryContentCommand.IsExecuting)
                .Select(x => !x)
                .Throttle(TimeSpan.FromMilliseconds(100))
                .ToPropertyEx(this, x => x.CanExecuteTargetLoadCommand);

            this.WhenAnyValue(x => x.PageSize)
                .Skip(1)
                .Where(_ => CurrentPath != null) 
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(async _ => await HandlePageSizeChanged());

        }

        private async Task LoadNewDirectoryAsync(DirectoryItem directoryVm)
        {            
            await LoadAll(directoryVm.Path);
        }

        private async Task LoadCurrentDirectory()
        {
            if (string.IsNullOrWhiteSpace(CurrentPath)) return;
            await LoadAll(CurrentPath);
        }

        private async Task LoadParentDirectory()
        {            
            if (CurrentPath is null) return;

            await LoadAll(CurrentPath.GetParentDirectory());
        }

        private async Task HandlePageSizeChanged()
        {            
            await LoadBatch(CurrentPageNumber);
            CurrentPageNumber = 1;
        }

        private async Task LoadNextItems()
        {
            var isLastPage = (CurrentPageNumber) * PageSize >= _currentItems.Count;
            if (_isMoreItemsLoading || isLastPage) return;

            _isMoreItemsLoading = true;
            CurrentPageNumber++;
            await LoadBatch(CurrentPageNumber);
            _isMoreItemsLoading = false;
        }

        private async Task LoadPrevItems()
        {
            if (_isMoreItemsLoading || CurrentPageNumber == 1) return;
            

            _isMoreItemsLoading = true;
            CurrentPageNumber--;
            await LoadBatch(CurrentPageNumber);            
            _isMoreItemsLoading = false;
        }

        private Task LoadBatch(int batchIndex)
        {   
            return Task.Run(() =>
            {
                var skip = (batchIndex - 1) * PageSize;
                var batchItems = _currentItems.Skip(skip).Take(PageSize).ToList();

                _dispatcher.InvokeAsync(() =>
                {
                    TargetItems.Clear();
                    TargetItems.AddRange(batchItems);
                    TotalPages = (int)Math.Ceiling((double)_currentItems.Count / PageSize);
                    MessageBus.Current.SendMessage(new ScrollToTopMessage());
                }, DispatcherPriority.Background);
            });
        }

        private Task LoadAll(string path)
        {
            return Task.Run(async () =>
            {
                CurrentPageNumber = 1;
                _isLoadingFiles.OnNext(true);
                var directoryContent = await LoadDirectoryContent(path, 0, 5000);

                CurrentPath = directoryContent?.Path;

                if (directoryContent is null) return;

                var batchItems = new List<StorageItem>();

                var directories = directoryContent.Directories
                    .Select(d => new DirectoryItem { Name = d.Name, Path = d.Path })
                    .OrderBy(d => d.Name)
                    .ToList();

                var files = directoryContent.Files
                    .Select(d => new FileItem { Name = d.Name, Path = d.Path })
                    .OrderBy(d => d.Name);

                _currentItems.Clear();
                _currentItems.AddRange(directories);
                _currentItems.AddRange(files);                

                LoadBatch(CurrentPageNumber);

                _isLoadingFiles.OnNext(false);
            });
        }

        private async Task<DirectoryContent?> LoadDirectoryContent(string path, uint skip, uint take)
        {
            GetDirectoryResponse? response = null;

            try
            {
                response = await _mediator.Send(new GetDirectoryRequest(path, skip, take));
            }
            catch (TeensyException ex)
            {
                _snackbar.Enqueue(ex.Message);
                return null;
            }
            if (response.DirectoryContent is null)
            {                
                _snackbar.Enqueue("Error receiving directory contents");
            }
            return response.DirectoryContent;
        }

        private async Task<Unit> TestDirectoryListAsync()
        {
            await _mediator.Send(new GetDirectoryRequest("/", 0, 20));
            return Unit.Default;
        }
    }
}