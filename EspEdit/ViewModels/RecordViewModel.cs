using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;

namespace EspEdit.ViewModels
{
    public class RecordViewModel
    {
        public RecordViewModel(string id)
        {
            Id = id;
        }

        public string Id { get; set; }

        public bool IsSelected2 { get; set; }
    }
}
