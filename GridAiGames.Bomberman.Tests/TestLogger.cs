using System;
using GridAiGames.Logging;

namespace GridAiGames.Bomberman.Tests
{
    internal class TestLogger : ILogger
    {
        private readonly bool canGenerateWarningAndErrors;

        public TestLogger(bool canGenerateWarningAndErrors)
        {
            this.canGenerateWarningAndErrors = canGenerateWarningAndErrors;
        }

        public void Log(LogType type, string message)
        {
            switch (type)
            {
                case LogType.Info:
                    break;
                case LogType.Warning:
                case LogType.Error:
                    if (!canGenerateWarningAndErrors)
                    {
                        throw new InvalidOperationException("Unit tests should not generate warnings or errors.");
                    }
                    break;
                default:
                    throw new InvalidOperationException($"Unknown log type: '{type}'.");
            }
        }
    }
}
