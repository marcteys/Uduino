using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;

public class AnalogRead : MonoBehaviour {

    UduinoManager u;
    int readValue = 0;
    public Light lightSouce;

    void Start ()
    {
        UduinoManager.Instance.InitPin(AnalogPin.A0, PinMode.Analog);
    }

    void Update ()
    {
        readValue = UduinoManager.Instance.analogRead(AnalogPin.A0);
        lightSouce.intensity = readValue/255;
    }

}
