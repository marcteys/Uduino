using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;

public class ReadWrite : MonoBehaviour {

    UduinoManager u;
    int readValue = 0;

    void Start ()
    {
        u = UduinoManager.Instance;
        u.InitPin(AnalogPin.A0, PinMode.Analog);
        u.InitPin(11, PinMode.PWM);
    }

    void Update ()
    {
        ReadValue();
    }

    void ReadValue()
    {
        readValue = u.analogRead(AnalogPin.A0);
        u.analogWrite(11,readValue/10);
    }
}
