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
        UduinoManager.Instance.OnValueReceived += OnValueReceived; //Create the Delegate
    }

    void Update ()
    {
        readValue = UduinoManager.Instance.analogRead(AnalogPin.A0);
        lightSouce.intensity = readValue/255;
    }

    void OnValueReceived(string data, string device)
    {
        Debug.Log(int.Parse(data)); // Use the data as you want !
    }

}
