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

      //  u.Read("myArduino", "myVar", 50);

        if (send == true)
        {
          //  Debug.Log(uduino.Read("myArduino", "myVar", 1000 / 120));
            send = false;
        }
   //     uduino.SendCommand("myArduino", "R");
    //  uduino.TRead("myArduino", "myVar");


    //  u.ARead("myArduino");


    //    Debug.Log(uduino.Read("myArduino",0));
	}

    void OnValueReceived(object data)
    {
        Debug.Log((string)data);
    }
}
