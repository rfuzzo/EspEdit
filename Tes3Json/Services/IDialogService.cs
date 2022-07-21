using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tes3Json.Services
{
    public interface IDialogService
    {
        Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons);
        Task<bool> DisplayAlert(string title, string message, string accept, string cancel);
        Task DisplayAlert(string title, string message, string cancel);
    }


}
