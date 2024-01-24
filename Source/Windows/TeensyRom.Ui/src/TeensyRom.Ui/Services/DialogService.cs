using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using TeensyRom.Ui.Features.Common.Models;

namespace TeensyRom.Ui.Services
{
    public class DialogService : IDialogService
    {
        public async Task<bool> ShowConfirmation(string message)
        {
            var viewModel = new ConfirmDialogViewModel(message);

            var view = new ContentControl
            {
                Content = viewModel,
                ContentTemplate = (DataTemplate)Application.Current.Resources["ConfirmationDialogTemplate"]
            };

            var result = await MaterialDesignThemes.Wpf.DialogHost.Show(view, "RootDialog");

            if (result is string resultString)
            {
                return bool.TryParse(resultString, out bool dialogResult) && dialogResult;
            }
            return false;
        }
    }
}
