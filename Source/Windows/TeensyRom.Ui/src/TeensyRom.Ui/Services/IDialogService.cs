using System.Threading.Tasks;

namespace TeensyRom.Ui.Services
{
    public interface IDialogService
    {
        Task<bool> ShowConfirmation(string message);
    }
}