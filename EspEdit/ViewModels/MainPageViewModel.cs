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
    private readonly IDialogService _dialogService;
    private readonly ITes3ConvService _tes3ConvService;

    //[ObservableProperty]
    private Dictionary<string, Record> flatRecords = new();

    //[ObservableProperty]
    //private ObservableCollection<RecordGroup> records;
    public ObservableCollection<RecordGroup> Records { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveRecordCommand))]
    [NotifyCanExecuteChangedFor(nameof(RestoreRecordCommand))]
    private string selectedRecordText;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveAsCommand))]
    private string currentFile;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedRecordText))]
    [NotifyCanExecuteChangedFor(nameof(SaveRecordCommand))]
    private RecordViewModel selectedRecord;
    partial void OnSelectedRecordChanged(RecordViewModel value)
    {
        var key = value.Key;
        if (flatRecords.TryGetValue(key, out var record))
        {
            SelectedRecordText = record.Item.GetRawText();
        }
    }

    public MainPageViewModel()
    {
        Records = new ObservableCollection<RecordGroup>();


        // todo do this properly with constructor injection
        _dialogService = App.Current.Services.GetService<IDialogService>();
        _tes3ConvService = App.Current.Services.GetService<ITes3ConvService>();
    }

    private bool IsJson()
    {
        return Path.GetExtension(CurrentFile).ToUpper() == ".JSON";
    }

    [RelayCommand]
    private void PerformSearch(string text)
    {
        string filter = text.ToLower();
        UpdateGroupedRecordsWith(filter);
    }

    private void UpdateGroupedRecordsWith(string filter = null)
    {
        IEnumerable<Record> filteredRecords = !string.IsNullOrEmpty(filter)
            ? flatRecords.Values.Where(x => x.Key.ToLower().Contains(filter))
            : flatRecords.Values;

        List<RecordGroup> _records = new();
        ILookup<string, Record> groups = filteredRecords.ToLookup(x => x.Type);

        //Records.Clear();
        foreach (IGrouping<string, Record> group in groups)
        {
            IEnumerable<RecordViewModel> vals = group.Select(x => new RecordViewModel(x.Key));

            _records.Add(new(group.Key, vals));
        }

        Records = new(_records);
        OnPropertyChanged(nameof(Records));
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
        catch (Exception)
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

        string json = IsJson() ? File.ReadAllText(CurrentFile) : await _tes3ConvService.GetJsonFromEsp(new FileInfo(CurrentFile));

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
        catch (Exception e)
        {
            // todo logging
            await _dialogService.DisplayAlert("Could not load file", $"{e.Message}\n{e.StackTrace}", "OK");
            return;
        }

        flatRecords.Clear();
        foreach (JsonElement item in document.EnumerateArray())
        {
            string type = item.GetProperty("type").GetString();

            string id = "";
            if (item.TryGetProperty("id", out JsonElement idValue))
            {
                id = idValue.GetString();
            }
            else
            {
                // todo ids
                id = type switch
                {
                    "Header" => item.GetProperty("author").GetString(),
                    "PathGrid" => item.GetProperty("cell").GetString(),
                    "Info" => item.GetProperty("info_id").GetString(),
                    _ => throw new ArgumentException(),
                };
            }

            string key = $"{type}.{id}";
            flatRecords.Add(key, new(item, type, key));
        }

        UpdateGroupedRecordsWith();
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

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsAsync()
    {
        // todo save as
        await Task.Delay(1);
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
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
        //foreach (RecordGroup group in Records)
        {
            //foreach (Record item in group)
            foreach (Record item in flatRecords.Values)
            {
                string text = item.Item.GetRawText();
                file += $"{text},\n";
            }
        }
        file = file.TrimEnd('\n').TrimEnd(',');
        file += "\n]";

        string espPath = IsJson() ? Path.ChangeExtension(CurrentFile, ".esp") : CurrentFile;
        if (await _tes3ConvService.ConvertJsonToEspAsync(file, espPath))
        {
            // todo log success
            await _dialogService.DisplayAlert("Save", "File Saved", "OK");
        }
        else
        {
            // todo log failure
        }
    }
    private bool CanSave()
    {
        return !string.IsNullOrEmpty(CurrentFile);
    }

    // todo make async
    [RelayCommand]
    private void Delete()
    {
        IEnumerable<RecordViewModel> flatVms = Records.SelectMany(x => x);
        IEnumerable<string> keysToRemove = flatVms.Where(x => x.IsSelected).Select(x => x.Key);

        foreach (string item in keysToRemove)
        {
            flatRecords.Remove(item);
        }

        //if (selectedRecords.Any())
        //{
        //    IEnumerable<Record> newFlatRecords = flatRecords.Except(selectedRecords);
        //    ObservableCollection<RecordGroup> newRecords = new();

        //    foreach (IGrouping<string, Record> group in newFlatRecords.ToLookup(x => x.Type))
        //    {
        //        IEnumerable<Record> vals = group.ToList();
        //        newRecords.Add(new(group.Key, vals));
        //    }
        //    Records = newRecords;
        //}

        UpdateGroupedRecordsWith();
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

    [RelayCommand(CanExecute = nameof(CanSaveRecord))]
    private async Task SaveRecord()
    {
        // parse json
        JsonElement element;
        try
        {
            element = JsonSerializer.Deserialize<JsonElement>(SelectedRecordText);
        }
        catch (Exception e)
        {
            await _dialogService.DisplayAlert("Could not save record", $"{e.Message}", "OK");
            return;
        }

        string key = SelectedRecord.Key;
        flatRecords[key].Item = element;
    }
    private bool CanSaveRecord()
    {
        return SelectedRecord is not null;
    }

    [RelayCommand(CanExecute = nameof(CanRestoreRecord))]
    private void RestoreRecord()
    {
        string key = SelectedRecord.Key;
        Record record = flatRecords[key];

        SelectedRecordText = record.Item.GetRawText();
    }
    private bool CanRestoreRecord()
    {
        return SelectedRecord is not null;
    }
}
