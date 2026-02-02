using System.Text.Json;
using Tools.Validation;

namespace Tools;

public static class MbitDefinitionLoader
{
    public static MbitDefinitions LoadAll(string folderPath)
    {
        var definitions = new MbitDefinitions();

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        foreach (var file in Directory.GetFiles(folderPath, "*.json"))
        {
            var version = Path.GetFileNameWithoutExtension(file);
            var json = File.ReadAllText(file);
            var versionDef = JsonSerializer.Deserialize<VersionDefinition>(json, jsonOptions);

            if (versionDef != null)
                definitions.Versions[version] = versionDef;
        }

        return definitions;
    }
}