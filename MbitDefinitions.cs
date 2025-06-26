using System.Text.Json.Serialization;

public class MbitDefinitions
{
    public Dictionary<string, VersionDefinition> Versions { get; set; } = new();
}

public class VersionDefinition
{
    [JsonPropertyName("record_length")]
    public int Record_Length { get; set; }

    [JsonPropertyName("record_types")]
    public Dictionary<string, RecordDefinition> Record_Types { get; set; } = new();
    
    [JsonPropertyName("start_date")]
    public DateTime Start_Date { get; set; }
}

public class RecordDefinition
{
   public List<FieldDefinition> Fields { get; set; } = new();

   public string Description { get; set; }
}


public class FieldDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = "";

    [JsonPropertyName("length")]
    public int Length { get; set; }

    [JsonPropertyName("required")]
    public bool IsRequired { get; set; }

    [JsonPropertyName("valid")] public string Valid { get; set; } = "";
}