using ReactiveUI;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using TeensyRom.Core.Settings;

namespace TeensyRom.Ui.Controls.LibraryFilter
{
    /// <summary>
    /// Interaction logic for LibraryFilterView.xaml
    /// </summary>
    public partial class LibraryFilterView : UserControl
    {
        public LibraryFilterView()
        {
            InitializeComponent();

            MessageBus.Current.Listen<RandomLaunchMessage>().Subscribe(_ => SpinRandomButton());
        }

        private void SpinRandomButton() 
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var storyboard = this.Resources["DiceSpinStoryboard"] as Storyboard;
                if (storyboard != null)
                {
                    Storyboard clonedStoryboard = storyboard.Clone();
                    clonedStoryboard.Begin(this.diceButton, true);
                }
            });
        }

        private void RadioButton_Loaded(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;

            if (radioButton == null) return;

            var parentDataContext = this.DataContext;
            var viewModel = parentDataContext as LibraryFilterViewModel;

            if (viewModel == null) return;

            if (viewModel.SelectedLibrary is TeensyFilter selected &&
                radioButton.DataContext is TeensyFilter current &&
                selected.Equals(current))
            {
                radioButton.IsChecked = true;
                return;
            }

            if (viewModel.SelectedLibrary is null &&
                radioButton.DataContext is TeensyFilter first &&
                first.Equals(viewModel.Libraries.FirstOrDefault()))
            {
                radioButton.IsChecked = true;
            }
        }
    }
}
