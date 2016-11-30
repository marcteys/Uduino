using UnityEngine;
using System;

namespace Uduino
{
    public enum LogLevel
    {
        NONE,
        INFO,
        WARNING,
        ERROR,
    }

    public static class Log 
    {

        public static void Error(object message)
        {
            if((int)UduinoManager.debugLevel >= (int)LogLevel.ERROR)
                Debug.LogError(message);
        }

        public static void Warning(object message)
        {
            if ((int)UduinoManager.debugLevel >= (int)LogLevel.WARNING)
            Debug.LogWarning(message);
        }
        public static void Info(object message)
        {
            if ((int)UduinoManager.debugLevel >= (int)LogLevel.INFO)
            Debug.Log(message);
        }

    }

}



