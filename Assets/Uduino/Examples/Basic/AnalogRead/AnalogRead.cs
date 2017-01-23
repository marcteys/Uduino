using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;

public class AnalogRead : MonoBehaviour {

    UduinoManager u;
    int readValue = 0;
    public Light lightSouce;
    public Light lightSouce2;

    int test = 0;

    void Start ()
    {
        UduinoManager.Instance.InitPin(AnalogPin.A0, PinMode.Analog);
        UduinoManager.Instance.InitPin(AnalogPin.A1, PinMode.Analog);
    }

    void Update ()
    {
        ReadLights();
    }

    void ReadLights()
    {
        readValue = UduinoManager.Instance.analogRead(AnalogPin.A0, "PinRead");
        lightSouce.intensity = readValue / 400.0f;

        test = UduinoManager.Instance.analogRead(AnalogPin.A1, "PinRead");
        lightSouce2.intensity = test / 200.0f;

        UduinoManager.Instance.SendBundle("PinRead");
    }

}
