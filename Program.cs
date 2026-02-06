using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Tools.Tools;
using Tools.Validation;

namespace Tools;

class FileIngestor
{
    private static void Main(string[] args)
    {
        // Load file types
        FileTypes.Load(Path.Combine("FileTypes", "FileTypes.json"));

        // Register supported flows ONCE (outside the loop)
        var handlers =
            new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase)
            {
                ["MBIT"] = RunMbitFlow, ["MEDIMPACT"] = RunMedImpactFlow,
                // Later: ["DTRR"] = RunDtrrFlow,
                // Later: ["REPLY_CODES"] = RunReplyCodesFlow,
            };

        while (true)
        {
            Console.Clear();
            Console.WriteLine("Available file types:");
            foreach (var kvp in FileTypes.Types)
            {
                var supported = handlers.ContainsKey(kvp.Key)
                    ? " (supported)"
                    : " (coming soon)";

                Console.WriteLine($" - {kvp.Key}: {kvp.Value}{supported}");
            }

            Console.Write("Select file type (key, e.g. MBIT): ");
            var selectedFileType =
                Console.ReadLine()?.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(selectedFileType))
            {
                Console.WriteLine("No selection provided.\n");
                continue;
            }

            if (!handlers.TryGetValue(selectedFileType, out var run))
            {
                Console.WriteLine($"\n[{selectedFileType}] Coming soon!\n");
                continue;
            }

            if (!FileTypes.Types.ContainsKey(selectedFileType))
            {
                Console.WriteLine($"Unknown file type '{selectedFileType}'.\n");
                continue;
            }


            run();

            Console.Write("\nDo you want to ingest another item? (y/n): ");
            var again = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (again is not ("y" or "yes")) break;

