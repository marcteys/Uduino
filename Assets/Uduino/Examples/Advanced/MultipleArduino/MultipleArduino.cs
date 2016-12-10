using UnityEngine;
using System.Collections;
using Uduino;

public class MultipleArduino : MonoBehaviour
{
    UduinoManager u;
    int sensorOne = 0;
    int sensorTwo = 0;

    void Start()
    {
        UduinoManager.Instance.OnValueReceived += OnValuesReceived;
    }

    void Update()
    {
        UduinoManager.Instance.Read("myArduinoName", "myVariable");
        UduinoManager.Instance.Read("myOtherArduino", "mySensor");
        Log.Info(sensorOne);
        Log.Info(sensorTwo);
    }

    void OnValuesReceived(string data, string device)
    {
        if (device == "myArduinoName") sensorOne = int.Parse(data);
        else if (device == "myOtherArduino") sensorTwo = int.Parse(data);
    }
}
