using UnityEngine;
using System.Collections;
using Uduino;

public class ReadSensor : MonoBehaviour
{

    UduinoManager u;

    void Awake()
    {
        UduinoManager.Instance.OnValueReceived += OnValueReceived; //Create the Delegate
    }

    void Update()
    {
        UduinoManager.Instance.Read("myArduinoName", "mySensor"); // Read every frame the value of the "mySensor" function on our board. 
    }

    void OnValueReceived(string data, string device)
    {
        Debug.Log(data); // Use the data as you want !
    }

    void Read()
    {

    }
}