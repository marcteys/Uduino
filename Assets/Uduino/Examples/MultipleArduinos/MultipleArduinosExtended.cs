using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using Uduino;

public class MultipleArduinosExtended : MonoBehaviour
{

    UduinoManager u;
    public int sensorOne = 0;
    public int sensorTwo = 0;

    void Start()
    {
        UduinoManager.Instance.OnValueReceived += OnValuesReceived;
    }

	void Update ()
	{
        UduinoManager.Instance.Read("leo", "read");
        UduinoManager.Instance.Read("test", "read");
    }

    void OnValuesReceived(string data, string device)
    {
        if (device == "leo") sensorOne = int.Parse(data);
        else if (device == "test") sensorTwo = int.Parse(data);
    }
}
