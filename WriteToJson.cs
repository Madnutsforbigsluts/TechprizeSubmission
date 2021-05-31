using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Glyph.Utilities
{
    public static class WriteToJson
    {
        public static void DumpContents(string fileName, JObject jObject)
        {
            using (StreamWriter file = File.CreateText(fileName))
            using (JsonWriter writer = new JsonTextWriter(file))
            {
                jObject.WriteTo(writer);
            }
        }
    }
}
