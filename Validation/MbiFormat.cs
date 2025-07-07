using System.Text.RegularExpressions;

namespace Tools.Validation;

public class MbiFormat
{
    private bool MbiCheck(string beneficiary_id)
    {
        try
        {
            beneficiary_id.Length = 12;
            var regex = @"^\d[A-Za-z]\d\d[A-Za-z]\d\d[A-Za-z]{2}\d$";
            var match = Regex.Match(beneficiary_id, regex, RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
    }
}