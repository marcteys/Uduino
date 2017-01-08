using UnityEngine;
using System;

namespace Uduino
{

    public static class Log 
    {
        private static LogLevel _debugLevel;

        public static string CurrentClass
        {
            get
            {
                var st = new System.Diagnostics.StackTrace();

                var index = Mathf.Min(st.FrameCount - 1, 2);

                if (index < 0)
                    return "{NoClass}";

                return "" + st.GetFrame(index).GetMethod().DeclaringType.Name + "";
            }
        }

        public static void Error(object message)
        {
            if((int)_debugLevel <= (int)LogLevel.ERROR)
                Debug.LogError(string.Format("{0}:{1}",  CurrentClass, message));
        }

        public static void Warning(object message)
        {
            if ((int)_debugLevel <= (int)LogLevel.WARNING)
            Debug.LogWarning(string.Format("{0}:{1}", CurrentClass, message));
        }
        public static void Info(object message)
        {
            if ((int)_debugLevel <= (int)LogLevel.INFO)
            Debug.Log(string.Format("{0}:{1}", CurrentClass, message));
        }

        public static void SetLogLevel(LogLevel level)
        {
            _debugLevel = level;
        }

    }

}