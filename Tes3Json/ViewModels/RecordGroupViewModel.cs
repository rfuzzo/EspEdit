using CommunityToolkit.Mvvm.ComponentModel;

namespace Tes3Json.ViewModels;

[ObservableObject]
public partial class RecordGroupViewModel : List<RecordViewModel>
{
    public string Name { get; private set; }

    [ObservableProperty]
    private bool isChecked;
    partial void OnIsCheckedChanged(bool value)
    {
        foreach (RecordViewModel item in this)
        {
            item.IsSelected = value;
        }
    }

    public RecordGroupViewModel(string name, IEnumerable<RecordViewModel> records) : base(records)
    {
        Name = name;
    }
}
