using System;
using GridAiGames.Logging;

namespace GridAiGames.Bomberman.Tests
{
    internal class TestLogger : ILogger
    {
        public void Log(LogType type, string message)
        {
            switch (type)
            {
                case LogType.Info:
                    break;
                case LogType.Warning:
                case LogType.Error:
                    throw new InvalidOperationException("Unit tests should not generate warnings or errors.");
                default:
                    throw new InvalidOperationException($"Unknown log type: '{type}'.");
            }
        }
    }
}
