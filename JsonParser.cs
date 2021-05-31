using System.IO;
using Newtonsoft.Json;

namespace Glyph.Utilities
{
    public static class JsonParser
    {
        public static dynamic FetchJson(string fileName)
        {
            string jsonString = File.ReadAllText(fileName);
            return JsonConvert.DeserializeObject<dynamic>(jsonString);
        }
    }
}
