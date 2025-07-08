using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Tools.Validation;
//designed to be reusable, once it works!
public class DateCheck
{
    private void ValidateDate(string date, string validOne, string validTwo, string validThree, string dayOfMonth)
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
