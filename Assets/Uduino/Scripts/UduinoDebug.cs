using UnityEngine;
using System;

namespace Uduino
{
    public enum LogLevel
    {
        INFO,
        WARNING,
        ERROR,
        NONE
    }

    public static class Log 
    {
        private static LogLevel _debugLevel;

        public static void Error(object message)
        {
            if((int)_debugLevel <= (int)LogLevel.ERROR)
                Debug.LogError(message);
        }

        public static void Warning(object message)
        {
            if ((int)_debugLevel <= (int)LogLevel.WARNING)
            Debug.LogWarning(message);
        }
        public static void Info(object message)
        {
            if ((int)_debugLevel <= (int)LogLevel.INFO)
            Debug.Log(message);
        }

        public static void SetLogLevel(LogLevel level)
        {
            _debugLevel = level;
        }

    }

}



