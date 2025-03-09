﻿using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using TeensyRom.Ui.Features.Common.Models;
using System;
using TeensyRom.Ui.Helpers;

namespace TeensyRom.Ui.Services
{
    public class DialogService : IDialogService
    {
        public Task<bool> ShowConfirmation(string content)
        {
            return ShowConfirmation("Confirm", content);
        }
        public async Task<bool> ShowConfirmation(string title, string content)
        {
            var viewModel = new ConfirmDialogViewModel(title, content);

            var view = new ContentControl
            {
                Content = viewModel,
                ContentTemplate = (DataTemplate)Application.Current.Resources["ConfirmationDialogTemplate"]
            };

            var result = await MaterialDesignThemes.Wpf.DialogHost.Show(view, UiConstants.RootDialog);

            if (result is string resultString)
            {
                return bool.TryParse(resultString, out bool dialogResult) && dialogResult;
            }
            return false;
        }

        public void ShowNoClose(string title, string content)
        {
            var noCloseDialogViewModel = new NoCloseDialogViewModel(title, content);
            var view = new ContentControl
            {
                Content = noCloseDialogViewModel,
                ContentTemplate = (DataTemplate)Application.Current.Resources["NoCloseDialogTemplate"]
            };
            MaterialDesignThemes.Wpf.DialogHost.Show(view, UiConstants.RootDialog);
        }

        public void HideNoClose()
        {
            MaterialDesignThemes.Wpf.DialogHost.Close(UiConstants.RootDialog);
        }
    }
}
