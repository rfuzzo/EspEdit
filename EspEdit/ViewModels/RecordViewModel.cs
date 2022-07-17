using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;

namespace EspEdit.ViewModels
{
    [ObservableObject]
    public partial class RecordViewModel
    {
        public RecordViewModel(string type, JsonElement record)
        {
            Type = type;
            Record = record;
        }

        [ObservableProperty]
        private string type;

        public JsonElement Record { get; set; }

    }
}
