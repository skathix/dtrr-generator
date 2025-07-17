using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Tools.Validation;

public class MbitFields
{
    private void MbiCheck(string beneficiary_id)
    {
        var regex = @"^\d[A-Za-z]\d\d[A-Za-z]\d\d[A-Za-z]{2}\d$";
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
        
        var surnameRegex = @"^[A-Za-z]{11}/s$";
        var firstNameRegex = @"^[A-Za-z]{7}$";
        var miRegex = @"^[a-zA-z\\s]+$";
        //pseudo
        //Get length of LastName and FirstName
        //Console.WriteLine("Names are blank: you would get a TCR 004 - Reject for this");
        //Regex for all alphas for all three - string MI may be blank
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
                        Console.WriteLine("This will be set to 0: not fail nor rejection");
                        break;
                }
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
            
            if (year >= (DateTime.Now.Year)+1)
            {
                Console.WriteLine(
                    "This is a date too far in the future - will be rejected");
            }
            
            
            //pseudo examples
            //year > 1870
            //year < = current year
            //date < EffectiveDate
            //day = 01

        }
    }

    class HardcodeRecordType
    {
        void recordTypeHardcode(string recordType)
        {
            var HcRecordType = ("T");
        }
    }

    class ContractCheck
    {
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
