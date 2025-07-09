using System.Text.RegularExpressions;

namespace Tools.Validation;

public class TextFields
{
    private void NamesCheck(string LastName, string FirstName, string MI)
    {
        //pseudo
        //Get length of LastName and FirstName
        //Console.WriteLine("Names are blank: you would get a TCR 004 - Reject for this");
        //Regex for all alphas for all three - string MI may be blank
    }
}

public class SexCodeCheck
{
    private void SexCheck(string sexCode)
    {
        //ternary?
        //var checkSex= (sexCode = 'M' ||sexCode ='F')? Console.WriteLine("this Value is correct"):Console.WriteLine(This value will be set to '0' - no fail or reject"));
    }
}