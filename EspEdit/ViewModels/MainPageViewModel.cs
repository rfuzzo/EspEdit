using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EspEdit.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EspEdit.ViewModels;

[ObservableRecipient]
public partial class MainPageViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedRecordText))]
    private Record selectedRecord;
    partial void OnSelectedRecordChanged(Record value)
    {
        SelectedRecordText = value.Item.GetRawText();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedRecordText))]
    private IList<Record> selectedRecords;
    partial void OnSelectedRecordsChanged(IList<Record> value)
    {
        SelectedRecordText = value.FirstOrDefault() is not null ? value.FirstOrDefault().Item.GetRawText() : "";
    }

    [ObservableProperty]
    private string selectedRecordText;

    [ObservableProperty]
    private ObservableCollection<RecordGroup> records;

    public MainPageViewModel(/*ISettingsService settingsService*/)
    {
        //_settingsService = settingsService;
        _settingsService = App.Current.Services.GetService<ISettingsService>();

        records = new ObservableCollection<RecordGroup>();

        SelectedRecordText = "TEST TEST ";
    }

    [RelayCommand]
    private async Task Load()
    {
        string json = "";

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
                json = File.ReadAllText(result.FullPath);
            }
        }
        catch (Exception ex)
        {
            // The user canceled or something went wrong
            // todo logging
            return;
        }

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


    [RelayCommand]
    private void SelectionChanged(IList<Record> selectionChangedCommandParameter)
    {


    }

    [RelayCommand]
    private void Save()
    {
        // create backup

        // save jsons


    }
    [RelayCommand]
    private void Delete()
    {



    }
}

public class RecordGroup : List<Record>
{
    public string Name { get; private set; }

    public RecordGroup(string name, IEnumerable<Record> records) : base(records)
    {
        Name = name;
    }
}

public class Record
{
    public Record(JsonElement item, string type, string id)
    {
        Item = item;
        Type = type;
        Id = id;
    }

    public string Type { get; set; }
    public string Id { get; set; }
    public JsonElement Item { get; }
}