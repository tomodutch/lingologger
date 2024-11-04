using System.Text.RegularExpressions;

namespace LingoLogger.Web.Models;

public class TimeParser
{
    public int ParseTimeToSeconds(string timeStr)
    {
        // Regular expression to match the time format
        var pattern = @"(?:(\d+)h)?(?:(\d+)m)?(?:(\d+)s)?";
        var matches = Regex.Match(timeStr, pattern);

        int totalSeconds = 0;

        if (matches.Success)
        {
            if (matches.Groups[1].Success) // hours
            {
                totalSeconds += int.Parse(matches.Groups[1].Value) * 3600;
            }
            if (matches.Groups[2].Success) // minutes
            {
                totalSeconds += int.Parse(matches.Groups[2].Value) * 60;
            }
            if (matches.Groups[3].Success) // seconds
            {
                totalSeconds += int.Parse(matches.Groups[3].Value);
            }
        }
        else
        {
            throw new ArgumentException("Invalid time format");
        }

        return totalSeconds;
    }
}
