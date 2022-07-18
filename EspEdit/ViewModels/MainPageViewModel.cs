using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EspEdit.Services;
using Microsoft.Maui;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EspEdit.ViewModels;

[ObservableRecipient]
public partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<RecordGroup> records;

    [ObservableProperty]
    private string selectedRecordText;

    [ObservableProperty]
    private string currentFile;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedRecordText))]
    private Record selectedRecord;
    partial void OnSelectedRecordChanged(Record value) => SelectedRecordText = value.Item.GetRawText();

    public MainPageViewModel()
    {
        records = new ObservableCollection<RecordGroup>();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        try
        {
            // todo json convert
            FilePickerFileType customFileType = new(
            new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, new[] { ".json", ".esp", ".esm" } },
                { DevicePlatform.iOS, new[] { ".json" } }, // or general UTType values
                { DevicePlatform.Android, new[] { ".json" } },
                { DevicePlatform.Tizen, new[] { ".json" } },
                { DevicePlatform.macOS, new[] { ".json" } },
            });

            PickOptions options = new()
            {
                PickerTitle = "Please select an esp",
                FileTypes = customFileType,
            };

            FileResult result = await FilePicker.Default.PickAsync(options);
            if (result != null)
            {
                // todo convert with tes3conv
                CurrentFile = result.FullPath;
            }
        }
        catch (Exception ex)
        {
            // The user canceled or something went wrong
            // todo logging
            return;
        }

        await LoadFileInternalAsync();
    }

    private async Task LoadFileInternalAsync()
    {
        if (string.IsNullOrEmpty(CurrentFile))
        {
            return;
        }

        string json = "";
        string extension = Path.GetExtension(CurrentFile).ToUpper();
        switch (extension)
        {
            case ".JSON":
                json = File.ReadAllText(CurrentFile);
                break;
            case ".ESP":
            case ".ESM":
                // convert to json with tes3conv
                json = await ConvertToJsonAsync(CurrentFile);
                break;
            default:
                break;
        }

        // double check again
        if (string.IsNullOrEmpty(json))
        {
            return;
        }

        JsonElement element;
        try
        {
            //JsonDocument doc = JsonDocument.Parse(json);
            element = JsonSerializer.Deserialize<JsonElement>(json);
        }
        catch (Exception ex)
        {
            // todo logging
            return;
        }

        List<Record> records = new();
        foreach (JsonElement item in element.EnumerateArray())
        {
            string type = item.GetProperty("type").GetString();

            string id = "";
            if (item.TryGetProperty("id", out JsonElement idValue))
            {
                id = idValue.GetString();
            }

            records.Add(new(item, type, id));
        }

        Records.Clear();
        ILookup<string, Record> groups = records.ToLookup(x => x.Type);
        foreach (IGrouping<string, Record> group in groups)
        {
            string name = group.Key;
            IEnumerable<Record> vals = groups[name];

            Records.Add(new(name, vals));

        }
    }

    // todo make async
    [RelayCommand]
    private async Task ReloadAsync()
    {
        await LoadFileInternalAsync();
    }

    [RelayCommand]
    private static void Exit()
    {
        Application.Current.Quit();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        // backup file
        // todo fix numbering
        string backup = $"{CurrentFile}.bak";
        if (File.Exists(backup))
        {
            return;
        }
        File.Copy(CurrentFile, backup);

        // create new file
        string file = "";
        foreach (RecordGroup group in Records)
        {
            foreach (Record item in group)
            {
                // todo try parsing json
                file += item.Item.GetRawText();
            }
        }


        throw new NotImplementedException();
        // save current file
        //await File.WriteAllTextAsync(CurrentFile, file);

        // convert to esp
        // todo

        await ConvertToEspAsync(file);
    }

    private async Task ConvertToEspAsync(string jsonPath)
    {
        //tes3conv "test.esp" "test.json"


        await Task.Delay(1);
    }

    private async Task<string> ConvertToJsonAsync(string currentFile)
    {
        if (!await FileSystem.Current.AppPackageFileExistsAsync("lib/tes3conv.exe"))
        {
            // todo logging
            return null;
        }

        // todo hash the exe
        string tes3convPath = Path.Combine(FileSystem.Current.AppDataDirectory, Constants.Tes3Conv);
        if (!File.Exists(tes3convPath))
        {
            using Stream fileStream = await FileSystem.Current.OpenAppPackageFileAsync("lib/tes3conv.exe");

            // Write the file content to the app data directory
            using FileStream outputStream = System.IO.File.OpenWrite(tes3convPath);
            await fileStream.CopyToAsync(outputStream);
        }

        if (!File.Exists(tes3convPath))
        {
            // todo logging
            return null;
        }

        //tes3conv "test.esp" to stdout
        string arg = $"\"{currentFile}\"";
        ProcessStartInfo si = new(tes3convPath, arg)
        {
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,

        };

        string error = "";
        Process p = new();
        p.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
        {
            error += e.Data;
        });

        p.StartInfo = si;
        p.Start();

        p.BeginErrorReadLine();
        string output = await p.StandardOutput.ReadToEndAsync();
        await p.WaitForExitAsync();

        return output;
    }

    // todo make async
    [RelayCommand]
    private void Delete()
    {
        IEnumerable<Record> flatRecords = Records.SelectMany(x => x);
        IEnumerable<Record> selectedRecords = flatRecords.Where(x => x.IsSelected);

        IEnumerable<Record> newFlatRecords = flatRecords.Except(selectedRecords);
        ObservableCollection<RecordGroup> newRecords = new();

        foreach (IGrouping<string, Record> group in newFlatRecords.ToLookup(x => x.Type))
        {
            IEnumerable<Record> vals = group.ToList();
            newRecords.Add(new(group.Key, vals));
        }
        Records = newRecords;
    }

    [RelayCommand]
    private static void ChangeTheme()
    {
        switch (Application.Current.UserAppTheme)
        {
            case AppTheme.Unspecified:
                Application.Current.UserAppTheme = AppTheme.Light;
                break;
            case AppTheme.Light:
                Application.Current.UserAppTheme = AppTheme.Dark;
                break;
            case AppTheme.Dark:
                Application.Current.UserAppTheme = AppTheme.Light;
                break;
            default:
                break;
        }


    }

    [RelayCommand]
    private void SaveRecord()
    {
        // parse json
        JsonElement element;
        try
        {
            element = JsonSerializer.Deserialize<JsonElement>(SelectedRecordText);
        }
        catch (Exception)
        {
            return;
        }

        SelectedRecord.Item = element;
    }

    [RelayCommand]
    private void RestoreRecord()
    {
        if (SelectedRecord is not null)
        {
            SelectedRecordText = SelectedRecord.Item.GetRawText();
        }
    }
}
