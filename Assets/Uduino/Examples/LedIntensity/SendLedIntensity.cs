using UnityEngine;
using System.Collections;
using Uduino;

public class SendLedIntensity : MonoBehaviour
{
    [Range(0, 255)]
    public int intensity = 0;

    void Update()
    {
        UduinoManager.Instance.Write("ledIntensity", "SetLight", intensity);
    }
}
