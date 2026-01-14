using System.Text.Json;
using System.Text;
using Tools.Tools;

//UPDATES on dashboard

namespace Tools;
//MBI was used after 1-1-2020 for new applications
class FileIngestor

{
    private static void Main(string[] args)
    {
        
        // This is the starting point
        // Create a file called "FileIngestor.cs" In the Models folder in SuperiorSelect
        // public class FileIngestor
        // {
        //      public FileIngestor() 
        //      {
        //      }
        //
        //      This is the Main(string[] args) function
        //      public Execute()
        //      {
        //          var allDefinitions...
        //          ...
        //          Console.WriteLine("Goodbye!");
        //      }
        
        // 1-14-26
        // Add here a general type of file:
        // MBIT, DTRR, DTRR Type P, Type 23(can add future file types)
        // Then the string divider can be updated
        
        // Console.WriteLine("Available file types:");
        // foreach (var kvp in FileTypes)
        // {
        //     string version = kvp.Key;
        //     Console.WriteLine(
        //         $" - {type}");
        // }
        
        
        
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

            Console.Write("Enter the option (e.g., 18.2, ReplayCodeNames): ");
            string selectedVersion = Console.ReadLine()?.Trim();
//TODO: Add validation
            /*if (selectedVersion != null) //need to figure this is correct
            {
               Console.Write("Enty error ");
               continue;
                
            }*/
            Console.Write(
                "Are you pasting a single record or a full file? (type 'file' or 'single'): ");
            string mode = Console.ReadLine()?.Trim().ToLower();
//TODO: Finish Validation
            if (mode == "file")
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
                Console.WriteLine("Paste a single full record here:");
                string inputString = Console.ReadLine();
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

               ProcessRecord(trimString, fields);

                Console.Write(
                    "Would you like to save the results? (none/txt/csv): ");
                string outputFormat = Console.ReadLine()?.Trim().ToLower();
                //TODO: Finish validation here
                if (outputFormat is "txt" or "csv")
                {
                    SaveSingleRecordOutput(trimString, recordType, fields
                        , recordDef, outputFormat);
                }
                /*else
                {
                    Console.Write("Enty error ");
                    continue;//correct response?
                }*/
                
            }
            /*else
            {
                Console.Write("Enty error ");
                continue;//correct response?
            }*/

            Console.Write("Do you want to check another item? (yes/no): ");
            string response = Console.ReadLine()!.ToLower();
            continueChecking = response == "yes" || response == "y";
            //TODO: Add validation/retry here
            /*if (response != "yes" || response != "y" || response != "no" ||
                response != "n")
            {
                Console.Write("Enty error");
                continue;//correct response?
            }*/
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
    }
}

