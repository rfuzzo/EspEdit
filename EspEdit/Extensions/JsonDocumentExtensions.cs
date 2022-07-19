using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EspEdit.Extensions
{
    internal static class JsonDocumentExtensions
    {
        public static string ToJsonString(this JsonDocument doc)
        {
            using MemoryStream stream = new();
            using (Utf8JsonWriter writer = new(stream, new JsonWriterOptions { Indented = true }))
            {
                doc.WriteTo(writer);
            }

            return System.Text.Encoding.UTF8.GetString(stream.ToArray());
        }

    }
}
