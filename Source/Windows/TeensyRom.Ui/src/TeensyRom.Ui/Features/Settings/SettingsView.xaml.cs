using Microsoft.VisualBasic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TeensyRom.Ui.Features.Settings
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        private bool _loaded = false;
        private bool _previousMidiEnabledValue;
        public SettingsView()
        {
            InitializeComponent();
            Loaded += SettingsView_Loaded;
        }

        private void SettingsView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _loaded = true;

            if (DataContext is SettingsViewModel viewModel) 
            {
                _previousMidiEnabledValue = viewModel.LastCart.MidiSettings.MidiEnabled;
            }
        }

        private void MidiEnabledCheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            SaveMidiEnabled(sender);
        }

        private void MidiEnabledCheckBox_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            SaveMidiEnabled(sender);
        }

        private void SaveMidiEnabled(object sender)
        {
            if (_loaded == false) return;

            var checkbox = sender as CheckBox;
            var viewModel = DataContext as SettingsViewModel;

            var shouldSave = checkbox is not null
                && checkbox.IsChecked.HasValue
                && viewModel is not null
                && viewModel.LastCart is not null
                && viewModel.LastCart.MidiSettings.MidiEnabled != _previousMidiEnabledValue;

            if (shouldSave)
            {
                var previousValue = viewModel!.LastCart!.MidiSettings.MidiEnabled;
                _previousMidiEnabledValue = viewModel.LastCart.MidiSettings.MidiEnabled;

                Task.Run(() =>
                {
                    viewModel.HandleSave();
                });
            }
        }
    }
}
