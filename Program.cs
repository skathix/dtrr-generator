using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;
using FileIngestor.Tools;

//UPDATES on dashboard

namespace Tools;
//MBI was used after 1-1-2020 for new applications
class FileIngestor

{
    /*private static void Main(string[] args)
    {
        FileTypes.Load("FileTypes.json");
       Console.WriteLine("Available file types:");

        foreach (var kvp in FileTypes.fileTypes)
        {
            Console.WriteLine($" - {kvp.Value}");
        }
        //Read input
        var userFileTypeSelection = Console.ReadLine();
        
        var allDefinitions = MbitDefinitionLoader.LoadAll("Definitions");
        bool continueChecking = true;
        while (continueChecking)
        {
            Console.WriteLine("Available options:");
            foreach (var kvp in allDefinitions.Versions.OrderBy(v =>
                         v.Value.Start_Date))
            {
                string version = kvp.Key;
                DateTime startDate = kvp.Value.Start_Date;
                Console.WriteLine(
                    $" - {version} (starts: {startDate:yyyy-MM-dd})");
            }

        Console.Write("Enter the option (e.g., 18.2): ");
        string selectedVersion = Console.ReadLine()?.Trim();
//TODO: Add validation
            if (string.IsNullOrEmpty(selectedVersion))
            {
                Console.WriteLine("No input provided.");
                continue;
            
            }
        Console.Write(
            "Are you pasting a single record or a full file? (type 'file' or 'single'): ");
        string mode = Console.ReadLine()?.Trim().ToLower();
        if (mode == "file"||mode == "f")
        {
            Console.Write("Enter path to file: ");
            string filePath = Console.ReadLine()!;

            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found.");
                return;
            }

            ProcessFileWithOutputOption(filePath, selectedVersion!
                , allDefinitions);
        }
        else 
        {
            /*Console.WriteLine("Paste a single full record here:");
            string? inputString = Console.ReadLine();
            //TODO - Do we even need this, once we remove the possibility of end-of-record characters?
            var trimmedString = inputString.TrimStart();
            //TODO: Check for end-of-record characters, remove those
            //trimmedString will allow for an easier division without the
            //need to remove dashes
            var trimString = trimmedString.Replace(" ", "-");

            var expectedLength = allDefinitions.Versions[selectedVersion!]
                .Record_Length;
            if (trimString.Length != expectedLength)
            {
                Console.WriteLine(
                    $"Invalid length: {trimString.Length} (expected {expectedLength})");
                continue;
            }
            

            var recordType = trimString.Substring(59, 2);

            if (!allDefinitions.Versions.TryGetValue(selectedVersion
                    , out var versionDef))
                //TODO: Better Handling
                versionDef = new VersionDefinition();

            if (!versionDef.Record_Types.TryGetValue(recordType
                    , out var recordDef))
                //TODO: Better Handling
                recordDef = new RecordDefinition();
            var fields = recordDef.Fields;

           ProcessRecord(trimString, fields);#1#
            Console.WriteLine("Paste a single full record here:");
            string? inputString = Console.ReadLine();

// Basic null/empty check
            if (string.IsNullOrEmpty(inputString))
            {
                Console.WriteLine("No input provided.");
                // continue; or return;
            }

// Normalize: remove line terminators only (don't mutate spaces)
            var line = inputString
                .TrimEnd('\r', '\n')
                .TrimStart('\uFEFF'); // BOM, if any

// Lookup the selected version
            if (!allDefinitions.Versions.TryGetValue(selectedVersion!, out var versionDef))
            {
                Console.WriteLine($"Unknown version '{selectedVersion}'.");
                // continue; or return;
            }

// Guard: ensure we can read the transaction_code at 59,2 safely
            if (line.Length < 61) // need at least 61 chars to read start=59, len=2
            {
                Console.WriteLine($"Invalid length: {line.Length}. Minimum 61 required to read transaction code at position 59.");
                // continue; or return;
            }

// Extract record type (MBIT transaction code is at offset 59, length 2 per your spec)
            var recordType = line.Substring(59, 2).Trim();

// Lookup record definition for this type (if missing, we still can validate against version length)
            allDefinitions.Versions[selectedVersion!].Record_Types.TryGetValue(recordType, out var recordDef);

// Determine accepted lengths
            var acceptedLengths = GetAcceptedLengths(versionDef, recordDef);

// Validate length against accepted lengths
            if (!acceptedLengths.Contains(line.Length))
            {
                Console.WriteLine(
                    $"Invalid length: {line.Length}. Expected one of: {string.Join(", ", acceptedLengths)} for record type {recordType} (version default: {versionDef.Record_Length}).");
                // continue; or return;
            }



            Console.Write(
                "Would you like to save the results? (none/txt/csv): ");
            string outputFormat = Console.ReadLine()?.Trim().ToLower();
            //TODO: Finish validation here
            if (outputFormat is "txt" or "csv")
            {
                SaveSingleRecordOutput(line, recordType, fields
                    , recordDef, outputFormat);
            }
            /*else
            {
                Console.Write("Enty error ");
                continue;//correct response?
            }#1#
            
        }
        /*else
        {
            Console.Write("Enty error ");
            continue;//correct response?
        }#1#

        Console.Write("Do you want to check another item? (yes/no): ");
        string response = Console.ReadLine()!.ToLower();
        continueChecking = response == "yes" || response == "y";
        //TODO: Add validation/retry here
        /*if (response != "yes" || response != "y" || response != "no" ||
            response != "n")
        {
            Console.Write("Enty error");
            continue;//correct response?
        }#1#
    }

    Console.WriteLine("Goodbye!");
}*/


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
            var selectedFileType = Console.ReadLine()?.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(selectedFileType))
            {
                Console.WriteLine("No selection provided.\n");
                continue;
            }

            if (selectedFileType != "MBIT")
            {
                Console.WriteLine($"\n[{selectedFileType}] Coming soon! MBIT is currently supported.\n");
                continue; // loop back to file type selection
            }

            RunMbitFlow();

            Console.Write("\nDo you want to ingest another item? (y/n): ");
            var again = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (again is not ("y" or "yes")) break;
        }

        Console.WriteLine("Goodbye!");
    }

    private static void RunMbitFlow()
    {
        var allDefinitions = MbitDefinitionLoader.LoadAll("Definitions");

        Console.WriteLine("\nAvailable MBIT versions:");
        
        foreach (var kvp in allDefinitions.Versions.OrderBy(v => v.Value.StartDate))
        {
            string version = kvp.Key;
            DateTime startDate = kvp.Value.StartDate;
            Console.WriteLine($" - {version} (starts: {startDate:yyyy-MM-dd})");
        }


        Console.Write("Enter the version (e.g., 18.2): ");
        var selectedVersion = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(selectedVersion) ||
            !allDefinitions.Versions.TryGetValue(selectedVersion, out var versionDef))
        {
            Console.WriteLine($"Unknown or missing version '{selectedVersion}'.");
            return;
        }

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

            ProcessFileWithOutputOption(filePath, selectedVersion!, allDefinitions);
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

            var line = inputString.TrimEnd('\r', '\n').TrimStart('\uFEFF');

            if (line.Length < 61)
            {
                Console.WriteLine($"Invalid length: {line.Length}. Minimum 61 required to read transaction code at position 59.");
                return;
            }

            var recordType = line.Substring(59, 2).Trim();

            
            versionDef.RecordTypes.TryGetValue(recordType, out var recordDef);
            var acceptedLengths = GetAcceptedLengths(versionDef, recordDef);

