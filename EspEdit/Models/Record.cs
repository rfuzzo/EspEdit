using System.Text.Json;

namespace EspEdit.ViewModels;

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
    public JsonElement Item { get; set; }

    public bool IsSelected { get; set; }
}