            Console.WriteLine();
        }

        Console.WriteLine("Goodbye!");
    }


    private static void RunMbitFlow()
    {
        var allDefinitions = MbitDefinitionLoader.LoadAll("Definitions");

        // Pick version (default to 18.8 if present, else latest by StartDate)
        var selectedVersion =
            ChooseMbitVersion(allDefinitions, preferredDefault: "18.8");
        var versionDef = allDefinitions.Versions[selectedVersion];

        
        var mode = ChooseMode();

        if (mode is "file" or "f")
        {
            Console.Write("Enter path to file: ");
            var filePath = Console.ReadLine()!;

            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found.");
                return;
            }

            ProcessFileWithOutputOption(filePath, selectedVersion
                , allDefinitions);
        }
        else
        {
            Console.WriteLine("Paste a single full record here:");
            var inputString = Console.ReadLine();

            if (string.IsNullOrEmpty(inputString))
            {
                Console.WriteLine("No input provided.");
                return;
            }

            // Preserve internal spacing; only remove line terminators + BOM
            var line = inputString.TrimEnd('\r', '\n').TrimStart('\uFEFF');

            // Must be long enough to read transaction code
            if (line.Length < 61)
            {
                Console.WriteLine(
                    $"Invalid length: {line.Length}. Minimum 61 required to read transaction code at position 59.");
                return;
            }

            var recordType = line.Substring(59, 2).Trim();

            versionDef.RecordTypes.TryGetValue(recordType, out var recordDef);
            var acceptedLengths = GetAcceptedLengths(versionDef, recordDef);

            // Console paste often loses trailing spaces; pad to expected length
            line = PadToExpectedLength(line, acceptedLengths);

            if (!acceptedLengths.Contains(line.Length))
            {
                Console.WriteLine(
                    $"Invalid length: {line.Length}. Expected: {string.Join(", ", acceptedLengths)}");
                return;
            }


            Console.WriteLine(
                $"\nRecord Type {recordType}: {recordDef?.Description ?? $"Unknown record type ({recordType})"}");

            var fields = recordDef?.Fields ?? new List<FieldDefinition>();
            ProcessRecord(line, fields);


            Console.Write(
                "Would you like to save the results? (none/txt/csv): ");
            var outputFormat = Console.ReadLine()?.Trim().ToLowerInvariant();

            // Always show field-by-field (your primary use case)
            ProcessRecord(line, fields);

            if (outputFormat is "txt" or "csv")
                SaveSingleRecordOutput(line, recordType, fields, recordDef
                    , outputFormat);
        }
    }

    private static string ChooseMbitVersion(MbitDefinitions allDefinitions
        , string preferredDefault)
    {
        var ordered = allDefinitions.Versions
            .OrderBy(v => v.Value.StartDate)
            .ToList();

        if (ordered.Count == 0)
            throw new InvalidOperationException(
                "No MBIT definition files found in Definitions folder.");

        var defaultVersion =
            allDefinitions.Versions.ContainsKey(preferredDefault)
                ? preferredDefault
                : ordered.Last().Key;

        Console.WriteLine("\nAvailable MBIT versions:");
        foreach (var kvp in ordered)
        {
            var marker = kvp.Key == defaultVersion ? "  <-- default" : "";
            Console.WriteLine(
                $" - {kvp.Key} (starts: {kvp.Value.StartDate:yyyy-MM-dd}){marker}");
        }

        Console.Write(
            $"\nEnter the version (press Enter for {defaultVersion}): ");
        var input = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(input))
            return defaultVersion;

        if (!allDefinitions.Versions.ContainsKey(input))
        {
            Console.WriteLine(
                $"Unknown version '{input}'. Using default '{defaultVersion}'.");
            return defaultVersion;
        }

        return input;
    }

    private static IReadOnlyList<int> GetAcceptedLengths(
        VersionDefinition versionDef, RecordDefinition? recordDef)
    {
        if (recordDef?.AcceptedLengths is { Count: > 0 } list) return list;
        if (recordDef?.RecordLength is int rl) return new[] { rl };
        return new[] { versionDef.DefaultRecordLength };
    }

    private static string PadToExpectedLength(string line
        , IReadOnlyList<int> acceptedLengths)
    {
        if (acceptedLengths.Contains(line.Length))
            return line;

        var target = acceptedLengths.OrderBy(x => x)
            .FirstOrDefault(x => x >= line.Length);
        if (target == 0)
            return line;

        return line.PadRight(target, ' ');
    }

    private static void ProcessRecord(string input
        , List<FieldDefinition> fields)
    {
        var sectionLengths = fields.Select(f => f.Length).ToList();
        var sections =
            StringDivider.DivideStringIntoVariableSections(input
                , sectionLengths);

        Console.WriteLine("\nProcessed record Data:");

        for (int i = 0; i < Math.Min(sections.Count, fields.Count); i++)
        {
            var field = fields[i];
            var label = string.IsNullOrWhiteSpace(field.DisplayName)
                ? field.Name
                : field.DisplayName;
            var rawValue = sections[i];

            // Visualize spaces as '-' for quick alignment checking (display only)
            var visibleValue = rawValue.Replace(' ', '-');

            var requiredMark = field.IsRequired ? "REQUIRED" : "";
            var validValue = string.IsNullOrWhiteSpace(field.Valid)
                ? ""
                : $" ({field.Valid})";

            Console.WriteLine(
                $"{label}({field.Length}) {requiredMark}: {visibleValue}{validValue}");
        }
    }

    private static string CsvEscape(string s)
    {
        if (s.Contains('"')) s = s.Replace("\"", "\"\"");
        if (s.Contains(',') || s.Contains('\n') || s.Contains('\r') ||
            s.Contains('"'))
            return $"\"{s}\"";
        return s;
    }

    private static void SaveSingleRecordOutput(
        string input,
        string recordType,
        List<FieldDefinition> fields,
        RecordDefinition? record,
        string outputFormat)
    {
        var sectionLengths = fields.Select(f => f.Length).ToList();
        var sections =
            StringDivider.DivideStringIntoVariableSections(input
                , sectionLengths);

        var txtOutput = new StringBuilder();
        var csvOutput = new StringBuilder();
        var description = record?.Description ??
                          $"Unknown record type ({recordType})";

        txtOutput.AppendLine(record?.Description ??
                             $"Record Type {recordType}");
        csvOutput.AppendLine(
            "RecordNumber,RecordType,RecordDescription,FieldName,Length,Required,Value,Valid");

        for (int i = 0; i < fields.Count; i++)
        {
            var field = fields[i];
            var label = string.IsNullOrWhiteSpace(field.DisplayName)
                ? field.Name
                : field.DisplayName;
            var value = i < sections.Count ? sections[i] : "";
            var valid = field.Valid ?? "";

            txtOutput.AppendLine(
                $"{label} ({field.Length}) {(field.IsRequired ? "REQUIRED" : "")}: {value}{(string.IsNullOrWhiteSpace(valid) ? "" : $" ({valid})")}");

            csvOutput.AppendLine(string.Join(",",
                "1",
                CsvEscape(recordType),
                CsvEscape(description),
                CsvEscape(label),
                field.Length.ToString(),
                field.IsRequired.ToString(),
                CsvEscape(value),
                CsvEscape(valid)
            ));
        }

        if (outputFormat == "txt")
        {
            File.WriteAllText("output_single.txt", txtOutput.ToString());
            Console.WriteLine("Results saved to output_single.txt");
        }
        else if (outputFormat == "csv")
        {
            File.WriteAllText("output_single.csv", csvOutput.ToString());
            Console.WriteLine("Results saved to output_single.csv");
        }
    }

    private static void ProcessFileWithOutputOption(string filePath
        , string selectedVersion, MbitDefinitions allDefinitions)
    {
        Console.Write("Would you like to save the results? (none/txt/csv): ");
        var outputFormat = Console.ReadLine()?.Trim().ToLowerInvariant();

        var txtOutput = new StringBuilder();
        var csvOutput = new StringBuilder();
        csvOutput.AppendLine(
            "RecordNumber,RecordType,RecordDescription,FieldName,Length,Required,Value,Valid");

        var versionDef = allDefinitions.Versions[selectedVersion];
        var lines = File.ReadAllLines(filePath);
        int recordCounter = 0;

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd('\r', '\n'); // don't mutate spaces

            if (line.StartsWith("AAAAAAHEADER"))
            {
                Console.WriteLine("Skipping header record.");
                continue;
            }

            if (line.Length < 61)
            {
                Console.WriteLine(
                    $"Line too short ({line.Length}) to determine record type.");
                continue;
            }

            var recordType = line.Substring(59, 2).Trim();
            versionDef.RecordTypes.TryGetValue(recordType, out var recordDef);
            var description = recordDef?.Description ??
                              $"Unknown record type ({recordType})";

            var acceptedLengths = GetAcceptedLengths(versionDef, recordDef);
            if (!acceptedLengths.Contains(line.Length))
            {
                Console.WriteLine(
                    $"Line has invalid length: {line.Length}. Expected: {string.Join(", ", acceptedLengths)} for type {recordType}");
                continue;
            }

            var fields = recordDef?.Fields ?? new List<FieldDefinition>();
            var sectionLengths = fields.Select(f => f.Length).ToList();
            var sections =
                StringDivider.DivideStringIntoVariableSections(line
                    , sectionLengths);

            Console.WriteLine(
                $"\nRecord {++recordCounter} — Type: {recordType} — {description}");
            txtOutput.AppendLine(
                $"Record {recordCounter} — Type: {recordType} — {description}");


            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                var label = string.IsNullOrWhiteSpace(field.DisplayName)
                    ? field.Name
                    : field.DisplayName;
                var value = i < sections.Count ? sections[i] : "";
                var valid = field.Valid ?? "";

                // Visualize spaces as '-' for console view
                var visibleValue = value.Replace(' ', '-');

                var lineTxt =
                    $"{label} ({field.Length}) {(field.IsRequired ? "REQUIRED" : "")}: {visibleValue}{(string.IsNullOrWhiteSpace(valid) ? "" : $" ({valid})")}";
                Console.WriteLine(lineTxt);

                txtOutput.AppendLine(lineTxt);


                csvOutput.AppendLine(string.Join(",",
                    recordCounter.ToString(),
                    CsvEscape(recordType),
                    CsvEscape(description),
                    CsvEscape(label),
                    field.Length.ToString(),
                    field.IsRequired.ToString(),
                    CsvEscape(value),
                    CsvEscape(valid)
                ));
            }

            txtOutput.AppendLine();
        }

        if (outputFormat == "txt")
        {
            File.WriteAllText("output.txt", txtOutput.ToString());
            Console.WriteLine("Results saved to output.txt");
        }
        else if (outputFormat == "csv")
        {
            File.WriteAllText("output.csv", csvOutput.ToString());
            Console.WriteLine("Results saved to output.csv");
        }
    }

    private static void RunMedImpactFlow()
    {
        // Register supported MedImpact record types here
        var mediHandlers =
            new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase)
            {
                ["23"] = RunMedImpactType23,
                ["24"] = RunMedImpactType24,
                // Later: ["25"] = RunMedImpactType25,
            };

        // Optional: show "coming soon" for known but not implemented types
        var knownTypes =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["23"] = "Type 23 - MedImpact (Eligibility)"
                , ["24"] = "Type 24 - Member Attribute Load File",
                // ["25"] = "Type 25 - ???",
            };

        Console.WriteLine("\nMedImpact record types available:");
        foreach (var kvp in knownTypes)
        {
            var supported = mediHandlers.ContainsKey(kvp.Key)
                ? " (supported)"
                : " (coming soon)";
            Console.WriteLine($" - {kvp.Key}: {kvp.Value}{supported}");
        }

        Console.Write("\nSelect MedImpact record type (e.g., 23): ");
        var selected = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(selected))
        {
            Console.WriteLine("No selection provided.\n");
            return;
        }

        if (!mediHandlers.TryGetValue(selected, out var run))
        {
            Console.WriteLine($"\n[MedImpact Type {selected}] Coming soon!\n");
            return;
        }

        run();
    }


    private static string VisualizeSpaces(string line, char blankChar = '-')
    {
        return line.Replace(' ', blankChar);
    }

    private static void ProcessMedImpactFile(string filePath
        , RecordDefinition type23)
    {
        Console.Write("Would you like to save the results? (none/txt/csv): ");
        var outputFormat = Console.ReadLine()?.Trim().ToLowerInvariant();

        var txtOutput = new StringBuilder();
        var csvOutput = new StringBuilder();
        csvOutput.AppendLine(
            "RecordNumber,RecordType,RecordDescription,FieldName,Length,Required,Value,Valid");

        var lines = File.ReadAllLines(filePath);
        int recordCounter = 0;

        foreach (var rawLine in lines)
        {
            // preserve spaces inside the record; remove only line terminators
            var line = rawLine.TrimEnd('\r', '\n');

            if (line.Length < 2)
            {
                Console.WriteLine(
                    $"Line too short ({line.Length}) to determine record type.");
                continue;
            }

            var recordType = line.Substring(0, 2).Trim();

            if (recordType != "23")
            {
                Console.WriteLine(
                    $"Skipping unsupported MedImpact record type '{recordType}'.");
                continue;
            }

            // RecordLength in RecordDefinition is nullable int? in your model
            var expectedLength = type23.RecordLength ?? 0;
            if (expectedLength == 0)
            {
                Console.WriteLine(
                    "Type 23 definition has no RecordLength. Cannot validate file.");
                return;
            }

            if (line.Length != expectedLength)
            {
                Console.WriteLine(
                    $"Line has invalid length: {line.Length}. Expected: {expectedLength} for type 23");
                continue;
            }

            var description = type23.Description ?? "Type 23 - MedImpact";

            Console.WriteLine(
                $"\nRecord {++recordCounter} — Type: {recordType} — {description}");
            txtOutput.AppendLine(
                $"Record {recordCounter} — Type: {recordType} — {description}");

            var fields = type23.Fields ?? new List<FieldDefinition>();
            var sectionLengths = fields.Select(f => f.Length).ToList();
            var sections =
                StringDivider.DivideStringIntoVariableSections(line
                    , sectionLengths);

            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                var label = string.IsNullOrWhiteSpace(field.DisplayName)
                    ? field.Name
                    : field.DisplayName;
                var value = i < sections.Count ? sections[i] : "";
                var valid = field.Valid ?? "";

                var visibleValue = value.Replace(' ', '-');

                var lineTxt =
                    $"{label} ({field.Length}) {(field.IsRequired ? "REQUIRED" : "")}: {visibleValue}" +
                    $"{(string.IsNullOrWhiteSpace(valid) ? "" : $" ({valid})")}";

                Console.WriteLine(lineTxt);
                txtOutput.AppendLine(lineTxt);

                csvOutput.AppendLine(string.Join(",",
                    recordCounter.ToString(),
                    CsvEscape(recordType),
                    CsvEscape(description),
                    CsvEscape(label),
                    field.Length.ToString(),
                    field.IsRequired.ToString(),
                    CsvEscape(value),
                    CsvEscape(valid)
                ));
            }

            txtOutput.AppendLine();
        }

        if (outputFormat == "txt")
        {
            File.WriteAllText("output_medi.txt", txtOutput.ToString());
            Console.WriteLine("Results saved to output_medi.txt");
        }
        else if (outputFormat == "csv")
        {
            File.WriteAllText("output_medi.csv", csvOutput.ToString());
            Console.WriteLine("Results saved to output_medi.csv");
        }
    }

    private static void RunMedImpactType23()
    {
        var type23 = MedImpactDefinitionLoader.Load(
            Path.Combine("Definitions", "MedImpact", "type23.json"));

        var expectedLength = type23.RecordLength ?? 0;
        if (expectedLength == 0)
        {
            Console.WriteLine("Type 23 definition is missing RecordLength.");
            return;
        }

       var mode = ChooseMode();

        if (mode is "file" or "f")
        {
            Console.Write("Enter path to file: ");
            var filePath = Console.ReadLine()!;
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found.");
                return;
            }

            ProcessMedImpactFile(filePath, type23);
            return;
        }

        Console.WriteLine("Paste a single full record here:");
        var inputString = Console.ReadLine();
        if (string.IsNullOrEmpty(inputString))
        {
            Console.WriteLine("No input provided.");
            return;
        }

        var line = inputString.TrimEnd('\r', '\n').TrimStart('\uFEFF');

        if (line.Length < 2)
        {
            Console.WriteLine(
                $"Invalid length: {line.Length}. Minimum 2 required to read record type.");
            return;
        }

        var recordType = line.Substring(0, 2).Trim();
        if (recordType != "23")
        {
            Console.WriteLine(
                $"This flow supports Type 23 only. Found '{recordType}'.");
            return;
        }

        if (line.Length < expectedLength)
            line = line.PadRight(expectedLength, ' ');

        if (line.Length != expectedLength)
        {
            Console.WriteLine(
                $"Invalid length: {line.Length}. Expected: {expectedLength}");
            return;
        }

        var fields = type23.Fields ?? new List<FieldDefinition>();
        Console.WriteLine(
            $"\nRecord Type {recordType}: {type23.Description ?? "Type 23 - MedImpact"}");
        ProcessRecord(line, fields);

        Console.Write("Would you like to save the results? (none/txt/csv): ");
        var outputFormat = Console.ReadLine()?.Trim().ToLowerInvariant();

        if (outputFormat is "txt" or "csv")
            SaveSingleRecordOutput(line, recordType, fields, type23
                , outputFormat);
    }

    private static void RunMedImpactType24()
    {
        var type24Defs = MedImpactType24DefinitionLoader.Load(
            Path.Combine("Definitions", "MedImpact", "Type24.json"));

       var mode = ChooseMode();

        if (mode is "file" or "f")
        {
            Console.Write("Enter path to file: ");
            var filePath = Console.ReadLine()!;
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found.");
                return;
            }

            ProcessMedImpactType24File(filePath, type24Defs);
            return;
        }

        Console.WriteLine("Paste a single full record here:");
        var inputString = Console.ReadLine();
        if (string.IsNullOrEmpty(inputString))
        {
            Console.WriteLine("No input provided.");
            return;
        }

        var line = inputString.TrimEnd('\r', '\n').TrimStart('\uFEFF');

        if (line.Length < 4)
        {
            Console.WriteLine(
                $"Invalid length: {line.Length}. Minimum 4 required (RecordType+SegmentCode).");
            return;
        }

        var recordType = line.Substring(0, 2);
        var segmentCode = line.Substring(2, 2);

        if (recordType != "24")
        {
            Console.WriteLine($"Not a Type 24 record (found '{recordType}').");
            return;
        }

        if (!type24Defs.Segments.TryGetValue(segmentCode, out var segDef))
        {
            Console.WriteLine($"Unknown Type 24 Segment '{segmentCode}'.");
            return;
        }

        var expectedLength = segDef.RecordLength ?? 0;
        if (expectedLength == 0)
        {
            Console.WriteLine("Segment definition missing RecordLength.");
            return;
        }

        if (line.Length < expectedLength)
            line = line.PadRight(expectedLength, ' ');

        if (line.Length != expectedLength)
        {
            Console.WriteLine(
                $"Invalid length: {line.Length}. Expected: {expectedLength} for segment {segmentCode}");
            return;
        }

        var fields = segDef.Fields ?? new List<FieldDefinition>();
        Console.WriteLine(
            $"\nType 24 Segment {segmentCode}: {type24Defs.Description ?? "Type 24"}");
        ProcessRecord(line, fields);

        Console.Write("Would you like to save the results? (none/txt/csv): ");
        var outputFormat = Console.ReadLine()?.Trim().ToLowerInvariant();

        if (outputFormat is "txt" or "csv")
            SaveSingleRecordOutput(line, $"24-{segmentCode}", fields, segDef
                , outputFormat);
    }

    private static void ProcessMedImpactType24File(string filePath
        , MedImpactType24Definitions defs)


    {
        Console.Write("Would you like to save the results? (none/txt/csv): ");
        var outputFormat = Console.ReadLine()?.Trim().ToLowerInvariant();

        var txtOutput = new StringBuilder();
        var csvOutput = new StringBuilder();
        csvOutput.AppendLine(
            "RecordNumber,RecordType,SegmentCode,RecordDescription,FieldName,Length,Required,Value,Valid");

        var lines = File.ReadAllLines(filePath);
        int recordCounter = 0;
        int skippedTooShort = 0;
        int skippedWrongType = 0;
        int skippedUnknownSegment = 0;
        int skippedWrongLength = 0;

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd('\r', '\n');
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (line.Length < 4)
            {
                skippedTooShort++;
                continue;
            }

            var recordType = line.Substring(0, 2);
            if (recordType != "24")
            {
                skippedWrongType++;
                continue;
            }

            var segmentCode = line.Substring(2, 2);

            if (!defs.Segments.TryGetValue(segmentCode, out var segDef))
            {
                skippedUnknownSegment++;
                continue;
            }

            var expectedLength = segDef.RecordLength ?? 0;
            if (expectedLength == 0 || line.Length != expectedLength)
            {
                skippedWrongLength++;
                continue;
            }

            var fields = segDef.Fields ?? new List<FieldDefinition>();
            var description = defs.Description ??
                              "Type 24 - Member Attribute Load File";

            Console.WriteLine(
                $"\nRecord {++recordCounter} — Type: 24 — Segment: {segmentCode} — {description}");
            txtOutput.AppendLine(
                $"Record {recordCounter} — Type: 24 — Segment: {segmentCode} — {description}");

            var sectionLengths = fields.Select(f => f.Length).ToList();
            var sections =
                StringDivider.DivideStringIntoVariableSections(line
                    , sectionLengths);

            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                var label = string.IsNullOrWhiteSpace(field.DisplayName)
                    ? field.Name
                    : field.DisplayName;
                var value = i < sections.Count ? sections[i] : "";
                var valid = field.Valid ?? "";

                var visibleValue = value.Replace(' ', '-');

                var lineTxt =
                    $"{label} ({field.Length}) {(field.IsRequired ? "REQUIRED" : "")}: {visibleValue}" +
                    $"{(string.IsNullOrWhiteSpace(valid) ? "" : $" ({valid})")}";

                Console.WriteLine(lineTxt);
                txtOutput.AppendLine(lineTxt);

                csvOutput.AppendLine(string.Join(",",
                    recordCounter.ToString(),
                    CsvEscape(recordType),
                    CsvEscape(segmentCode),
                    CsvEscape(description),
                    CsvEscape(label),
                    field.Length.ToString(),
                    field.IsRequired.ToString(),
                    CsvEscape(value),
                    CsvEscape(valid)
                ));
            }

            txtOutput.AppendLine();
        }

        Console.WriteLine(
            $"\nSummary: parsed={recordCounter}, tooShort={skippedTooShort}, wrongType={skippedWrongType}, unknownSeg={skippedUnknownSegment}, wrongLen={skippedWrongLength}");

        if (outputFormat == "txt")
        {
            File.WriteAllText("output_medi_type24.txt", txtOutput.ToString());
            Console.WriteLine("Results saved to output_medi_type24.txt");
        }
        else if (outputFormat == "csv")
        {
            File.WriteAllText("output_medi_type24.csv", csvOutput.ToString());
            Console.WriteLine("Results saved to output_medi_type24.csv");
        }
    }

    private static string ChooseMode()
    {
        Console.Write("Single record or full file? (file/single): ");
        return Console.ReadLine()?.Trim().ToLowerInvariant() ?? "";
    }
}