using System.IO;
using System.Text.Json;

namespace core.Utility
{
    public static class JsonUtility
    {
        public static T DeserializeFromFile<T>(string file)
        {
            string jsonData = File.ReadAllText(file);
            return JsonSerializer.Deserialize<T>(jsonData);
        }
    }
}
