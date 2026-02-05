using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tools.Validation;

namespace Tools;

public static class MedImpactDefinitionLoader
{
    public static RecordDefinition Load(string path)
    {
        var json = File.ReadAllText(path);

        var raw = JsonSerializer.Deserialize<MedImpactRawDefinition>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        if (raw == null)
            throw new InvalidOperationException($"Unable to parse MedImpact definition: {path}");

        var fields = raw.Fields.Select(f => new FieldDefinition
        {
            Name = f.Name,
            DisplayName = f.DisplayName,
            Length = f.Length,
            Valid = f.Valid,
            IsRequired = f.Required
        }).ToList();

        // ✅ Return your EXISTING RecordDefinition type
        return new RecordDefinition
        {
            Description = raw.Description ?? "MedImpact Record",
            RecordLength = raw.RecordLength,
            Fields = fields
        };
    }

    private sealed class MedImpactRawDefinition
    {
        [JsonPropertyName("start_date")]
        public string? StartDate { get; set; }

        [JsonPropertyName("record_length")]
        public int RecordLength { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("fields")]
        public List<MedImpactRawField> Fields { get; set; } = new();
    }

    private sealed class MedImpactRawField
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = "";

        [JsonPropertyName("length")]
        public int Length { get; set; }

        [JsonPropertyName("valid")]
        public string? Valid { get; set; }

        [JsonPropertyName("required")]
        public bool Required { get; set; }
    }
}