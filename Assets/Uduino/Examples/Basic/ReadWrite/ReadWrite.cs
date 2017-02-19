using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;

public class ReadWrite : MonoBehaviour {

    UduinoManager u;
    int readValue = 0;

    void Start ()
    {
        UduinoManager.Instance.InitPin(AnalogPin.A0, PinMode.Analog);
        UduinoManager.Instance.InitPin(11, PinMode.PWM);
    }

    void Update ()
    {
        ReadValue();
    }

    void ReadValue()
    {
        readValue = UduinoManager.Instance.analogRead(AnalogPin.A0);
        UduinoManager.Instance.analogWrite(11,readValue/2);
    }
}
