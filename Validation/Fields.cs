using System.Text.RegularExpressions;

namespace Tools.Validation;

public class Fields
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
        //pseudo
        //Get length of LastName and FirstName
        //Console.WriteLine("Names are blank: you would get a TCR 004 - Reject for this");
        //Regex for all alphas for all three - string MI may be blank
    }


    private class SexCodeCheck
    {
        private void SexCheck(string sexCode)
        {
            //ternary?
            //var checkSex= (sexCode = 'M' ||sexCode ='F')?
            //Console.WriteLine("this Value is correct"):
            //Console.WriteLine(This value will be set to '0' - no fail or reject"));
        }
    }

    private class BirthDateCheck
    {
        private void ValidateBirthDate(string date, string validOne
            , string validTwo, string validThree, string dayOfMonth)
        {

            var year = date.Substring(0, 4);
            var day = date.Substring(6, 2);
            //pseudo examples
            //year > 1870
            //year < = current year
            //date < EffectiveDate
            //day = 01

        }
    }

    private class HardcodeRecordType
    {
        void recordTypeHardcode(string recordType)
        {
            var HcRecordType = ("T");
        }
    }

    private class ContractCheck
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
    
    private class StateCodeLookup
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

    private class CountyCodeLookup
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






}