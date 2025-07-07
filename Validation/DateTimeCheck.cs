namespace Tools.Validation;
//designed to be reusable, once it works!
public class DateTimeCheck
{
    private bool ValidateDate(string date)
    {
        try
        {
            date.Length = 8;
            ValidateDate(date);
            
            return true;
        }
        catch
        {
            return false;
        }
    } 
    private bool ValidateDateTime(string dateTime)
    {
        try
        {
            dateTime.Length = ?;
            //TODO: split string for compare
            // ValidateDate(date);
            //TODO: Validate time
            
            return true;
        }
        catch
        {
            return false;
        }
    } 
}