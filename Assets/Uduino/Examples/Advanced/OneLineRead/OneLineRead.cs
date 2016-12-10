using UnityEngine;
using System.Collections;
using Uduino;

public class UduinoShortCall : MonoBehaviour
{
    UduinoManager u;
    public int sensorOne = 0;

    void Update()
    {
        UduinoManager.Instance.Read("myArduinoBoard", "mySensor", action: (string data) => sensorOne = int.Parse(data));
    }
}