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
        if (true || Input.GetKey(KeyCode.Space))
        {
            // Single();
            Multiple();
        }
    }

    void Single()
    {
        readValue = UduinoManager.Instance.analogRead(AnalogPin.A0);
        lightSouce.intensity = readValue / 400.0f;

        test = UduinoManager.Instance.analogRead(AnalogPin.A1);
        lightSouce2.intensity = test / 200.0f;
    }

    void Multiple()
    {
        readValue = UduinoManager.Instance.analogRead(AnalogPin.A0, "PinRead");
        lightSouce.intensity = readValue / 400.0f;

        test = UduinoManager.Instance.analogRead(AnalogPin.A1, "PinRead");
        lightSouce2.intensity = test / 200.0f;

        UduinoManager.Instance.SendBundle("PinRead");
    }

}
