using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tes3Json.Services;

namespace EspEdit.Services
{
    internal class FileService : IFileService
    {
        public async Task<string> OpenFileAsync(string title, string[] filetypes)
        {
            try
            {
                FilePickerFileType customFileType = new(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                { DevicePlatform.WinUI, filetypes },
                { DevicePlatform.iOS, filetypes }, // or general UTType values
                { DevicePlatform.Android, filetypes },
                { DevicePlatform.Tizen, filetypes },
                { DevicePlatform.macOS, filetypes },
                });

                PickOptions options = new()
                {
                    PickerTitle = title,
                    FileTypes = customFileType,
                };

                FileResult result = await FilePicker.Default.PickAsync(options);
                if (result != null)
                {
                    return result.FullPath;
                }
            }
            catch (Exception)
            {
                // The user canceled or something went wrong
                // todo logging
                return null;
            }

            return null;
        }



    }
}
