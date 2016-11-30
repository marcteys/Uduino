using UnityEngine;

public static class UduinoUtils {

    public static int ToArduinoInt(this int value, int valueMin, int valueMax)
    {
        float floatVal = value;
        return Mathf.FloorToInt(floatVal.Map(valueMin, valueMax, 0, 255));
    }

    public static int ToArduinoInt(this float value, float valueMin, float valueMax)
    {
        return Mathf.FloorToInt(value.Map(valueMin, valueMax, 0, 255));
    }

    /// <summary>
    /// Map a current value between a maximum and a minimum
    /// </summary>
    /// <param name="value">Value we want to map</param>
    /// <param name="low1">The assumed minimum of current value</param>
    /// <param name="high1">The assumed maximum of curent value</param>
    /// <param name="low2">Minimim range of returned value</param>
    /// <param name="high2">Maximum range of returned value</param>
    /// <returns>New value mapped between low2 and high2float</returns>
    public static float Map(this float value, float low1, float high1, float low2, float high2)
    {
        float val = low2 + (value - low1) * (high2 - low2) / (high1 - low1);
        //if (val > high2) val = high2;
        return val;
    }

}
