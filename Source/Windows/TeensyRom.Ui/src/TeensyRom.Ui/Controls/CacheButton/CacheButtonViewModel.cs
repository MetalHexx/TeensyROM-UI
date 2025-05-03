using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Controls.CacheButton
{
    public class CacheButtonViewModel : ReactiveObject
    {   
        public ReactiveCommand<Unit, Unit> CacheAllCommand { get; set; }

        public CacheButtonViewModel(Func<Task> cacheAllFunc, IDialogService dialog, TeensyStorageType storageType, INavigationService nav)
        {
            CacheAllCommand = ReactiveCommand.CreateFromTask(
                execute: async _ =>
                {
                    var confirm = await dialog.ShowConfirmation($"Index {storageType} Storage", $"This will create a local index of all the files stored on your {storageType} to enable search and file launch randomization across your entire collection.\r\rThis may take a few minutes if you have thousands of files. Proceed?");
                    if (!confirm) return;
                    confirm = await dialog.ShowConfirmation($"Index {storageType} Storage", $"You'll now be navigated to the \"Terminal\" to watch as all the files are indexed.");
                    if (!confirm) return;

                    nav.NavigateTo(NavigationLocation.Terminal);
                    dialog.ShowNoClose($"Indexing {storageType} Storage", $"This may take a few minutes.  Don't touch your commodore while indexing is in progress.");
                    await cacheAllFunc();
                    dialog.HideNoClose();
                    confirm = await dialog.ShowConfirmation($"Indexing Completed", $"Indexing has completed for {storageType} storage.");
                },
                outputScheduler: RxApp.MainThreadScheduler);
        }
    }
}
