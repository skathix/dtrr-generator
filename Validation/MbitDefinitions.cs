using System.Text.Json.Serialization;

namespace Tools.Validation;

public class MbitDefinitions
{
    public Dictionary<string, VersionDefinition> Versions { get; set; } = new();
}

public class VersionDefinition

{
    [JsonPropertyName("default_record_length")]
    public int DefaultRecordLength { get; set; }

    [JsonPropertyName("record_types")]
    public Dictionary<string, RecordDefinition> RecordTypes { get; set; } = new();

    [JsonPropertyName("start_date")]
    public DateTime StartDate { get; set; }
}


public class RecordDefinition

{
    // Per-record override (you already use this in transaction 61/92)
    [JsonPropertyName("record_length")]
    public int? RecordLength { get; set; }

    [JsonPropertyName("accepted_lengths")]
    public List<int>? AcceptedLengths { get; set; }

    [JsonPropertyName("fields")]
    public List<FieldDefinition> Fields { get; set; } = new();

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";
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

    [JsonPropertyName("valid")]
    public string Valid { get; set; } = "";
}