using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Tools.Validation;

public class FieldValidations
{
    private void MbiCheck(string beneficiary_id)
    {
        //TODO Third and sixth characters may be numeric or one of the letters
        var regex = @"^\d[ACDEFGHJKMNPQRTUVWXY]\d\d[ACDEFGHJKMNPQRTUVWXY]\d\d[ACDEFGHJKMNPQRTUVWXY]{2}\d$";
        var match = Regex.Match(beneficiary_id, regex
            , RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            Console.WriteLine(
                "MBI format: you would get a TCR 007 - Reject for this");
        }

        Console.WriteLine(
            "POTENTIAL MBI not found: you would get a TCR 008 - Reject for this");
    }
    private void NamesCheck(string LastName, string FirstName, string MI)
    {
        var surnameRegex = @"^[A-Za-z][a-zA-z\\s]{11}$";
        var firstNameRegex = @"^[A-Za-z][A-Za-z]{6}$";
        var miRegex = @"^[a-zA-z\\s]+$";

        var surnamematch = Regex.Match(LastName, surnameRegex
            , RegexOptions.IgnoreCase);
        if (!surnamematch.Success)
        {
            Console.WriteLine(
                "Required item - you will get a reject for this record if 3 of 4 don't match");
        }


        var firstmatch = Regex.Match(FirstName, firstNameRegex
            , RegexOptions.IgnoreCase);
        if (!firstmatch.Success)
        {
            Console.WriteLine(
                "Required item - you will get a reject for this record if 3 of 4 don't match");
        }

        var mimatch = Regex.Match(MI, miRegex
            , RegexOptions.IgnoreCase);
        if (!mimatch.Success)
        {
            Console.WriteLine(
                "Required item - you will get a reject for this record if 3 of 4 don't match");
        }
    }
    void SexCheck(string sexCode)
    {
        switch (sexCode)
        {
            case "1":
                Console.WriteLine("Male");
                break;
            case "2":
                Console.WriteLine("Female");
                break;
            default:
                Console.WriteLine(
                    "This will be set to 0: not fail nor rejection");
                break;
        }
    }
    private void ValidateBirthDate(string date)
    {
        DateValidation(date);

        var yearText = date.Substring(0, 4);
        var year = int.Parse(yearText);
        var day = date.Substring(6, 2);
        if (year < 1870)
        {
            Console.WriteLine(
                "This is a date before 1870 - will be rejected");
        }

        if (year >= (DateTime.Now.Year) + 1)
        {
            Console.WriteLine(
                "This is a date too far in the future - will be rejected");
        }
    }
    void recordTypeHardcode(string recordType)
    {
        var HcRecordType = ("T");
    }
    private void contract(string contractNumber)
    {
        var regex = @"^[A-Za-z]\d\d\d\d$";
        var match = Regex.Match(contractNumber, regex
            , RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            Console.WriteLine(
                "Contract Number format: you would get a TCR 007 - Reject for this");
        }
    }
    private void NumericStateCode(string StateCode)
    {
        var regex = @"^\d\d$";
        var match = Regex.Match(StateCode, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "State Code: Sent by CMS");
        }
    }
    private void NumericCountyCode(string CountyCode)
    {
        var regex = @"^\d\d\d$";
        var match = Regex.Match(CountyCode, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "County Code: Sent by CMS");
        }
    }
    private void DisabilityCode(int DisabilityIndicator)
    {
        switch (DisabilityIndicator)
        {
            case 0:
                Console.WriteLine("No Disability");
                break;
            case 1:
                Console.WriteLine("Disabled without ERSD");
                break;
            case 2:
                Console.WriteLine("ESRD Only");
                break;
            case 3:
                Console.WriteLine("Disabled with ESRD");
                break;
            default:
                Console.WriteLine("NA");
                break;
        }
    }
    void hospiceCheck(string hospice)
    {
        switch (hospice)
        {
            case "0":
                Console.WriteLine("No Hospice");
                break;
            case "1":
                Console.WriteLine("Hospice");
                break;
            default:
                Console.WriteLine("Not applicable");
                break;
        }
    }
    private void InstitutionalCode(int institutionalIndicator)
    {
        switch (institutionalIndicator)
        {
            case 0:
                Console.WriteLine("No Institutional");
                break;
            case 1:
                Console.WriteLine("Institutional");
                break;
            case 2:
                Console.WriteLine("NHC");
                break;
            case 3:
                Console.WriteLine("HCBS");
                break;
            default:
                Console.WriteLine("NA");
                break;
        }
    }
    void esrdCheck(string esrd)
    {
        switch (esrd)
        {
            case "0":
                Console.WriteLine("No End-Stage Renal Disease");
                break;
            case "1":
                Console.WriteLine("End Stage Renal Disease");
                break;
            default:
                Console.WriteLine("Not applicable");
                break;
        }
    }
    void TransactionReply(string transactionReply, string trcShortName)
    {
        switch (transactionReply)
        {
            //This, and a few other ones, are not returned on the DTRR: but are certainly problems
            //Add yet another file type to parse? Something else?
            case "001":
                if (trcShortName != "BAD TRANS CODE")
                {
                    Console.WriteLine("Error for 001");
                }
                break;
                case "002":
                    if (trcShortName != "BAD ACTION CODE")
                {
                    Console.WriteLine("Error for 002");
                }
                break;
            case "003":
                if (trcShortName != "BAD CONTRACT #")
                {
                    Console.WriteLine("Error for 003");
                }
                break;
            case "004":
                if (trcShortName != "NEED MEMB NAME")
                {
                    Console.WriteLine("Error for 004");
                }
                break;
            case "006":
                if (trcShortName != "BAD BIRTH DATE")
                {
                    Console.WriteLine("Error for 006");
                }
                break;
            case "007":
                if (trcShortName != "BAD BENE ID FORMAT")
                {
                    Console.WriteLine("Error for 007");
                }
                break;
            case "008":
                if (trcShortName != "BENE ID NOT FOUND")
                {
                    Console.WriteLine("Error for 008");
                }
                break;
            case "009":
                if (trcShortName != "NO BENE MATCH")
                {
                    Console.WriteLine("Error for 009");
                }
                break;
            case "011":
                if (trcShortName != "ENROLL ACCEPTED")
                {
                    Console.WriteLine("Error for 011");
                }
                break;
            case "013":
                if (trcShortName != "DISENROL ACCEPT")
                {
                    Console.WriteLine("Error for 013");
                }
                break;
            case "014":
                if (trcShortName != "DISNROL-NEW MCO")
                {
                    Console.WriteLine("Error for 014");
                }
                break;
            case "015":
                if (trcShortName != "ENROLL REMOVED")
                {
                    Console.WriteLine("Error for 015");
                }
                break;
            case "016":
                if (trcShortName != "ENROLL-OUT AREA")
                {
                    Console.WriteLine("Error for 016");
                }
                break;
            case "017":
                if (trcShortName != "ENROLL-BAD SCC")
                {
                    Console.WriteLine("Error for 017");
                }
                break;
            case "018":
                if (trcShortName != "AUTO DISENROLL")
                {
                    Console.WriteLine("Error for 018");
                }
                break;
            case "019":
                if (trcShortName != "NO ENROLL-NO AB")
                {
                    Console.WriteLine("Error for 019");
                }
                break;
            case "020":
                if (trcShortName != "NO ENROLL-NOT55")
                {
                    Console.WriteLine("Error for 020");
                }
                break;
            case "022":
                if (trcShortName != "NEW BENE ID")
                {
                    Console.WriteLine("Error for 022");
                }
                break;
            case "023":
                if (trcShortName != "NEW NAME")
                {
                    Console.WriteLine("Error for 023");
                }
                break;
            case "025":
                if (trcShortName != "DISROL-NEW MBI")
                {
                    Console.WriteLine("Error for 025");
                }
                break;
            case "026":
                if (trcShortName != "DISROL-NEW NAME")
                {
                    Console.WriteLine("Error for 026");
                }
                break;
            case "032":
                if (trcShortName != "MEMB HAS NO B")
                {
                    Console.WriteLine("Error for 032");
                }
                break;
            case "033":
                if (trcShortName != "MEMB HAS NO A")
                {
                    Console.WriteLine("Error for 033");
                }
                break;
            case "034":
                if (trcShortName != "MEMB NOT AGE 65")
                {
                    Console.WriteLine("Error for 034");
                }
                break;
            case "035":
                if (trcShortName != "MEMB IN HOSPICE")
                {
                    Console.WriteLine("Error for 035");
                }
                break;
            case "036":
                if (trcShortName != "MEMB DECEASED")
                {
                    Console.WriteLine("Error for 036");
                }
                break;
            case "037":
                if (trcShortName != "BAD ENROLL DATE")
                {
                    Console.WriteLine("Error for 037");
                }
                break;
            case "038":
                if (trcShortName != "DUPLICATE")
                {
                    Console.WriteLine("Error for 038");
                }
                break;
            case "039":
                if (trcShortName != "ALREADY ENROLL")
                {
                    Console.WriteLine("Error for 039");
                }
                break;
            case "042":
                if (trcShortName != "ENROLL BLOCKED")
                {
                    Console.WriteLine("Error for 042");
                }
                break;
            case "044":
                if (trcShortName != "NO CONTRACT")
                {
                    Console.WriteLine("Error for 044");
                }
                break;
            case "045":
                if (trcShortName != "MEMB HAS ESRD")
                {
                    Console.WriteLine("Error for 045");
                }
                break;
            //048 applies to effective dates prior to 1/1/2008
            case "050":
                if (trcShortName != "NOT ENROLLED")
                {
                    Console.WriteLine("Error for 050");
                }
                break;
            case "051":
                if (trcShortName != "BAD DISENR DATE")
                {
                    Console.WriteLine("Error for 051");
                }
                break;
            case "052":
                if (trcShortName != "DUPLICATE")
                {
                    Console.WriteLine("Error for 052");
                }
                break;
            case "054":
                if (trcShortName != "RETRO DISN DATE")
                {
                    Console.WriteLine("Error for 054");
                }
                break;
            case "055":
                if (trcShortName != "ESRD CANCELED")
                {
                    Console.WriteLine("Error for 055");
                }
                break;
            case "056":
                if (trcShortName != "FAILS DEMO REQ")
                {
                    Console.WriteLine("Error for 056");
                }
                break;
            case "060":
                if (trcShortName != "NOT ENROLLED")
                {
                    Console.WriteLine("Error for 060");
                }
                break;
            //062 applies only to effective dates prior to 1/1/2008
            case "071":
                if (trcShortName != "HOSPICE ON")
                {
                    Console.WriteLine("Error for 071");
                }
                break;
            case "072":
                if (trcShortName != "HOSPICE OFF")
                {
                    Console.WriteLine("Error for 072");
                }
                break;
            case "073":
                if (trcShortName != "ESRD ON")
                {
                    Console.WriteLine("Error for 073");
                }
                break;
            case "074":
                if (trcShortName != "ESRD OFF")
                {
                    Console.WriteLine("Error for 074");
                }
                break;
            //075 applies only to effective dates prior to 1/1/2008   
            case "077":
                if (trcShortName != "MEDICAID ON")
                {
                    Console.WriteLine("Error for 077");
                }
                break;
            case "078":
                if (trcShortName != "MEDICAID OFF")
                {
                    Console.WriteLine("Error for 074");
                }
                break;
            case "079":
                if (trcShortName != "MEDICARE A OFF")
                {
                    Console.WriteLine("Error for 079");
                }
                break;
            case "080":
                if (trcShortName != "MEDICARE A ON")
                {
                    Console.WriteLine("Error for 080");
                }
                break;
            case "081":
                if (trcShortName != "MEDICARE B OFF")
                {
                    Console.WriteLine("Error for 081");
                }
                break;
            case "082":
                if (trcShortName != "MEDICARE B ON")
                {
                    Console.WriteLine("Error for 082");
                }
                break;
            case "085":
                if (trcShortName != "NEW SCC")
                {
                    Console.WriteLine("Error for 085");
                }
                break;
            case "086":
                if (trcShortName != "NEW MBI")
                {
                    Console.WriteLine("Error for 086");
                }
                break;
            case "087":
                if (trcShortName != "NEW NAME")
                {
                    Console.WriteLine("Error for 087");
                }
                break;
            case "088":
                if (trcShortName != "NEW SEX CODE")
                {
                    Console.WriteLine("Error for 088");
                }
                break;
            case "089":
                if (trcShortName != "NEW BIRTH DATE")
                {
                    Console.WriteLine("Error for 089");
                }
                break;
            case "090":
                if (trcShortName != "MEMB DECEASED")
                {
                    Console.WriteLine("Error for 090");
                }
                break;
            case "091":
                if (trcShortName != "DEATH DATE OFF")
                {
                    Console.WriteLine("Error for 091");
                }
                break;
            case "092":
                if (trcShortName != "NEW DEATH DATE")
                {
                    Console.WriteLine("Error for 092");
                }
                break;
            case "099":
                if (trcShortName != "MCAID CHANGE")
                {
                    Console.WriteLine("Error for 099");
                }
                break;
            case "100":
                if (trcShortName != "PBP CHANGE OK")
                {
                    Console.WriteLine("Error for 100");
                }
                break;
            case "102":
                if (trcShortName != "BAD APP DATE")
                {
                    Console.WriteLine("Error for 102");
                }
                break;
            case "103":
                if (trcShortName != "NO A/B ENT")
                {
                    Console.WriteLine("Error for 103");
                }
                break;
            case "104":
                if (trcShortName != "BAD ELECT TYPE")
                {
                    Console.WriteLine("Error for 104");
                }
                break;
            case "105":
                if (trcShortName != "BAD ELECT DATE")
                {
                    Console.WriteLine("Error for 105");
                }
                break;
            case "106":
                if (trcShortName != "LATER APPLIC")
                {
                    Console.WriteLine("Error for 106");
                }
                break;
            case "107":
                if (trcShortName != "BAD PBP NUMBER")
                {
                    Console.WriteLine("Error for 107");
                }
                break;
            case "108":
                if (trcShortName != "NO MORE ELECTS")
                {
                    Console.WriteLine("Error for 108");
                }
                break;
            case "109":
                if (trcShortName != "ALREADY ENROLL")
                {
                    Console.WriteLine("Error for 109");
                }
                break;
            case "110":
                if (trcShortName != "NO PART A/EGHP")
                {
                    Console.WriteLine("Error for 110");
                }
                break;
            case "114":
                if (trcShortName != "RX NOT AEP/OEPI")
                {
                    Console.WriteLine("Error for 114");
                }
                break;
            case "116":
                if (trcShortName != "BAD SEGMENT NUM")
                {
                    Console.WriteLine("Error for 116");
                }
                break;
            case "117":
                if (trcShortName != "FBD AUTO ENROLL")
                {
                    Console.WriteLine("Error for 117");
                }
                break;
            case "118":
                if (trcShortName != "LIS FAC ENROLL")
                {
                    Console.WriteLine("Error for 118");
                }
                break;
            case "121":
                if (trcShortName != "LIS UPDATE")
                {
                    Console.WriteLine("Error for 121");
                }
                break;
            case "127":
                if (trcShortName != "EMP SUB REJ")
                {
                    Console.WriteLine("Error for 127");
                }
                break;
            case "128":
                if (trcShortName != "EMP SUB OVR REJ")
                {
                    Console.WriteLine("Error for 128");
                }
                break;
            case "129":
                if (trcShortName != "EMP SUB ACC")
                {
                    Console.WriteLine("Error for 129");
                }
                break;
            case "130":
                if (trcShortName != "BAD OPT OUT CD")
                {
                    Console.WriteLine("Error for 130");
                }
                break;
            case "131":
                if (trcShortName != "OPT OUT OK")
                {
                    Console.WriteLine("Error for 131");
                }
                break;
            case "133":
                if (trcShortName != "BAD 2 INS FLAG")
                {
                    Console.WriteLine("Error for 133");
                }
                break;
            case "134":
                if (trcShortName != "NO 2 INS INFO")
                {
                    Console.WriteLine("Error for 134");
                }
                break;
            case "135":
                if (trcShortName != "DIALYSIS START")
                {
                    Console.WriteLine("Error for 135");
                }
                break;
            case "136":
                if (trcShortName != "DIALYSIS END")
                {
                    Console.WriteLine("Error for 136");
                }
                break;
            case "137":
                if (trcShortName != "TRANSPLANT ADD")
                {
                    Console.WriteLine("Error for 137");
                }
                break;
            case "138":
                if (trcShortName != "ADDR NOT U.S.")
                {
                    Console.WriteLine("Error for 138");
                }
                break;
            case "139":
                if (trcShortName != "EGHP FLAG CHG")
                {
                    Console.WriteLine("Error for 139");
                }
                break;
            case "143":
                if (trcShortName != "4RX SCD INS CHG")
                {
                    Console.WriteLine("Error for 143");
                }
                break;
            case "150":
                if (trcShortName != "OVER CAP LIMIT")
                {
                    Console.WriteLine("Error for 150");
                }
                break;
            case "152":
                if (trcShortName != "NEW RACE CODE")
                {
                    Console.WriteLine("Error for 152");
                }
                break;
            case "154":
                if (trcShortName != "OUT OF AREA")
                {
                    Console.WriteLine("Error for 154");
                }
                break;
            case "155":
                if (trcShortName != "INCARCERATED")
                {
                    Console.WriteLine("Error for 155");
                }
                break;
            case "156":
                if (trcShortName != "BAD USR FOR PLAN")
                {
                    Console.WriteLine("Error for 156");
                }
                break;
            case "157":
                if (trcShortName != "UNAUT REQUEST")
                {
                    Console.WriteLine("Error for 157");
                }
                break;
            case "158":
                if (trcShortName != "INST CHANGE")
                {
                    Console.WriteLine("Error for 158");
                }
                break;
            case "159":
                if (trcShortName != "NHC CHANGE")
                {
                    Console.WriteLine("Error for 159");
                }
                break;
            case "161":
                if (trcShortName != "MBD ALERT")
                {
                    Console.WriteLine("Error for 161");
                }
                break;
            
            
            
            
            
            default:
                Console.WriteLine("Bad Transaction Reply Code");
                break;
        }
    }
    }
    void SentTransactionCode(string transactionCode)
    {
        var regex = @"^\d\d$";
        var match = Regex.Match(transactionCode, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Non-numeric Transaction Code");
        }

        switch (transactionCode)
        {
            case "51":
                Console.WriteLine("Disenrollment Transaction (51)");
                break;
            case "54":
                Console.WriteLine("Disenrollment Transaction (54)");
                break;
            case "61":
                Console.WriteLine("Enrollment Transaction (61)");
                break;
            case "72":
                Console.WriteLine("4Rx Data Change (72)");
                break;
            case "76":
                Console.WriteLine("Residence Address Change (76)");
                break;
            case "79":
                Console.WriteLine("Part D Opt-Out (79)");
                break;
            case "80":
                Console.WriteLine("Cancellation of Enrollment (80)");
                break;
            case "81":
                Console.WriteLine("Cancellation of Disenrollment (81)");
                break;
            case "82":
                Console.WriteLine("MMP Enrollment Cancellation (82)");
                break;
            case "83":
                Console.WriteLine("MMP Opt-Out Update (83)");
                break;
            case "90":
                Console.WriteLine("CARA Status (90)");
                break;
            case "91":
                Console.WriteLine("IC Model Participation (91)");
                break;
            case "92":
                Console.WriteLine("Personal Information Change (92)");
                break;
            case "93":
                Console.WriteLine("Enrollment SEP Reason Code Correction (93)");
                break;
            case "94":
                Console.WriteLine(
                    "Disenrollment SEP Reason Code Correction (94)");
                break;
            case "95":
                Console.WriteLine("Medicare Prescription Payment Plan (95)");
                break;

            default:
                Console.WriteLine("Invalid transaction code");
                break;
        }
    }
    void EntitlementCheck(string entitlementCheck)
    {
        var regex = @"^\[YZ/s]$";
        var match = Regex.Match(entitlementCheck, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Transaction Code");
        }
    }
    void EffectiveDateCheck(string effectiveDateCheck, string transactionReply)
    {
        DateValidation(effectiveDateCheck);

        switch (transactionReply)
        {
            case "071":
                Console.WriteLine("Effective date of the hospice period");
                break;
            case "072":
                goto case "071";
            case "090":
                Console.WriteLine("Current Calendar Month");
                break;
            case "091":
                Console.WriteLine("Previously Reported incorrect death date");
                break;
            case "121":
                Console.WriteLine("PBP Effective Enrollment");
                break;
            case "194":
                goto case "121";
            case "223":
                goto case "121";
            case "245":
                Console.WriteLine("Payments impacted by MSP date");
                break;
            case "280":
                goto case "245";
            //this needs additional verification
            case "293":
                Console.WriteLine(
                    "Enrollment End Date - last day of the month ");
                break;
            case "305":
                Console.WriteLine("New ZIP Code Start Date");
                break;
            case "345":
                Console.WriteLine("Effective date of attempted enrollment");
                break;
            case "346":
                Console.WriteLine("End date of the enrollment period");
                break;
            case "347":
                Console.WriteLine("Start date of the reenrollment period");
                break;
            case "366":
                Console.WriteLine(
                    "Effective date for change of Medicaid status");
                break;
            case "368":
                Console.WriteLine(
                    "Plans payments impacted date - based on MSP date");
                break;
            case "409":
                Console.WriteLine("Effective date for M3P");
                break;
            case "410":
                goto case "409";
            case "411":
                goto case "409";
            case "412":
                goto case "409";
            case "413":
                goto case "409";
            case "414":
                goto case "409";
            case "415":
                goto case "409";
            case "416":
                goto case "409";
            case "417":
                goto case "409";
            case "701":
                Console.WriteLine("New Enrollment period start date");
                break;
            case "702":
                Console.WriteLine("Fill-in enrollment period start date");
                break;
            case "703":
                Console.WriteLine("Start date of cancelled enrollment period");
                break;
            case "704":
                Console.WriteLine(
                    "Start date of enrollment cancelled for PBP correction");
                break;
            case "705":
                Console.WriteLine(
                    "Start date of enrollment for PBP correction");
                break;
            case "706":
                Console.WriteLine(
                    "Start date of enrollment cancelled for segment correction");
                break;
            case "707":
                Console.WriteLine(
                    "Start date of enrollment for segment correction");
                break;
            case "708":
                Console.WriteLine(
                    "Enrollment period end date assigned to opened ended enrollment");
                break;
            case "709":
                Console.WriteLine("New start date resulting from update");
                break;
            case "710":
                goto case "709";
            case "711":
                Console.WriteLine("New end date resulting from update");
                break;
            case "712":
                goto case "711";
            case "713":
                Console.WriteLine("End date removed");
                break;
            default:
                Console.WriteLine("Not applicable");
                break;
        }
    }
    void WaHardcode(string WorkingAgeIndicator)
    {
        var workingAgeCheck = ("1");
    }
    void PbpIdCode(string PlanBenefitPackageId)
    {
        var regex = @"^\d\d\d$";
        var match = Regex.Match(PlanBenefitPackageId, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid PBP Number");
        }
    }
    void filler(string Filler, int numWhiteSpaces)
    {
        FillerValidation(Filler, numWhiteSpaces);
    }
    void TransactionDate(string transactionDate, string transactionReply)
    {
        DateValidation(transactionDate);

        switch (transactionReply)
        {
            case "121":
                goto case "223";
            case "072":
                goto case "223";
            case "223":
                Console.WriteLine("Date report was generated");
                break;
            default:
                Console.WriteLine("Date of Transaction");
                break;
        }
    }
    void UiInitiatedChangeCode(string UIInitiatedChangeFlag)
    {
        var regex = @"^\d$";
        var match = Regex.Match(UIInitiatedChangeFlag, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid PBP Number");
        }

        switch (UIInitiatedChangeFlag)
        {
            case "0":
                Console.WriteLine("Transaction not created in user interface");
                break;
            case "1":
                Console.WriteLine("Transaction created in user interface");
                break;
            default:
                Console.WriteLine("Not applicable");
                break;
        }
    }
    void todoVariableData(string VariableData)
    {
        var regex = @"^\d$";
        var match = Regex.Match(VariableData, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "WTF?!");
        }

        switch (VariableData)
        {
            default:
                Console.WriteLine("Not applicable");
                break;
        }
    }
    void districtOfficeCode(string DistrictOfficeCode)
    {
        var regex = @"^\s\s\s$";
        var match = Regex.Match(DistrictOfficeCode, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid unless the transaction is a 53");
        }
    }
    void prevPartDContractAndPBP(string PrevPartDContractAndPBP)
    {
        var empty = @"^[/s{8}$";
        var regex = @"^[A-Za-z]\d\d\d\d\d\d\d$";
        var emptyMatch = Regex.Match(PrevPartDContractAndPBP, empty
            , RegexOptions.IgnoreCase);
        if (!emptyMatch.Success)
        {
            var match = Regex.Match(PrevPartDContractAndPBP, regex
                , RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                Console.WriteLine(
                    "Contract Number/PBP format error");
            }
        }
    }
    void SepReasonCode(string sepReasonCode, string electionType
        , string transactionReply)
    {
        var type1 = "S";
        var type2 = "Y";
        if (electionType != type1)
        {
            if (electionType != type2)
            {
                Console.WriteLine("This string should be blank.");
            }
        }

        var sepRegex = @"^[a-zA-Z0-9]{2}$";
        var match = Regex.Match(sepReasonCode, sepRegex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid SEP entry");
        }

        switch (transactionReply)
        {
            case "011":
                goto case "725";
            case "013":
                goto case "725";
            case "015":
                goto case "725";
            case "018":
                goto case "725";
            case "022":
                goto case "725";
            case "023":
                goto case "725";
            case "025":
                goto case "725";
            case "026":
                goto case "725";
            case "100":
                goto case "725";
            case "397":
                goto case "725";
            case "401":
                goto case "725";
            case "402":
                goto case "725";
            case "701":
                goto case "725";
            case "702":
                goto case "725";
            case "704":
                goto case "725";
            case "705":
                goto case "725";
            case "708":
                goto case "725";
            case "709":
                goto case "725";
            case "710":
                goto case "725";
            case "711":
                goto case "725";
            case "712":
                goto case "725";
            case "713":
                goto case "725";
            case "717":
                goto case "725";

            case "725":
                Console.WriteLine("Should not be blank");
                break;
            default:
                Console.WriteLine("Should be blank");
                break;
        }
    }
    void DtrrFiller1(string dtrrFiller1, int numWhiteSpace)
    {
        FillerValidation(dtrrFiller1, numWhiteSpace);
    }
    private void SourceId(string sourceId)
    {
        var regex = @"^[A-Za-z]\d\d\d\d$";
        var match = Regex.Match(sourceId, regex
            , RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            Console.WriteLine(
                "Source Id Error: should match contract number");
        }
    }
    void PriorPlanBenefitPackageId(string priorPlanBenefitPackageId)
    {
        var regex = @"^\d\d\d$";
        var match = Regex.Match(priorPlanBenefitPackageId, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid PBP Number");
        }
    }
    void ApplicationDate(string applicationDate)
    {
        DateValidation(applicationDate);
    }
    void UIUserOrganizationDesignation(string uiUserOrganizationDesignation)
    {
        var regex = @"^\s{2}$";
        var match = Regex.Match(uiUserOrganizationDesignation, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid - should be blanks");
        }
    }
    void OutOfAreaFlag(string outOfAreaFlag)
    {
        var regex = @"^\[NY/s]$";
        var match = Regex.Match(outOfAreaFlag, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Out of Area Flag");
        }
    }
    void SegmentNumber(string segmentNumber)
    {
        var regex = @"^\s{3}$";
        var match = Regex.Match(segmentNumber, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid - should be blanks");
        }
    }
    void PartCBeneficiaryPremium(string partCBeneficiaryPremium)
    {
        CurrencyValidation(partCBeneficiaryPremium);
    }
    void PartDBeneficiaryPremium(string partDBeneficiaryPremium)
    {
        CurrencyValidation(partDBeneficiaryPremium);
    }
    void ElectionTypeCode(string electionTypeCode)
    {
        var regex = @"^\[ACDFIJLMNORSTUVWXYZ/s]$";
        var match = Regex.Match(electionTypeCode, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Election type code");
        }
    }
    void EnrollmentSourceCode(string enrollmentSourceCode)
    {
        var regex = @"^\[ABCDEFGHIJKLN/s]$";
        var match = Regex.Match(enrollmentSourceCode, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid enrollment Source Code");
        }
    }
    void PartDOptOutFlag(string partDOptOutFlag)
    {
        var regex = @"^\[YN/s]$";
        var match = Regex.Match(partDOptOutFlag, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Part D OptOut Flag");
        }
    }
    void PtsCAndDPremiumWithholdOpt(string ptsCAndDPremiumWithholdOpt)
    {
        var regex = @"^\[DNRS/s]$";
        var match = Regex.Match(ptsCAndDPremiumWithholdOpt, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Pts C And D Premium Withhold Opt - 120, 185, and 186 report the PPO involved in the communication, all other report the PPO in effect");
        }
    }
    void CumulativeNoOfUncoverdMonths(string cumulativeNoOfUncoverdMonths)
    {
        cumulativeNoOfUncoverdMonths = "000";
    }
    void CreditableCoverageFlag(string creditableCoverageFlag)
    {
        var regex = @"^\[YNALRTU/s]$";
        var match = Regex.Match(creditableCoverageFlag, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Creditable Coverage Flag");
        }
    }
    void EmployerSubsidyOverrideFlag(string employerSubsidyOverrideFlag)
    {
        var regex = @"^\[Y/s]$";
        var match = Regex.Match(employerSubsidyOverrideFlag, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Employer Subsidy Override Flag");
        }
    }
    void ProcessingTimestamp(string processingTimestamp)
    {
        var regex = @"\b\d{2}\.\d{2}\.\d{2}.\d{6}\b";
        var match = Regex.Match(processingTimestamp, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid timestamp");
        }
    }
    void EndDate(string endDate)
    {
        DateValidation(endDate);
    }
    void SubmittedNoOfUncoveredMonths(string submittedNoOfUncoveredMonths)
    {
        var regex = @"^\d\d\d$";
        var match = Regex.Match(submittedNoOfUncoveredMonths, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid SubmittedNoOfUncoveredMonths");
        }
    }
    void SecondaryDrugInsuranceFlag(string secondaryDrugInsuranceFlag)
    {
        var regex = @"^\[YN/s]$";
        var match = Regex.Match(secondaryDrugInsuranceFlag, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Secondary Drug Insurance Flag");
        }
    }
    void SecondaryRxId(string secondaryRxId)
    {
        var regex = @"^/s{20}$";
        var match = Regex.Match(secondaryRxId, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Secondary Rx Id");
        }
    }
    void SecondaryRxGroup(string secondaryRxGroup)
    {
        var regex = @"^/s{15}$";
        var match = Regex.Match(secondaryRxGroup, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Secondary Rx Group");
        }
    }
    void Egph(string egph, int transactionCode)
    {
        if (transactionCode == 61)
        {
            var regex61 = @"^\[Y/s]$";
            var match61 = Regex.Match(egph, regex61);
            if (!match61.Success)
            {
                Console.WriteLine(
                    "Invalid Enrollment EGPH");
            }
        }
        else if (transactionCode == 74)
        {
            var regex74 = @"^\[YN/s]$";
            var match74 = Regex.Match(egph, regex74);
            if (!match74.Success)
            {
                Console.WriteLine(
                    "Invalid EGPH change value");
            }
        }
        else
        {
            var regexAll = @"^/s$";
            var matchAll = Regex.Match(egph, regexAll);
            if (!matchAll.Success)
            {
                Console.WriteLine(
                    "Invalid -should be blank");
            }
        }
    }
    void PartDLowIncomePremSubsidyLvl(string partDLowIncomePremSubsidyLvl)
    {
        var regex = @"^\d\d\d$";
        var match = Regex.Match(partDLowIncomePremSubsidyLvl, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Part D Low Income Prem Subsidy Lvl");
        }

        switch (partDLowIncomePremSubsidyLvl)
        {
            case "000":
                Console.WriteLine("No subsidy");
                break;
            case "025":
                Console.WriteLine("25% subsidy");
                break;
            case "050":
                Console.WriteLine("50% subsidy");
                break;
            case "075":
                Console.WriteLine("75% subsidy");
                break;
            case "100":
                Console.WriteLine("100% subsidy");
                break;
            default:
                Console.WriteLine("NA");
                break;
        }
    }
    void LowIncomeCopayCategory(string lowIncomeCopayCategory)
    {
        var regex = @"^\d$";
        var match = Regex.Match(lowIncomeCopayCategory, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Low Income Copay Category");
        }

        switch (lowIncomeCopayCategory)
        {
            case "0":
                Console.WriteLine("Not low-income");
                break;
            case "1":
                Console.WriteLine("High");
                break;
            case "2":
                Console.WriteLine("Low");
                break;
            case "3":
                Console.WriteLine("0");
                break;
            case "4":
                Console.WriteLine("15%");
                break;
            case "5":
                Console.WriteLine("Unknown");
                break;
            default:
                Console.WriteLine("NA");
                break;
        }
    }
    void LowIncomePeriodEffDate(string lowIncomePeriodEffDate)
    {
        DateValidation(lowIncomePeriodEffDate);
    }
    void PtDLateEnrlPenaltyAmt(string ptDLateEnrlPenaltyAmt)
    {
        CurrencyValidation(ptDLateEnrlPenaltyAmt);
    }
    void PtDLateEnrlPenaltyWaivedAmt(string ptDLateEnrlPenaltyWaivedAmt)
    {
        CurrencyValidation(ptDLateEnrlPenaltyWaivedAmt);
    }
    void PtDLateEnrlPenaltySubsidyAmt(string ptDLateEnrlPenaltySubsidyAmt)
    {
        CurrencyValidation(ptDLateEnrlPenaltySubsidyAmt);
    }
    void LowIncomePtDPremiumSubsidyAmt(string lowIncomePtDPremiumSubsidyAmt)
    {
        CurrencyValidation(lowIncomePtDPremiumSubsidyAmt);
    }
    void PartDRxBIN(string partDRxBin, string transactionCode, int numWhiteSpace)
    {
        if (transactionCode = "61" || transactionCode = "72")
        {
            partDRxBin.Length = numWhiteSpace;
        }
        else
        {
            FillerValidation(partDRxBin, numWhiteSpace);
        }
    }
    void PartDRxPCN(string partDRxPcn, string transactionCode, int numWhiteSpace)
    {
        if (transactionCode = "61" || transactionCode = "72")
        {
            partDRxPcn.Length = numWhiteSpace;
        }
        else
        {
            FillerValidation(partDRxPcn, numWhiteSpace);
        }
    }
    void PartDRxGroup(string partDRxGroup, string transactionCode, int numWhiteSpace)
    {
        if (transactionCode = "61" || transactionCode = "72")
        {
            partDRxGroup.Length = numWhiteSpace;
        }
        else
        {
            FillerValidation(partDRxGroup, numWhiteSpace);
        }
    }
    void PartDRxId(string partDRxId, string transactionCode, int numWhiteSpace)
    {
        if (transactionCode = "61" || transactionCode = "72")
        {
            partDRxId.Length = numWhiteSpace;
        }
        else
        {
            FillerValidation(partDRxId, numWhiteSpace);
        }
    }
    void SecondaryRxBIN(string secondaryRxBIN, string transactionCode, int numWhiteSpace)
    {
        if (transactionCode = "61" || transactionCode = "72")
        {
            secondaryRxBIN.Length = numWhiteSpace;
        }
        else
        {
            FillerValidation(secondaryRxBIN, numWhiteSpace);
        }
    }
    void SecondaryRxPCN(string secondaryRxPCN, string transactionCode, int numWhiteSpace)
    {
        if (transactionCode = "61" || transactionCode = "72")
        {
            secondaryRxPCN.Length = numWhiteSpace;
        }
        else
        {
            FillerValidation(secondaryRxPCN, numWhiteSpace);
        }
    }
    void DeMinimisDifferentialAmt(string deMinimisDifferentialAmt)
    {
        CurrencyValidation(deMinimisDifferentialAmt);
    }
    void MSPStatusFlag(string MmspStatusFlag)
    {
        var regex = @"^\[PSN/s]$";
        var match = Regex.Match(MmspStatusFlag, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Medicare secondary payer status flag");
        }
    }
    void LowIncomePeriodEndDate(string lowIncomePeriodEndDate)
    {
        DateValidation(lowIncomePeriodEndDate);
    }
    void LowIncomeSubsidySrceCode(string lowIncomeSubsidySrceCode)
    {
        var regex = @"^\[AD/s]$";
        var match = Regex.Match(lowIncomeSubsidySrceCode, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Low Income Subsidy Source Code");
        }
    }
    void EnrolleeTypeFlagPBPLevel(string enrolleeTypeFlagPBPLevel)
    {
        var regex = @"^\[CPY/s]$";
        var match = Regex.Match(enrolleeTypeFlagPBPLevel, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Enrollee Type Flag, PBP Level");
        }
    }
    void ApplicationDateIndicator(string applicationDateIndicator)
    {
        var regex = @"^\[Y/s]$";
        var match = Regex.Match(applicationDateIndicator, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Application Date Indicator");
        }
    }
    



    static void DateValidation(string date)
    {
        if (DateTime.TryParse(date, out var convertedDate))
        {
            Console.WriteLine(
                "This is a valid date");
            return;
        }

        Console.WriteLine("Invalid date");
    }

    static void CurrencyValidation(string premium)
    {
        var regex = @"(?:^\b(\d+\.\d{2})\b$)|(?:^\s{7}$)";
        var match = Regex.Match(premium, regex);
        if (!match.Success)
        {
            //\b\d+\.\d{2}\b|(?:^\s{7}$)
            //\b\d{2}\.\d{2}\.\d{2}.\d{6}\b -- Timestamp

            Console.WriteLine(("Bad premium amount"));
        }
    }

    static void FillerValidation(string Filler, int numWhiteSpace)
    {
        var regex = $@"^\s{numWhiteSpace}$";
        var match = Regex.Match(Filler, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid filler");
        }
    }
}