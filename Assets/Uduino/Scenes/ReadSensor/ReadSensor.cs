using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using Uduino;

public class ReadSensor : MonoBehaviour
{

    UduinoManager u;
    int pos = 0;

	void Start ()
	{
        u = UduinoManager.Instance;
        u.OnValueReceived += OnValueReceived;
    }

	void Update ()
	{
        u.Read("myArduino","SENSOR");
        this.transform.position = new Vector3(pos, 0.0f, 0.0f);
	}

    void OnValueReceived(string data)
    {
        pos = int.Parse(data);
    }
}
