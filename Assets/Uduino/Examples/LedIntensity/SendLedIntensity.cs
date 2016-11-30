using UnityEngine;
using System.Collections;
using Uduino;

public class SendLedIntensity : MonoBehaviour
{
    [Range(0, 255)]
    public int intensity = 0;

    void Update()
    {
        Debug.Log(UduinoManager.debugLevel);
        //UduinoManager.Instance.Write("ledIntensity", "SetLight", intensity);
    }
}
