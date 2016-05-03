using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using Uduino;

public class ReadSensor : MonoBehaviour
{

    UduinoManager u;
    public int sensor = 0;

	void Start ()
	{
        UduinoManager.Instance.OnValueReceived += OnValueReceived;
    }

	void Update ()
	{
        UduinoManager.Instance.Read("sensorArduino", "SENSOR", 2000);
        this.transform.position = new Vector3(sensor, 0.0f, 0.0f);
	}

    void OnValueReceived(string data)
    {
        sensor = int.Parse(data);
    }
}
