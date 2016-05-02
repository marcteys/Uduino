using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using Uduino;

public class ReadSensor : MonoBehaviour
{

    public bool send = false;
    UduinoManager u;

	void Start ()
	{
        u = UduinoManager.Instance;
        u.OnValueReceived += OnValueReceived;
    }

	void Update ()
	{
        u.Read("myArduino","myVar");
	}

    void OnValueReceived(object data)
    {
        Debug.Log((string)data);
    }
}
