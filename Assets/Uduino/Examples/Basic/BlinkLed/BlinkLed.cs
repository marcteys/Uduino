using UnityEngine;
using System.Collections;
using Uduino; // adding Uduino NameSpace 

public class BlinkLed : MonoBehaviour
{
    UduinoManager u; // The instance of Uduino is initialized here

    
    void Start()
    {
        UduinoManager.Instance.InitPin(13, PinMode.PWM);
        StartCoroutine(BlinkLoop());
    }

    IEnumerator BlinkLoop()
    {
        while (true)
        {
            UduinoManager.Instance.Write(13, State.HIGH);
            yield return new WaitForSeconds(1);
            UduinoManager.Instance.Write(13, State.LOW);
            yield return new WaitForSeconds(1);
        }
    }
}