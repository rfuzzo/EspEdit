using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;

namespace Tes3Json.ViewModels;

/// <summary>
/// Represents a record in a CollectionView
/// Use Key to look up record in doctionary
/// </summary>
[ObservableObject]
public partial class RecordViewModel
{
    public RecordViewModel(string key)
    {
        Key = key;
    }

    public string Key { get; set; }

    [ObservableProperty]
    private bool isSelected;
}
