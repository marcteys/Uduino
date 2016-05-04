using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using Uduino;

public class MultipleArduinosAction : MonoBehaviour
{

    UduinoManager u;
    public int sensorOne = 0;
    public int sensorTwo = 0;
   
	void Update ()
	{
        UduinoManager.Instance.Read("leo", "read", action:((string data) => sensorOne = int.Parse(data)));
        UduinoManager.Instance.Read("test", "read", action: ((string data) => sensorTwo = int.Parse(data)));
    }

}
