using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Tools.Validation;

public class MbitFields
{
    private void MbiCheck(string beneficiary_id)
    {
        var regex = @"^\d[A-Za-z]\d\d[A-Za-z]\d\d[A-Za-z]{2}\d$";
        var mbiMatch = Regex.Match(beneficiary_id, regex
            , RegexOptions.IgnoreCase);
        if (!mbiMatch.Success)
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
    private class SexCodeCheck
    {
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
    }
    class BirthDateCheck
    {
        private void ValidateBirthDate(string date)
        {
            var yearText = date.Substring(0, 4);
            Debug.Assert(yearText != null, nameof(yearText) + " != null");
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
    }

    class RecordType
    {
        void recordTypeHc(string recordType)
        {
            var HcRecordType = ("T");
        }
    }

    class ContractCheck
    {
        private void contract(string contractNumber)
        {
            var regex = @"^[A-Za-z]\d\d\d\d$";
            var contractMatch = Regex.Match(contractNumber, regex
                , RegexOptions.IgnoreCase);
            if (!contractMatch.Success)
            {
                Console.WriteLine(
                    "Contract Number format: you would get a TCR 007 - Reject for this");
            }
        }
    }

    class StateCodeLookup
    {
        private void NumericStateCode(string StateCode)
        {
            var regex = @"^\d\d$";
            var match = Regex.Match(StateCode, regex);
            if (!match.Success)
            {
                Console.WriteLine(
                    "State Code: Sent to CMS");
            }
        }
    }

    class CountyCodeLookup
    {
        private void NumericCountyCode(string CountyCode)
        {
            var regex = @"^\d\d$";
            var match = Regex.Match(CountyCode, regex);
            if (!match.Success)
            {
                Console.WriteLine(
                    "County Code: Sent by CMS");
            }
        }
    }

    class DisabilityCheck
    {
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







    }
}
