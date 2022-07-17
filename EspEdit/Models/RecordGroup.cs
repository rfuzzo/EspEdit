namespace EspEdit.ViewModels;

public class RecordGroup : List<Record>
{
    public string Name { get; private set; }

    public RecordGroup(string name, IEnumerable<Record> records) : base(records)
    {
        Name = name;
    }
}
