using System;

namespace GridAiGames.Logging
{
    public class ConsoleLogger : ILogger
    {
        private readonly object sync = new object();

        public void Log(LogType type, string message)
        {
            lock (sync)
            {
                var oldColor = Console.ForegroundColor;

                switch (type)
                {
                    case LogType.Info:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    case LogType.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogType.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown log type: '{type}'.");
                }

                Console.Write(DateTime.Now.ToString("HH:mm:ss.fff"));
                Console.Write(" - ");
                Console.WriteLine(message);

                Console.ForegroundColor = oldColor;
            }
        }
    }
}
