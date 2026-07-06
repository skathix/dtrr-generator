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
    private enum MbitViewMode
    {
        FileVerification = 1
        , DataEntry = 2
    }

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
            Console.WriteLine("Available file types:");
            foreach (var kvp in FileTypes.Types)
            {
                var supported = handlers.ContainsKey(kvp.Key)
                    ? " (supported)"
                    : " (coming soon)";

                Console.WriteLine($" - {kvp.Key}: {kvp.Value}{supported}");
            }


            Console.Write("Select file type (key, e.g. MBIT): ");

            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("No selection provided.\n");
                continue;
            }


            var selectedFileType = input.ToUpperInvariant();

// Check it's a known key from FileTypes.json FIRST (better message)
            if (!FileTypes.Types.ContainsKey(selectedFileType))
            {
                Console.WriteLine($"Unknown file type '{selectedFileType}'.\n");
                continue;
            }

// Then check whether it is supported by your code
            if (!handlers.TryGetValue(selectedFileType, out var run))
            {
                Console.WriteLine($"\n[{selectedFileType}] Coming soon!\n");
                continue;
            }

            //var viewMode = ChooseMbitViewMode();
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

        var selectedVersion =
            ChooseMbitVersion(allDefinitions, preferredDefault: "19.3");
        var versionDef = allDefinitions.Versions[selectedVersion];
        var viewMode = ChooseMbitViewMode();

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

            Console.Write(
                "Would you like to save the results? (none/txt/csv): ");
            var outputFormat = Console.ReadLine()?.Trim().ToLowerInvariant();

            // Always show field-by-field (your primary use case)
            ProcessMbitRecord(line, fields, viewMode);

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

    private static void ProcessMbitRecord(string input
        , List<FieldDefinition> fields,
        MbitViewMode viewMode)
    {
        var sectionLengths = fields.Select(f => f.Length).ToList();
        var sections =
            StringDivider.DivideStringIntoVariableSections(input
                , sectionLengths);

        Console.WriteLine($"\nProcessed record Data ({viewMode}):");
        if (viewMode == MbitViewMode.DataEntry)
        {
            
        }

        var displayFields = viewMode == MbitViewMode.FileVerification
            ? fields
            : GetFieldsForDataEntry(fields);

        foreach (var field in displayFields)
        {
            var index = fields.IndexOf(field);
            if (index < 0 || index >= sections.Count)
                continue;

            var rawValue = sections[index];
            var visibleValue = rawValue.Replace(' ', '-');

            var label = string.IsNullOrWhiteSpace(field.DisplayName)
                ? field.Name
                : field.DisplayName;

            var requiredMark = field.IsRequired ? "REQUIRED" : "";
            var validValue = string.IsNullOrWhiteSpace(field.Valid)
                ? ""
                : $" ({field.Valid})";

            Console.WriteLine(
                $"{label} ({field.Length}) {requiredMark}: {visibleValue}{validValue}");
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


    private static List<FieldDefinition> GetFieldsForDataEntry(
        List<FieldDefinition> fields)
    {
        return fields
            .OrderBy(f => GetEntryPriority(f))
            .ThenBy(f => f.DisplayName)
            .ToList();
    }

    private static MbitViewMode ChooseMbitViewMode()
    {
        Console.WriteLine("\nSelect output view:");
        Console.WriteLine("  1. File verification (PCUG order)");
        Console.WriteLine("  2. Data entry order");

        Console.Write("Choose 1 or 2 (Enter = 1): ");
        var input = Console.ReadLine()?.Trim();

        return input == "2"
            ? MbitViewMode.DataEntry
            : MbitViewMode.FileVerification;
    }

    private static readonly Dictionary<string, int>
        EntryOrder =
            new(StringComparer.OrdinalIgnoreCase)

            {
                ["beneficiary_id"] = 1, ["surname"] = 2, ["first_name"] = 3
                , ["middle_initial"] = 4, ["gender"] = 5, ["birthdate"] = 6
                , ["contract_number"] = 8, ["disability"] = 11
                , ["esrd_override"] = 14, ["transaction_type_code"] = 16
                , ["effective_date"] = 18, ["pbp_number"] = 20
                , ["transactionDate"] = 21, ["rolled_contract"] = 25
                , ["rolled_pbp"] = 27, ["application_date"] = 28
                , ["segment_id"] = 31, ["part_c_premium"] = 32
                , ["election_type_code"] = 34, ["enrollment_source"] = 35
                , ["part_d_opt_out"] = 36, ["premium_withhold"] = 37
                , ["credible_coverage_flag"] = 39, ["employer_override"] = 40
                , ["uncovered_months"] = 25, ["secondary_insurance"] = 44
                , ["secondary_rx_id"] = 45, ["secondary_rx_group"] = 46
                , ["eghp_flag"] = 47, ["part_d_bin"] = 55, ["part_d_pcn"] = 56
                , ["part_d_group"] = 57, ["part_d_id"] = 58
                , ["secondary_bin"] = 59, ["secondary_pcn"] = 60
                , ["disenrollment_reason"] = 68, ["mmp_opt_out"] = 69
                , ["tracking_id"] = 84, ["relationshipagent"] = 87
                , ["relationshipbroker"] = 88, ["relationshipship"] = 89
                , ["relationshipar"] = 90, ["relationshipother"] = 91
                , ["relationshipself"] = 92, ["relationshipblank"] = 93
                , ["npnnumber"] = 94, ["oecflag"] = 95
                , ["oecapplicationdate"] = 96, ["oecapplicationnumber"] = 97
                , ["memberphonenumber"] = 98, ["memberemail"] = 99
                , ["electiontype"] = 100, ["preferred_language"] = 101
                , ["accessible_format"] = 102,
            };

    private static int GetEntryPriority(FieldDefinition field)
    {
        foreach (var kvp in EntryOrder)
        {
            if (field.Name.Contains(kvp.Key,
                    StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Value;
            }
        }


        Console.WriteLine(
            $"No priority defined for: {field.Name}");

        return 999;
    }

    private record FixedDisplayField(
        int Priority
        , string Label
        , string Value);

    private static readonly List<FixedDisplayField> FixedFields =
        new List<FixedDisplayField>
        {
            new(7, "RecordType", "T")
            , new(9, "StateCode", "Reference StateCode")
            , new(10, "CountyCode", " Last 3 digits of CountyCode")
            , new(12,"Hospice Indicator","blank")
            , new(13, "InstitutionalNHCHCBSIndicator", "blank")
            , new(14, "TransactionReplyCode", "From ticket")
            , new(16, "EntitlementTypeCode", "Y")
            , new(18, "WorkingAgeIndicator", "0")
            , new(21, "UIInitiatedChangeFlag", "0")
            , new(22, "VariableData", "from ticket for 51s")
            , new(23, "DistrictOfficeCode", "blank")
            , new(26, "Source ID", "Contract Number")
            , new(29, "UIUserOrganizationDesignation", "blank")
            , new(30, "OutOfAreaFlag", "N")
            , new(33, "PartDBeneficiaryPremium", "PremiumPayment table")
            , new(38, "CumulativeNoOfUncoveredMonths", "0")
            , new(41, "ProcessingTimeStamp", " Non Midnight hh.mm.ss.ssssss")
            , new(42,"End Date","blank")
            , new(43, "Submitted Covered Months", "0")
            , new(48, "PartDLowIncomePremiumSubsidyLvl", "reference LIS")
            , new(49, "LowIncomeCopayCategory", "reference LIS")
            , new(50, "Low Income Effective Date","From LIS")
            , new(51,"PtD LateEnrlPenalty Amount","0.00")
            , new(52, "PtDLateEnrlWaivedyAmt", "0.00")
            , new(53, "PtDLateEnrlSubsidyAmt", "0.00")
            , new(54, "LowIncomePtDPremiumSubsidyAmt", "reference LIS")
            , new(61, "DeMinimisDifferentialAmt", "0.00")
            , new(62, "MSPStatusFlg", "Blank")
            , new(63, "LowIncomePeriodEndDate", "reference LIS")
            , new(64, "LowIncomeSubsidySourceCode", "reference LIS")
            , new(65, "EnrolleeTypeFlagPBPLevel", "Blank")
            , new(66, "ApplicationDateIndicator", "Blank")
            , new(67, "TRCShortName", "reference PCUG")
            , new(70, "CleanupId", "Blank")
            , new(71, "POSDrugEditUpdateDeleteFlag", "Blank")
            , new(72, "POSDrugEditStatus", "Blank")
            , new(73, "POSDrugEditClass", "Blank")
            , new(74, "POSDrugEditCode", "Blank")
            , new(75, "NotificationDate", "Blank")
            , new(76, "ImplementationDate", "Blank")
            , new(77, "TerminationDate", "Blank")
            , new(78, "HospiceProviderNumber", "Blank")
            , new(79, "ICModelTypeIndicator", "Blank")
            , new(80, "ICModelEndDateReasonCode", "Blank")
            , new(81, "ICModelbenefitStatus", "Blank")
            , new(82, "UpdtMedicaidStatForCoummunityRAFBenef", "Blank")
            , new(83, "SystemsAssignedTrackingNumber"
                , "000000000mmddyy<last 4 of plan assigned tracking number>")
            , new(85, "LoadDate", "reference DTRR SQL")
            , new(86, "Status", "Unprocessed")
            , new(103,"CARA Implementation End Date", "blank")
            , new(104,"Prescriber Limitation", "blank")
            , new(105,"Pharmacy Limitation", "blank")
        };


    private static void RunMedImpactFlow()
    {
        // Register supported MedImpact record types here
        var mediHandlers =
            new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase)
            {
                ["23"] = RunMedImpactType23, ["24"] = RunMedImpactType24
                , ["112"] = RunMedImpactType112,
                // Later: ["25"] = RunMedImpactType25,
            };

        // Optional: show "coming soon" for known but not implemented types
        var knownTypes =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["23"] = "Type 23 - MedImpact (Eligibility)"
                , ["24"] = "Type 24 - Member Attribute Load File"
                , ["112"] = "Type 112 - Outbound Claims Detail Extract"
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

    private static void RunMedImpactType112()
    {
        var type112 = MedImpactDefinitionLoader.Load(
            Path.Combine("Definitions", "MedImpact", "type112.json"));

        var expectedLength = type112.RecordLength ?? 0;
        if (expectedLength == 0)
        {
            Console.WriteLine("Type 112 definition is missing RecordLength.");
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

            ProcessMedImpactFile(filePath, type112);
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
        if (recordType != "112")
        {
            Console.WriteLine(
                $"This flow supports Type 112 only. Found '{recordType}'.");
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

        var fields = type112.Fields ?? new List<FieldDefinition>();
        Console.WriteLine(
            $"\nRecord Type {recordType}: {type112.Description ?? "Type 112 - MedImpact"}");
        ProcessRecord(line, fields);

        Console.Write("Would you like to save the results? (none/txt/csv): ");
        var outputFormat = Console.ReadLine()?.Trim().ToLowerInvariant();

        if (outputFormat is "txt" or "csv")
            SaveSingleRecordOutput(line, recordType, fields, type112
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

    private static void ProcessMedImpactType112File(string filePath
        , MedImpactType112Definitions defs)


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
            if (recordType != "112")
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
                              "Type 112 - Outbound Claims Detail Extract File";

            Console.WriteLine(
                $"\nRecord {++recordCounter} — Type: 112 — Segment: {segmentCode} — {description}");
            txtOutput.AppendLine(
                $"Record {recordCounter} — Type: 112 — Segment: {segmentCode} — {description}");

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
            File.WriteAllText("output_medi_type112.txt", txtOutput.ToString());
            Console.WriteLine("Results saved to output_medi_type112.txt");
        }
        else if (outputFormat == "csv")
        {
            File.WriteAllText("output_medi_type112.csv", csvOutput.ToString());
            Console.WriteLine("Results saved to output_medi_type112.csv");
        }
    }

    private static string ChooseMode()
    {
        Console.Write("Single record or full file? (file/single): ");
        return Console.ReadLine()?.Trim().ToLowerInvariant() ?? "";
    }
}