using UnityEngine;
using System.Collections;
using Uduino; // adding Uduino NameSpace 

public class BlinkLed : MonoBehaviour
{
    UduinoManager u; // The instance of Uduino is initialized here
    public int blinkPin = 13;
    [Range(0,5)]
    public float blinkSpeed = 1;
    void Start()
    {
        UduinoManager.Instance.InitPin(blinkPin, PinMode.Output);
        StartCoroutine(BlinkLoop());
    }

    IEnumerator BlinkLoop()
    {
        while (true)
        {
            UduinoManager.Instance.digitalWrite(blinkPin, State.HIGH);
            yield return new WaitForSeconds(blinkSpeed);
            UduinoManager.Instance.digitalWrite(blinkPin, State.LOW);
            yield return new WaitForSeconds(blinkSpeed);
        }
    }
}