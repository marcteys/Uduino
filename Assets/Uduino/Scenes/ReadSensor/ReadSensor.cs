using UnityEngine;
using System.Collections;
using Uduino;

public class ReadSensor : MonoBehaviour {

    UduinoManager uduino;

	void Start ()
	{
        uduino = UduinoManager.Instance;
    }
	
	void Update ()
	{
   //     uduino.SendCommand("myArduino", "R");
        Debug.Log(uduino.Read("myArduino","myVar"));

    //    Debug.Log(uduino.Read("myArduino",0));
	}
}
