using System.Text.RegularExpressions;

namespace Tools.Validation;

public class MbiFormat
{
    private void MbiCheck(string beneficiary_id)
    {
       
            var regex = @"^\d[A-Za-z]\d\d[A-Za-z]\d\d[A-Za-z]{2}\d$";
            var match = Regex.Match(beneficiary_id, regex
                , RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                Console.WriteLine("MBI format: you would get a TCR 007 - Reject for this");
            }

            Console.WriteLine(
                "POTENTIAL MBI not found: you would get a TCR 008 - Reject for this");
    }
}