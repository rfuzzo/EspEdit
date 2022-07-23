using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
using Tes3Json.Models;
using Tes3Json.Services;
using static System.Net.Mime.MediaTypeNames;

namespace Tes3Json.ViewModels;

[ObservableObject]
public partial class MainPageViewModel
{
    private readonly IDialogService _dialogService;
    private readonly ITes3ConvService _tes3ConvService;
    private readonly IFileService _fileService;

    //[ObservableProperty]
    private readonly Dictionary<string, Record> flatRecords = new();

    [ObservableProperty]
    private ObservableCollection<RecordGroupViewModel> records;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveRecordCommand))]
    [NotifyCanExecuteChangedFor(nameof(RestoreRecordCommand))]
    private string? selectedRecordText;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveAsCommand))]
    private string? currentFile;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedRecordText))]
    [NotifyCanExecuteChangedFor(nameof(SaveRecordCommand))]
    private RecordViewModel selectedRecord;
    partial void OnSelectedRecordChanged(RecordViewModel value)
    {
        if (value is null)
        {
            return;
        }

        var key = value.Key;
        if (flatRecords.TryGetValue(key, out var record))
        {
            SelectedRecordText = record.Item.GetRawText();
        }
    }

    #region Editor

    [ObservableProperty]
    private bool editorInfoIsOpen;

    [ObservableProperty]
    private string editorInfoTitle;

    [ObservableProperty]
    private string editorInfoMessage;

    //[ObservableProperty]
    //private InfoBarSeverity editorInfoSeverity;

    [ObservableProperty]
    private bool isFullTextSearchEnabled;

    #endregion

    public MainPageViewModel(IDialogService dialogService, ITes3ConvService tes3ConvService, IFileService fileService)
    {
        records = new ObservableCollection<RecordGroupViewModel>();

        _dialogService = dialogService;
        _tes3ConvService = tes3ConvService;
        _fileService = fileService;
    }

    private bool IsJson()
    {
        ArgumentNullException.ThrowIfNull(CurrentFile);
        return Path.GetExtension(CurrentFile).ToUpper() == ".JSON";
    }

    [RelayCommand]
    private void PerformSearch(string text)
    {
        string filter = text;
        UpdateGroupedRecordsWith(filter);
    }

    private void UpdateGroupedRecordsWith(string filter = "")
    {
        IEnumerable<Record> filteredRecords = new List<Record>();
        filteredRecords = string.IsNullOrEmpty(filter)
            ? (IEnumerable<Record>)flatRecords.Values
            : IsFullTextSearchEnabled
                ? flatRecords.Values.Where(x => x.Item.GetRawText().Contains(filter, StringComparison.InvariantCultureIgnoreCase))
                : flatRecords.Values.Where(x => x.Key.ToLower().Contains(filter.ToLower()));

        ILookup<string, Record> groups = filteredRecords.ToLookup(x => x.Type);

        Records.Clear();
        foreach (IGrouping<string, Record> group in groups)
        {
            IEnumerable<RecordViewModel> vals = group.Select(x => new RecordViewModel(x.Key));

            Records.Add(new(group.Key, vals));
        }
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        // todo fix non-windows cases
        CurrentFile = await _fileService.OpenFileAsync("Open file", new string[] { ".json", ".esp", ".esm" });

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
            string? type = item.GetProperty("type").GetString();
            ArgumentNullException.ThrowIfNull(type);

            string? id = item.TryGetProperty("id", out JsonElement idValue)
                ? idValue.GetString()
                : type switch
                {
                    "Header" => item.GetProperty("version").GetSingle().ToString(),
                    "PathGrid" => item.GetProperty("cell").GetString(),
                    "Info" => item.GetProperty("info_id").GetString(),
                    _ => throw new ArgumentException(),
                };
            string key = $"{type}.{id}";
            flatRecords.Add(key, new(item, type, key));
        }

        UpdateGroupedRecordsWith();
    }

    [RelayCommand]
    private async Task ReloadAsync() => await LoadFileInternalAsync();

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsAsync()
    {
        ArgumentNullException.ThrowIfNull(CurrentFile);

        string path = await _fileService.SaveFileAsync("Save As...", new string[] { ".json", ".esp", ".esm" }, Path.GetFileName(CurrentFile));

        await SaveInternal(path);
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        ArgumentNullException.ThrowIfNull(CurrentFile);

        if (!IsJson())
        {
            // backup the esp file
            // if a backup file already exists then overwrite
            string backup = $"{CurrentFile}.bak";
            if (!File.Exists(backup))
            {
                File.Copy(CurrentFile, backup);
            }
        }

        await SaveInternal(CurrentFile);
    }

    private async Task SaveInternal(string path)
    {
        string file = "[\n";
        foreach (Record item in flatRecords.Values)
        {
            string text = item.Item.GetRawText();
            file += $"{text},\n";
        }
        file = file.TrimEnd('\n').TrimEnd(',');
        file += "\n]";

        if (Path.GetExtension(path) == ".json")
        {
            File.WriteAllText(path, file);
            await _dialogService.DisplayAlert("Save", "File Saved", "OK");
        }
        else
        {
            if (await _tes3ConvService.ConvertJsonToEspAsync(file, path))
            {
                await _dialogService.DisplayAlert("Save", "File Saved", "OK");
            }
        }
    }

    private bool CanSave() => !string.IsNullOrEmpty(CurrentFile);

    [RelayCommand]
    private void DeleteRecord(RecordViewModel viewModel)
    {
        flatRecords.Remove(viewModel.Key);
        UpdateGroupedRecordsWith();
    }

    [RelayCommand]
    private void Delete()
    {
        IEnumerable<RecordViewModel> flatVms = Records.SelectMany(x => x);
        IEnumerable<string> keysToRemove = flatVms.Where(x => x.IsChecked).Select(x => x.Key);

        foreach (string item in keysToRemove)
        {
            flatRecords.Remove(item);
        }

        UpdateGroupedRecordsWith();
    }

    [RelayCommand(CanExecute = nameof(CanSaveRecord))]
    private async Task SaveRecord()
    {
        if (SelectedRecordText is null || SelectedRecord is null)
        {
            return;
        }

        JsonElement element;
        try
        {
            element = JsonSerializer.Deserialize<JsonElement>(SelectedRecordText);
        }
        catch (Exception e)
        {
            await DisplayInfoBarMessage("Could not save record", $"{e.Message}");
            //await _dialogService.DisplayAlert("Could not save record", $"{e.Message}", "OK");
            return;
        }

        string key = SelectedRecord.Key;
        flatRecords[key].Item = element;
        EditorInfoIsOpen = false;
    }
    private async Task DisplayInfoBarMessage(string title, string message)
    {
        EditorInfoIsOpen = true;
        EditorInfoMessage = message;
        EditorInfoTitle = title;

        //await Task.Delay(10);
        //EditorInfoIsOpen = false;
    }

    private bool CanSaveRecord() => SelectedRecord is not null;

    [RelayCommand(CanExecute = nameof(CanRestoreRecord))]
    private void RestoreRecord()
    {
        ArgumentNullException.ThrowIfNull(SelectedRecord);
        string key = SelectedRecord.Key;
        Record record = flatRecords[key];

        SelectedRecordText = record.Item.GetRawText();
        EditorInfoIsOpen = false;
    }
    private bool CanRestoreRecord() => SelectedRecord is not null;



    [RelayCommand]
    private async Task CheckForUpdate()
    {

    }
}
