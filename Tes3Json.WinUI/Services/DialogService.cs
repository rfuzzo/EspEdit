using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tes3Json.Services;
using Tes3Json.WinUI.Dialogs;

namespace Tes3Json.WinUI.Services;

internal class DialogService : IDialogService
{
    public async Task DisplayAlert(string title, string message, string cancel)
    {
        ContentDialog noWifiDialog = new()
        {
            Title = title,
            Content = message,
            CloseButtonText = cancel,
            XamlRoot = App.MainRoot.XamlRoot,
        };

        await noWifiDialog.ShowAsync();
    }
}
