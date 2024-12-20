using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LingoLogger.Web.Models;

public class TimeParser
{
    public string SecondsToTimeFormat(int seconds)
    {
        // Calculate hours, minutes, and remaining seconds
        int hours = seconds / 3600;
        int minutes = seconds % 3600 / 60;
        int remainingSeconds = seconds % 60;

        // Build the formatted string
        var sb = new StringBuilder();

        if (hours > 0)
            sb.Append($"{hours}h");
        if (minutes > 0)
            sb.Append($"{minutes}m");
        if (remainingSeconds > 0)
            sb.Append($"{remainingSeconds}s");


        if (sb.Length == 0)
        {
            return "0s";
        }

        return sb.ToString();
    }

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

    public DateTimeOffset? ParseDate(string createdAtString)
    {
        if (createdAtString == null)
        {
            return null;
        }

        var formatted = createdAtString.ToLowerInvariant().Trim();

        if (formatted == "yesterday")
        {
            return new DateTimeOffset(DateTimeOffset.UtcNow.AddDays(-1).UtcDateTime.Date, TimeSpan.Zero);
        }

        if (formatted == "tomorrow")
        {
            return new DateTimeOffset(DateTimeOffset.UtcNow.AddDays(1).UtcDateTime.Date, TimeSpan.Zero);
        }

        var canParse = DateTimeOffset.TryParseExact(createdAtString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var createdAt);
        if (canParse)
        {
            return createdAt;
        }

        return null;
    }
}
