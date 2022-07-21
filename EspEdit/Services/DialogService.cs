using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tes3Json.Services;

namespace EspEdit.Services
{
    internal class DialogService : IDialogService
    {
        public async Task DisplayAlert(string title, string message, string cancel) => await Application.Current.MainPage.DisplayAlert(title, message, cancel);
    }
}
