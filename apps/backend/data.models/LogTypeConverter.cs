namespace LingoLogger.Data.Models;

public class LogTypeConverter
{
    public static string ConvertLogTypeToString(LogType logType) => logType switch
    {
        LogType.Readable => "Readable",
        LogType.Audible => "Audible",
        LogType.Watchable => "Watchable",
        LogType.Other => "Other",
        _ => "Other"
    };

    public static LogType ConvertStringToLogType(string logTypeString) => logTypeString switch
    {
        "Readable" => LogType.Readable,
        "Audible" => LogType.Audible,
        "Watchable" => LogType.Watchable,
        "Other" => LogType.Other,
        _ => LogType.Other
    };
}