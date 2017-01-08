using UnityEngine;

namespace Uduino
{
    public static class Log 
    {
        private static LogLevel _debugLevel;

        public static void Error(object message)
        {
            if((int)_debugLevel <= (int)LogLevel.ERROR)
                UnityEngine.Debug.LogError(message);
        }

        public static void Warning(object message)
        {
            if ((int)_debugLevel <= (int)LogLevel.WARNING)
                UnityEngine.Debug.LogWarning(message);
        }
        public static void Info(object message)
        {
            if ((int)_debugLevel <= (int)LogLevel.INFO)
                UnityEngine.Debug.Log(message);
        }

        public static void Debug(object message)
        {
            if ((int)_debugLevel <= (int)LogLevel.DEBUG)
                UnityEngine.Debug.Log(message);
        }

        public static void SetLogLevel(LogLevel level)
        {
            _debugLevel = level;
        }

    }

}