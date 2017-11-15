namespace GridAiGames.Logging
{
    public interface ILogger
    {
        void Log(LogType type, string message);
    }

    public enum LogType
    {
        Info,
        Warning,
        Error
    }
}
