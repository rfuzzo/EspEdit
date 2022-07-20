namespace EspEdit.ViewModels;

public class RecordGroup : List<RecordViewModel>
{
    public string Name { get; private set; }

    public RecordGroup(string name, IEnumerable<RecordViewModel> records) : base(records)
    {
        Name = name;
    }
}
