namespace LingoLogger.Data.Models;

public class LogTypeConverter
{
    public static string ConvertLogTypeToString(LogType logType) => logType switch
    {
        LogType.Readable => "Readable",
        LogType.Audible => "Audible",
        LogType.Watchable => "Watchable",
        LogType.Anki => "Anki",
        _ => throw new ArgumentOutOfRangeException(nameof(logType), logType, null)
    };

    public static LogType ConvertStringToLogType(string logTypeString) => logTypeString switch
    {
        "Readable" => LogType.Readable,
        "Audible" => LogType.Audible,
        "Watchable" => LogType.Watchable,
        "Anki" => LogType.Anki,
        _ => throw new ArgumentOutOfRangeException(nameof(logTypeString), logTypeString, "Invalid log type")
    };
}