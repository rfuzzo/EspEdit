using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EspEdit.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    private readonly ISettingsService _settingsService;

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
        SelectedRecordText = value.Item.GetRawText();
    }

    //[ObservableProperty]
    //[NotifyPropertyChangedFor(nameof(SelectedRecordText))]
    //private ObservableCollection<Record> selectedRecords;
    //partial void OnSelectedRecordsChanged(ObservableCollection<Record> value)
    //{
    //    SelectedRecordText = value.FirstOrDefault() is not null ? value.FirstOrDefault().Item.GetRawText() : "";
    //}

    public MainPageViewModel(/*ISettingsService settingsService*/)
    {
        //_settingsService = settingsService;
        _settingsService = App.Current.Services.GetService<ISettingsService>();

        records = new ObservableCollection<RecordGroup>();
    }

    [RelayCommand]
    private async Task Load()
    {
        try
        {
            // todo json convert
            FilePickerFileType customFileType = new(
            new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.iOS, new[] { ".json" } }, // or general UTType values
                { DevicePlatform.Android, new[] { ".json" } },
                { DevicePlatform.WinUI, new[] { ".json" } },
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

        LoadFile();
    }

    // todo make async
    private void LoadFile()
    {
        string json = File.ReadAllText(CurrentFile);
        if (string.IsNullOrEmpty(json))
        {
            return;
        }

        JsonElement dict;
        try
        {
            JsonDocument doc = JsonDocument.Parse(json);
            dict = JsonSerializer.Deserialize<JsonElement>(json);
        }
        catch (Exception ex)
        {
            // todo logging
            return;
        }

        List<Record> records = new();
        foreach (JsonElement item in dict.EnumerateArray())
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


    //[RelayCommand]
    //private void SelectionChanged(IList<object> selectionChangedCommandParameter)
    //{
    //    if (selectionChangedCommandParameter == null)
    //    {
    //        return;
    //    }

    //    IEnumerable<Record> cast = selectionChangedCommandParameter.Cast<Record>();
    //    if (cast is IEnumerable<Record> list)
    //    {
    //        SelectedRecords = new(list);
    //    }
    //}

    // todo make async
    [RelayCommand]
    private void Reload()
    {
        LoadFile();
    }

    [RelayCommand]
    private static void Exit()
    {
        Application.Current.Quit();
    }

    // todo make async
    [RelayCommand]
    private void Save()
    {
        // backup file
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
                file += item.Item.GetRawText();
            }
        }

        // save current file
        File.WriteAllText(CurrentFile, file);

        // convert to esp
        // todo
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
    private void ChangeTheme()
    {

    }
}
