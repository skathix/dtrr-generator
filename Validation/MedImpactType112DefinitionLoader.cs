using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tools.Validation;

namespace Tools;

public sealed class MedImpactType112Definitions
{
    public string? StartDate { get; set; }
    public string? Description { get; set; }

    // SegmentCode -> RecordDefinition
    public Dictionary<string, RecordDefinition> Segments { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public static class MedImpactType112DefinitionLoader
{
    public static MedImpactType112Definitions Load(string path)
    {
        var json = File.ReadAllText(path);

        var raw = JsonSerializer.Deserialize<Type112RawRoot>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (raw == null)
            throw new InvalidOperationException($"Unable to parse Type 112 definition: {path}");

        var result = new MedImpactType112Definitions
        {
            StartDate = raw.StartDate,
            Description = raw.Description
        };

        foreach (var kvp in raw.RecordTypes)
        {
            var segCode = kvp.Key;
            var seg = kvp.Value;

            var fields = seg.Fields.Select(f => new FieldDefinition
            {
                Name = f.Name,
                DisplayName = f.DisplayName,
                Length = f.Length,
                Valid = f.Valid,
                IsRequired = f.Required ?? false
            }).ToList();

            result.Segments[segCode] = new RecordDefinition
            {
                Description = $"Type 112 Segment {segCode}",
                RecordLength = seg.RecordLength,
                Fields = fields
            };
        }

        return result;
    }

    private sealed class Type112RawRoot
    {
        [JsonPropertyName("start_date")]
        public string? StartDate { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("record_types")]
        public Dictionary<string, Type112RawSegment> RecordTypes { get; set; } = new();
    }

    private sealed class Type112RawSegment
    {
        [JsonPropertyName("record_length")]
        public int RecordLength { get; set; }

        [JsonPropertyName("fields")]
        public List<Type112RawField> Fields { get; set; } = new();
    }

    private sealed class Type112RawField
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
        public bool? Required { get; set; }
    }
}