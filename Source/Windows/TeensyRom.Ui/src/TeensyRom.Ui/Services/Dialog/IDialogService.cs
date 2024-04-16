using System;
using System.Threading.Tasks;

namespace TeensyRom.Ui.Services
{
    public interface IDialogService
    {
        Task<bool> ShowConfirmation(string message);
        Task<bool> ShowConfirmation(string title, string content);
        void HideNoClose();
        void ShowNoClose(string title, string content);
    }
}