using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;

namespace EspEdit.ViewModels
{
    /// <summary>
    /// Represents a record in a CollectionView
    /// Use Key to look up record in doctionary
    /// </summary>
    public class RecordViewModel
    {
        public RecordViewModel(string key)
        {
            Key = key;
        }

        public string Key { get; set; }

        public bool IsSelected { get; set; }
    }
}
