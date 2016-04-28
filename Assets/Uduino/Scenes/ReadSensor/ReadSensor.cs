using UnityEngine;
using System.Collections;
using Uduino;

public class ReadSensor : MonoBehaviour {

    UduinoManager uduino;

    public bool send = false;

	void Start ()
	{
        uduino = UduinoManager.Instance;
    }
	
	void Update ()
	{
   //     uduino.SendCommand("myArduino", "R");
        Debug.Log(uduino.Read("myArduino","myVar",500));


      //  uduino.ARead("myArduino");


    //    Debug.Log(uduino.Read("myArduino",0));
	}
}
