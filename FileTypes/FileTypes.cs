using System.Text.Json;

namespace Tools;

public static class FileTypes
{
    public static Dictionary<string, string> Types { get; private set; } = new();

    public static void Load(string jsonPath)
    {
        if (!File.Exists(jsonPath))
            throw new FileNotFoundException("FileTypes.json not found.", jsonPath);

        var json = File.ReadAllText(jsonPath);

        var loaded = JsonSerializer.Deserialize<Dictionary<string, string>>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        Types = loaded ?? new Dictionary<string, string>();
    }
}