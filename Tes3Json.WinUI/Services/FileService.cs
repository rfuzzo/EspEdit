using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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

        Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();

        return file?.Path;
    }

    public async Task<string> SaveFileAsync(string title, string[] filetypes, string currentFile)
    {
        FileSavePicker savePicker = new()
        {
            SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
        };

        savePicker.FileTypeChoices.Add("tes3 plugin", filetypes);
        savePicker.SuggestedFileName = currentFile;

        IntPtr hwnd = Window.Current == null ? GetActiveWindow() : WinRT.Interop.WindowNative.GetWindowHandle(Window.Current);
        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

        Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();

        return file?.Path;
    }

}
