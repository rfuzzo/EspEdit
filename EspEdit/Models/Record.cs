using System.Text.Json;

namespace EspEdit.ViewModels;

public class Record
{
    public Record(JsonElement item, string type, string key)
    {
        Item = item;
        Type = type;
        Key = key;
    }

    public string Type { get; set; }
    public string Key { get; set; }
    public JsonElement Item { get; set; }
}