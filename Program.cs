using System.Text.Json;
using System.Text;
using Tools.Tools;

namespace Tools;
//MBI was used after 1-1-2020 for new applications
class MbitTool
{
    private static void Main(string[] args)
    {
        var allDefinitions = MbitDefinitionLoader.LoadAll("Definitions");

        bool continueChecking = true;

        while (continueChecking)
        {
            Console.WriteLine("Available versions:");
            foreach (var kvp in allDefinitions.Versions.OrderBy(v => v.Value.Start_Date))
            {
                string version = kvp.Key;
                DateTime startDate = kvp.Value.Start_Date;
                Console.WriteLine($" - {version} (starts: {startDate:yyyy-MM-dd})");
            }

            Console.Write("Enter the version number (e.g., 18.2): ");
            string selectedVersion = Console.ReadLine()?.Trim();

            Console.Write("Are you pasting a single record or a full file? (type 'file' or 'single'): ");
            string mode = Console.ReadLine()?.Trim().ToLower();

            if (mode == "file")
            {
                Console.Write("Enter path to file: ");
                string filePath = Console.ReadLine()!;

                if (!File.Exists(filePath))
                {
                    Console.WriteLine("File not found.");
                    return;
                }

                ProcessFileWithOutputOption(filePath, selectedVersion!, allDefinitions);
            }
            else
            {
                Console.WriteLine("Paste a single full MBIT here:");
                string inputString = Console.ReadLine()!;
                var trimString = inputString.TrimStart().Replace(" ", "-");

                var expectedLength = allDefinitions.Versions[selectedVersion!].Record_Length;
                if (trimString.Length != expectedLength)
                {
                    Console.WriteLine($"Invalid length: {trimString.Length} (expected {expectedLength})");
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
                
                //
                // if (!TryGetFieldDefinitions(allDefinitions, selectedVersion, recordType, out var fields))
                // {
                //     Console.WriteLine($"No definition found for record type {recordType} in version {selectedVersion}.");
                //     continue;
                // }

                ProcessRecord(trimString, fields);

                Console.Write("Would you like to save the results? (none/txt/csv): ");
                string outputFormat = Console.ReadLine()?.Trim().ToLower();
                if (outputFormat is "txt" or "csv")
                {
                    SaveSingleRecordOutput(trimString, recordType, fields, recordDef, outputFormat);
                }
            }

            Console.Write("Do you want to check another item? (yes/no): ");
            string response = Console.ReadLine()!.ToLower();
            continueChecking = response == "yes" || response == "y";
        }

        Console.WriteLine("Goodbye!");
    }

    private static bool TryGetFieldDefinitions(
        MbitDefinitions allDefinitions,
        string version,
        string recordType,
        out List<FieldDefinition> fields)
    {
        fields = new List<FieldDefinition>();
        //TODO: Check into this function, may be deletable or at least refactortable, unused at the moment
        return true;
    }

    private static void ProcessRecord(string input, List<FieldDefinition> fields)
    {
        var sectionLengths = fields.Select(f => f.Length).ToList();
        var sections = StringDivider.DivideStringIntoVariableSections(input, sectionLengths);

        Console.WriteLine("\nProcessed MBIT Data:");

        for (int i = 0; i < Math.Min(sections.Count, fields.Count); i++)
        {
            var field = fields[i];
            string label = field.DisplayName ?? field.Name;
            string rawValue = sections[i];
            string requiredMark = field.IsRequired ? "REQUIRED" : "";
            Console.WriteLine($"{label}({field.Length}){requiredMark}: {rawValue}");
        }
    }

    private static void SaveSingleRecordOutput(string input, string recordType, List<FieldDefinition> fields, RecordDefinition record, string outputFormat)
    {
        var sectionLengths = fields.Select(f => f.Length).ToList();
        var sections = StringDivider.DivideStringIntoVariableSections(input, sectionLengths);

        var txtOutput = new StringBuilder();
        var csvOutput = new StringBuilder();
        csvOutput.AppendLine(record.Description);
        csvOutput.AppendLine("RecordNumber,FieldName,Length,Required,Value");

        for (int i = 0; i < fields.Count; i++)
        {
            var field = fields[i];
            string label = field.DisplayName ?? field.Name;
            string value = sections[i];
            string requiredMark = field.IsRequired ? "REQUIRED" : "";

            string lineTxt = $"{label} ({field.Length}){requiredMark}: {value}";
            txtOutput.AppendLine(lineTxt);
            csvOutput.AppendLine($"1,{label},{field.Length},{field.IsRequired},{value}");
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
        string outputFormat = Console.ReadLine()?.Trim().ToLower();

        var txtOutput = new StringBuilder();
        var csvOutput = new StringBuilder();
        csvOutput.AppendLine("RecordNumber,FieldName,Length,Required,Value");

        var expectedLength = allDefinitions.Versions[selectedVersion].Record_Length;
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
                Console.WriteLine($"Line has invalid length: {trimmed.Length} (expected {expectedLength})");
                continue;
            }

            var recordType = trimmed.Substring(59, 2);
           
            Console.WriteLine($"\nRecord {++recordCounter} — Type: {recordType}");
            if (!allDefinitions.Versions.TryGetValue(selectedVersion
                    , out var versionDef))
                //TODO: Better Handling
                versionDef = new VersionDefinition();

            if (!versionDef.Record_Types.TryGetValue(recordType
                    , out var recordDef))
                //TODO: Better Handling
                recordDef = new RecordDefinition();
            var fields = recordDef.Fields;
            
            // if (!TryGetFieldDefinitions(allDefinitions, selectedVersion, recordType, out var fields))
            // {
            //     Console.WriteLine($"No definition found for record type {recordType}.");
            //     continue;
            // }

            var sectionLengths = fields.Select(f => f.Length).ToList();
            var sections = StringDivider.DivideStringIntoVariableSections(trimmed, sectionLengths);
            
            txtOutput.AppendLine($"Record {recordCounter} — {recordDef.Description}");
            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                string label = field.DisplayName ?? field.Name;
                string value = sections[i];
                string requiredMark = field.IsRequired ? "REQUIRED" : "";
                string lineTxt = $"{label} ({field.Length}){requiredMark}: {value}";
                Console.WriteLine(lineTxt);
                // Don't need to do both here, instead collect all the information then in the switch
                // on outputFormat
                txtOutput.AppendLine(lineTxt);

                csvOutput.AppendLine($"{recordCounter},{label},{field.Length},{field.IsRequired},{value}");
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
}
