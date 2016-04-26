using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public static class ExtensionMethods
{
    public static float Map(this float value, float low1, float high1, float low2, float high2)
    {

        float val = low2 + (value - low1) * (high2 - low2) / (high1 - low1);
        if (val > high2)
        {
            val = high2;
        }
        return val;


    }


}