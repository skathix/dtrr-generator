namespace FileIngestor.Tools;


private static string PadToExpectedLength(string line, IReadOnlyList<int> acceptedLengths)
{
    // choose the smallest accepted length that can contain the line
    var target = acceptedLengths.OrderBy(x => x).FirstOrDefault(x => x >= line.Length);

    // if no target found, return as-is (will fail validation later)
    if (target == 0) return line;

    // pad with spaces to preserve fixed-width indexing
    return line.PadRight(target, ' ');
}


private static string VisualizeSpaces(string line, char blankChar = '-')
{
    return line.Replace(' ', blankChar);
}

