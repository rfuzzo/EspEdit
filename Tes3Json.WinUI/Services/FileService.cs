using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Tes3Json.Services;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT;
using WinRT.Interop;
using static Tes3Json.WinUI.Services.WinUIConversionUtil;

namespace Tes3Json.WinUI.Services;

public static class WinUIConversionUtil
{
    [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto, PreserveSig = true, SetLastError = false)]
    public static extern IntPtr GetActiveWindow();
}


internal class FileService : IFileService
{
    public async Task<string> OpenFileAsync(string title, string[] filetypes)
    {
        //FileOpenPicker picker = new() { CommitButtonText = "Valider", SuggestedStartLocation = PickerLocationId.ComputerFolder, FileTypeFilter = { ".jpg", ".jpeg" } };
        //WinUIConversionUtil.InitFileOpenPicker(picker);
        //StorageFile fichier = await picker.PickSingleFileAsync();

        Windows.Storage.Pickers.FileOpenPicker picker = new()
        {
            ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
            SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary
        };
        foreach (string item in filetypes)
        {
            picker.FileTypeFilter.Add(item);
        }

        // Need to get the hwnd (“window” is a pointer to a WinUI Window object). 
        // WinRT.Interop namespace is provided by C#/WinRT projected interop wrappers for .NET 5+
        // Need to initialize the picker object with the hwnd / IInitializeWithWindow 
        IntPtr hwnd = Window.Current == null ? GetActiveWindow() : WinRT.Interop.WindowNative.GetWindowHandle(Window.Current);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        // Now we can use the picker object as normal
        Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();

        if (file != null)
        {
            // Application now has read/write access to the picked file
            return file.Path;
        }
        else
        {
            return null;
        }
    }



}
