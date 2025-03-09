using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Helpers;

namespace TeensyRom.Ui.Controls.Playlist
{
    public interface IPlaylistDialogService 
    {
        Task<bool> ShowDialog(ILaunchableItem item);
    }
    internal class PlaylistDialogService(PlaylistViewModel vm) : IPlaylistDialogService
    {
        public async Task<bool> ShowDialog(ILaunchableItem item)
        {
            await vm.InitializeAsync(item);

            var view = new ContentControl 
            {
                Content = vm,
                ContentTemplate = (DataTemplate)Application.Current.Resources["PlayListDialogTemplate"]
            };
            var result = await MaterialDesignThemes.Wpf.DialogHost.Show(view, UiConstants.RootDialog);

            if (result is string resultString)
            {
                return bool.TryParse(resultString, out bool dialogResult) && dialogResult;
            }
            return false;
        }
    }
}
