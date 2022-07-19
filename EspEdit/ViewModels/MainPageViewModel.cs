using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EspEdit.Extensions;
using EspEdit.Services;
using Microsoft.Maui;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Compression;
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
    private readonly IDialogService dialogService;
    private readonly ITes3ConvService tes3ConvService;

    [ObservableProperty]
    private ObservableCollection<RecordGroup> records;

    [ObservableProperty]
    private string selectedRecordText;

    [ObservableProperty]
    private string currentFile;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedRecordText))]
    private Record selectedRecord;
    partial void OnSelectedRecordChanged(Record value)
    {
        if (value?.Item is not null)
        {
            var text = value.Item.GetRawText();
            SelectedRecordText = text;
        }
    }

    public MainPageViewModel()
    {
        records = new ObservableCollection<RecordGroup>();

        // todo do this properly with constructor injection
        dialogService = App.Current.Services.GetService<IDialogService>();
        tes3ConvService = App.Current.Services.GetService<ITes3ConvService>();
    }

    private bool IsJson()
    {
        return Path.GetExtension(CurrentFile).ToUpper() == ".JSON";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        try
        {
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

        string json = IsJson() ? File.ReadAllText(CurrentFile) : await tes3ConvService.GetJsonFromEsp(new FileInfo(CurrentFile));

        // double check again
        if (string.IsNullOrEmpty(json))
        {
            return;
        }

        JsonElement document;
        try
        {
            //JsonDocument doc = JsonDocument.Parse(json);
            document = JsonSerializer.Deserialize<JsonElement>(json);
        }
        catch (Exception ex)
        {
            // todo logging
            return;
        }

        List<Record> records = new();
        foreach (JsonElement item in document.EnumerateArray())
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
    private async Task SaveAsAsync()
    {
        await Task.Delay(1);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsJson())
        {
            // nothing to back up
        }
        else
        {
            // backup the esp file
            string backup = $"{CurrentFile}.bak";
            if (!File.Exists(backup))
            {
                File.Copy(CurrentFile, backup);
            }
            else
            {
                // if a backup file already exists then overwrite
                // todo better?
            }
        }

        // create new file
        // todo move this to individual save? to speed up save times?
        string file = "[\n";
        foreach (RecordGroup group in Records)
        {
            foreach (Record item in group)
            {
                string text = item.Item.GetRawText();
                file += $"{text},\n";
            }
        }
        file = file.TrimEnd('\n').TrimEnd(',');
        file += "\n]";

        string espPath = IsJson() ? Path.ChangeExtension(CurrentFile, ".esp") : CurrentFile;
        if (await tes3ConvService.ConvertJsonToEspAsync(file, espPath))
        {
            // todo log success
        }
        else
        {
            // todo log failure
        }
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
