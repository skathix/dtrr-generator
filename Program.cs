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
        FileTypes.Load("FileTypes.json");

        while (true)
        {
            Console.WriteLine("Available file types:");
            foreach (var kvp in FileTypes.Types)
                Console.WriteLine($" - {kvp.Key}: {kvp.Value}");

            Console.Write("Select file type (key, e.g. MBIT): ");
            var selectedFileType =
                Console.ReadLine()?.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(selectedFileType))
            {
                Console.WriteLine("No selection provided.\n");
                continue;
            }

            if (selectedFileType != "MBIT")
            {
                Console.WriteLine(
                    $"\n[{selectedFileType}] Coming soon! MBIT is currently supported.\n");
                continue;
            }

            RunMbitFlow();

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

        Console.Write("Single record or full file? (file/single): ");
        var mode = Console.ReadLine()?.Trim().ToLowerInvariant();

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

            var fields = recordDef?.Fields ?? new List<FieldDefinition>();

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

        txtOutput.AppendLine(record?.Description ??
                             $"Record Type {recordType}");
        csvOutput.AppendLine(
            "RecordNumber,FieldName,Length,Required,Value,Valid");

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
            "RecordNumber,FieldName,Length,Required,Value,Valid");

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
                $"\nRecord {++recordCounter} — Type: {recordType}");
            txtOutput.AppendLine(
                $"Record {recordCounter} — {recordDef?.Description ?? $"Type {recordType}"}");

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

    private static string PadToExpectedLength(string line
        , IReadOnlyList<int> acceptedLengths)
    {
        // choose the smallest accepted length that can contain the line
        var target = acceptedLengths.OrderBy(x => x)
            .FirstOrDefault(x => x >= line.Length);

        // if no target found, return as-is (will fail validation later)
        if (target == 0) return line;

        // pad with spaces to preserve fixed-width indexing
        return line.PadRight(target, ' ');
    }


    private static string VisualizeSpaces(string line, char blankChar = '-')
    {
        return line.Replace(' ', blankChar);
    }
}