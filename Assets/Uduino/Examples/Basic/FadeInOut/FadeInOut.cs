using UnityEngine;
using System.Collections;
using Uduino; // adding Uduino NameSpace 

public class FadeInOut : MonoBehaviour
{
    UduinoManager u; // The instance of Uduino is initialized here
    int ledPin = 9;
    int brightness = 0;
    int fadeAmount = 5;

    void Start()
    {
        UduinoManager.Instance.InitPin(ledPin, PinMode.Output);
        StartCoroutine(FadeLoop());
    }

    IEnumerator FadeLoop()
    {
        while (true)
        {
            UduinoManager.Instance.analogWrite(ledPin, brightness);
            brightness += fadeAmount;
            if (brightness <= 0 || brightness >= 255) fadeAmount = -fadeAmount;
            yield return new WaitForSeconds(0.01f);
        }
    }
}