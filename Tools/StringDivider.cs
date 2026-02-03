using System;
using System.Collections.Generic;

namespace Tools.Tools;

public class StringDivider //mostly LLM assisted
{
    public static List<string> DivideStringIntoVariableSections(string input
        , List<int> sectionLengths)
    {
        if (sectionLengths == null || sectionLengths.Count == 0)
            throw new ArgumentException(
                "Section lengths list cannot be null or empty.");

        var sections = new List<string>();
        var currentIndex = 0;

        // Loop through the section lengths and extract the corresponding sections
        foreach (var length in sectionLengths)
            if (currentIndex + length > input.Length)
            {
                // If the remaining string is shorter than the section length,
                // take the rest of the string
                sections.Add(input.Substring(currentIndex));
                break;
            }
            else
            {
                sections.Add(input.Substring(currentIndex, length));
                currentIndex += length;
            }

        // Handle any remaining part of the string if there is any left
        if (currentIndex < input.Length)
            sections.Add(input.Substring(currentIndex));

        return sections;
    }
}