// 1) pad for console paste mode
            line = PadToExpectedLength(line, acceptedLengths);

// 2) validate
            if (!acceptedLengths.Contains(line.Length))
            {
                Console.WriteLine($"Invalid length: {line.Length}. Expected: {string.Join(", ", acceptedLengths)}");
                return;
            }

// 3) parse using real line
            var fields = recordDef?.Fields ?? new List<FieldDefinition>();
            ProcessRecord(line, fields);


            Console.Write("Would you like to save the results? (none/txt/csv): ");
            var outputFormat = Console.ReadLine()?.Trim().ToLowerInvariant();

            if (outputFormat is "txt" or "csv")
                SaveSingleRecordOutput(line, recordType, fields, recordDef, outputFormat);
            else
                ProcessRecord(line, fields);
        }
    }

    private static IReadOnlyList<int> GetAcceptedLengths(VersionDefinition versionDef, RecordDefinition? recordDef)
    {
        if (recordDef?.AcceptedLengths is { Count: > 0 } list) return list;
        if (recordDef?.RecordLength is int rl) return new[] { rl };
        return new[] { versionDef.RecordLength };
    }

    private static void ProcessRecord(string input, List<FieldDefinition> fields)
    {
        var sectionLengths = fields.Select(f => f.Length).ToList();
        var sections = StringDivider.DivideStringIntoVariableSections(input, sectionLengths);

        Console.WriteLine("\nProcessed record Data:");
        for (int i = 0; i < Math.Min(sections.Count, fields.Count); i++)
        {
            var field = fields[i];
            var label = string.IsNullOrWhiteSpace(field.DisplayName) ? field.Name : field.DisplayName;
            var rawValue = sections[i];
            var requiredMark = field.IsRequired ? "REQUIRED" : "";
            var validValue = string.IsNullOrWhiteSpace(field.Valid) ? "" : $" ({field.Valid})";

            Console.WriteLine($"{label}({field.Length}) {requiredMark}: {rawValue}{validValue}");
        }
    }

    private static string CsvEscape(string s)
    {
        if (s.Contains('"')) s = s.Replace("\"", "\"\"");
        if (s.Contains(',') || s.Contains('\n') || s.Contains('\r') || s.Contains('"'))
            return $"\"{s}\"";
        return s;
    }

    private static void SaveSingleRecordOutput(string input, string recordType,
        List<FieldDefinition> fields, RecordDefinition? record, string outputFormat)
    {
        var sectionLengths = fields.Select(f => f.Length).ToList();
        var sections = StringDivider.DivideStringIntoVariableSections(input, sectionLengths);

        var txtOutput = new StringBuilder();
        var csvOutput = new StringBuilder();

        txtOutput.AppendLine(record?.Description ?? $"Record Type {recordType}");
        csvOutput.AppendLine("RecordNumber,FieldName,Length,Required,Value,Valid");

        for (int i = 0; i < fields.Count; i++)
        {
            var field = fields[i];
            var label = string.IsNullOrWhiteSpace(field.DisplayName) ? field.Name : field.DisplayName;
            var value = i < sections.Count ? sections[i] : "";
            var requiredMark = field.IsRequired ? "REQUIRED" : "";
            var valid = field.Valid ?? "";

            txtOutput.AppendLine($"{label} ({field.Length}) {requiredMark}: {value}{(string.IsNullOrWhiteSpace(valid) ? "" : $" ({valid})")}");

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

    private static void ProcessFileWithOutputOption(string filePath, string selectedVersion, MbitDefinitions allDefinitions)
    {
        Console.Write("Would you like to save the results? (none/txt/csv): ");
        var outputFormat = Console.ReadLine()?.Trim().ToLowerInvariant();

        var txtOutput = new StringBuilder();
        var csvOutput = new StringBuilder();
        csvOutput.AppendLine("RecordNumber,FieldName,Length,Required,Value,Valid");

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
                Console.WriteLine($"Line too short ({line.Length}) to determine record type.");
                continue;
            }

            var recordType = line.Substring(59, 2).Trim();
            versionDef.RecordTypes.TryGetValue(recordType, out var recordDef);

            var acceptedLengths = GetAcceptedLengths(versionDef, recordDef);
            if (!acceptedLengths.Contains(line.Length))
            {
                Console.WriteLine($"Line has invalid length: {line.Length}. Expected: {string.Join(", ", acceptedLengths)} for type {recordType}");
                continue;
            }

            var fields = recordDef?.Fields ?? new List<FieldDefinition>();
            var sectionLengths = fields.Select(f => f.Length).ToList();
            var sections = StringDivider.DivideStringIntoVariableSections(line, sectionLengths);

            Console.WriteLine($"\nRecord {++recordCounter} — Type: {recordType}");
            txtOutput.AppendLine($"Record {recordCounter} — {recordDef?.Description ?? $"Type {recordType}"}");

            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                var label = string.IsNullOrWhiteSpace(field.DisplayName) ? field.Name : field.DisplayName;
                var value = i < sections.Count ? sections[i] : "";
                var valid = field.Valid ?? "";
                var requiredMark = field.IsRequired ? "REQUIRED" : "";

                var lineTxt = $"{label} ({field.Length}) {requiredMark}: {value}{(string.IsNullOrWhiteSpace(valid) ? "" : $" ({valid})")}";
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
}

    /*private static bool TryGetFieldDefinitions(
        MbitDefinitions allDefinitions,
        string version,
        string recordType,
        out List<FieldDefinition> fields)
    {
        fields = new List<FieldDefinition>();
        return true;
    }*/

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
            string label = field.DisplayName ?? field.Name;
            string rawValue = sections[i];
            string requiredMark = field.IsRequired ? "REQUIRED" : "";
            string validValue = string.IsNullOrWhiteSpace(field.Valid) ? "" : $" ({field.Valid})";
            Console.WriteLine(
                $"{label}({field.Length}) {requiredMark}: {rawValue} {validValue}");
        }
    }

    private static void SaveSingleRecordOutput(string input, string recordType
        , List<FieldDefinition> fields, RecordDefinition record
        , string outputFormat)
    {
        var sectionLengths = fields.Select(f => f.Length).ToList();
        var sections =
            StringDivider.DivideStringIntoVariableSections(input
                , sectionLengths);

        var txtOutput = new StringBuilder();
        var csvOutput = new StringBuilder();
        csvOutput.AppendLine(record.Description);
        csvOutput.AppendLine(
            "RecordNumber,FieldName,Length,Required,Value,Valid");

        for (int i = 0; i < fields.Count; i++)
        {
            var field = fields[i];
            string label = field.DisplayName ?? field.Name;
            string value = sections[i];
            string requiredMark = field.IsRequired ? "REQUIRED" : "";
            string validValue = string.IsNullOrWhiteSpace(field.Valid) ? "" : $" ({field.Valid})";

            string lineTxt =
                $"{label} ({field.Length}) {requiredMark}: {value} {validValue}";
            txtOutput.AppendLine(lineTxt);
            csvOutput.AppendLine(
                $"1,{label},{field.Length},{field.IsRequired},{value},{validValue}");
        }

        if (outputFormat == "txt")
        {
            File.WriteAllText("output_single.txt", txtOutput.ToString());
            Console.WriteLine("Results saved to output_single.txt");
        }
        else if (outputFormat == "csv")
        {
           //Figure out why the validation information isn't going into the csv
           
            File.WriteAllText("output_single.csv", csvOutput.ToString());
            Console.WriteLine("Results saved to output_single.csv");
        }
    }

    private static void ProcessFileWithOutputOption(string filePath
        , string selectedVersion, MbitDefinitions allDefinitions)
    {
        Console.Write("Would you like to save the results? (none/txt/csv): ");
        string outputFormat = Console.ReadLine()?.Trim().ToLower();
        //TODO: Again, validate entry

        var txtOutput = new StringBuilder();
        var csvOutput = new StringBuilder();
        csvOutput.AppendLine("RecordNumber,FieldName,Length,Required,Value");

        var expectedLength =
            allDefinitions.Versions[selectedVersion].Record_Length;
        var lines = File.ReadAllLines(filePath);
        int recordCounter = 0;

        foreach (string line in lines)
        {
            string trimmed = line.TrimStart().Replace(" ", "-");

            if (trimmed.StartsWith("AAAAAAHEADER"))
            {
                Console.WriteLine("Skipping header record.");
                continue;
            }

            if (trimmed.Length != expectedLength)
            {
                Console.WriteLine(
                    $"Line has invalid length: {trimmed.Length} (expected {expectedLength})");
                continue;
            }

            var recordType = trimmed.Substring(59, 2);

            Console.WriteLine(
                $"\nRecord {++recordCounter} — Type: {recordType}");
            if (!allDefinitions.Versions.TryGetValue(selectedVersion
                    , out var versionDef))
                //TODO: Better Handling
                //TODO: Add in saved record/field:Value string creation
                versionDef = new VersionDefinition();

            if (!versionDef.Record_Types.TryGetValue(recordType
                    , out var recordDef))
                //TODO: Better Handling
                recordDef = new RecordDefinition();
            var fields = recordDef.Fields;

            var sectionLengths = fields.Select(f => f.Length).ToList();
            var sections =
                StringDivider.DivideStringIntoVariableSections(trimmed
                    , sectionLengths);

            txtOutput.AppendLine(
                $"Record {recordCounter} — {recordDef.Description}");
            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                string label = field.DisplayName ?? field.Name;
                string value = sections[i];
                string requiredMark = field.IsRequired ? "REQUIRED" : "";
                string validValue = string.IsNullOrWhiteSpace(field.Valid) ? "" : $" ({field.Valid})";

                string lineTxt =
                    $"{label} ({field.Length}) {requiredMark}: {value} {validValue}";
                Console.WriteLine(lineTxt);
                // Don't need to do both here, instead collect all the information then in the switch
                // on outputFormat
                txtOutput.AppendLine(lineTxt);

                csvOutput.AppendLine(
                    $"{recordCounter},{label},{field.Length},{field.IsRequired},{value}");
            }

            /*
             * if (output == db)
             * {
             *
             * }
             */
            txtOutput.AppendLine(); // spacing between records
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
        else if (outputFormat == "db")
        {

        }
        
    }

    /*
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
                var versionDef =
                    JsonSerializer.Deserialize<VersionDefinition>(json
                        , jsonOptions);

                if (versionDef != null)
                    definitions.Versions[version] = versionDef;
            }

            return definitions;
        }
    }*/
    
    /*public static class FileTypes
    {
        // Holds your in-memory file types after loading JSON
        public static Dictionary<string, string> fileTypes = new();

        public static void Load(string jsonPath)
        {
            if (!File.Exists(jsonPath))
                throw new FileNotFoundException("FileTypes.json not found.", jsonPath);

            var json = File.ReadAllText(jsonPath);

            var loaded = JsonSerializer.Deserialize<Dictionary<string, string>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            fileTypes = loaded ?? new Dictionary<string, string>();
        }
    }
    public class RecordDefinition
    {
        [JsonPropertyName("record_length")]
        public int? Record_Length { get; set; } // optional per-type override

        [JsonPropertyName("accepted_lengths")]
        public List<int>? Accepted_Lengths { get; set; } // optional multi-lengths

        public List<FieldDefinition> Fields { get; set; } = new();
        public string Description { get; set; } = "";
        
        [JsonPropertyName("record_length")]
        public int? Record_Length { get; set; } // optional per-type override

        [JsonPropertyName("accepted_lengths")]
        public List<int>? Accepted_Lengths { get; set; } // optional multi-lengths

        public List<FieldDefinition> Fields { get; set; } = new();
        public string Description { get; set; } = "";*/

    }
    
    
    private static IReadOnlyList<int> GetAcceptedLengths(VersionDefinition versionDef, RecordDefinition? recordDef)
    {
        if (recordDef?.AcceptedLengths is { Count: > 0 } list) return list;
        if (recordDef?.RecordLength is int rl) return new[] { rl };
        return new[] { versionDef.DefaultRecordLength };
    }


