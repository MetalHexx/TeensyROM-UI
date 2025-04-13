using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TeensyRom.Core.Music.Midi;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Ui.Controls.DirectoryList
{
    public partial class DirectoryListView : UserControl
    {
        private List<IDisposable> _subscriptions = [];
        private DirectoryListViewModel _vm;

        public DirectoryListView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Unloaded += (s, e) => Dispose();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is DirectoryListViewModel vm)
            {
                _vm = vm;

                _subscriptions.Add
                (
                    _vm.MidiEvents
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Where(e => e.DJEventType is DJEventType.NavigateDirectory)                        
                        .Select(e => 
                        {
                            var ccMapping = e.Mapping as CCMapping;

                            if (ccMapping == null) return 0;

                            return ccMapping.CCType switch
                            {
                                CCType.Absolute => e.GetAbsoluteValueDelta(-1, 1, e.Value),
                                CCType.Relative1 => e.GetRelativeValue_TwosComplement(e.Value),
                                CCType.Relative2 => e.GetRelativeValue_BinaryOffset(),
                                _ => 0
                            };
                        })
                        .Select(doubleAmt => doubleAmt > 0 ? 1 : -1)
                        .Subscribe(direction => 
                        {
                            
                            if (!DirectoryList.IsFocused) 
                            {
                                DirectoryList.Focus();
                            }
                            if (DirectoryList.Items.Count == 0) return;

                            if (DirectoryList.SelectedIndex == -1)
                            {
                                DirectoryList.SelectedIndex = 0;
                            }
                            else
                            {
                                int newIndex = DirectoryList.SelectedIndex + direction;

                                if (newIndex < 0)
                                {
                                    newIndex = DirectoryList.Items.Count - 1;
                                }
                                else if (newIndex >= DirectoryList.Items.Count)
                                {
                                    newIndex = 0;
                                }
                                DirectoryList.SelectedIndex = newIndex;
                            }

                            DirectoryList.ScrollIntoView(DirectoryList.SelectedItem);

                        })
                );

                _subscriptions.Add
                (
                    _vm.MidiEvents
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Where(e => e.DJEventType is DJEventType.LaunchSelectedFile)
                        .Subscribe(direction =>
                        {
                            if (DirectoryList.SelectedItem is DirectoryItem directoryItem)
                            {
                                _vm.LoadDirectoryCommand.Execute(directoryItem).Subscribe();
                                return;
                            }
                            if (DirectoryList.SelectedItem is ILaunchableItem launchable)                             
                            {
                                _vm.PlayCommand.Execute(launchable).Subscribe();
                            }
                        })
                );
            }
        }

        private void OnListViewDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            ListView? listView = sender as ListView;

            if (TryOpenDirectory(listView)) return;

            TryLaunch(listView);
        }

        private void OnListViewClicked(object sender, MouseButtonEventArgs e)
        {
            TrySelect(sender as ListView);
        }

        private bool TrySelect(ListView? listView)
        {
            if (listView?.SelectedItem is ILaunchableItem launchItem)
            {
                var viewModel = (DirectoryListViewModel)DataContext;
                viewModel.SelectCommand.Execute(launchItem).Subscribe();
                return true;
            }
            return false;
        }

        private bool TryLaunch(ListView? listView)
        {
            if (listView?.SelectedItem is ILaunchableItem launchable)
            {
                var viewModel = (DirectoryListViewModel)DataContext;
                viewModel.PlayCommand.Execute(launchable).Subscribe();
                return true;
            }
            return false;
        }

        private bool TryOpenDirectory(ListView? listView)
        {
            if (listView?.SelectedItem is DirectoryItem directoryItem)
            {
                var viewModel = (DirectoryListViewModel)DataContext;
                viewModel.LoadDirectoryCommand.Execute(directoryItem).Subscribe();
                return true;
            }
            return false;
        }

        private void OnListViewPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                {
                    RoutedEvent = MouseWheelEvent,
                    Source = sender
                };

                var parent = ((Control)sender).Parent as UIElement;
                parent?.RaiseEvent(eventArg);
            }
        }

        private void DirectoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = sender as ListView;
            if (listView != null && listView.SelectedItem != null)
            {
                listView.ScrollIntoView(listView.SelectedItem);
            }
        }

        public void Dispose()
        {
            _subscriptions.ForEach(s => s?.Dispose());
        }
    }
}
