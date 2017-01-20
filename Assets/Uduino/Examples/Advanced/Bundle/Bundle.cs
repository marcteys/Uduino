using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;

public class Bundle : MonoBehaviour {

    UduinoManager u;

	void Start ()
    {
        u = UduinoManager.Instance;

        for(int i=2;i < 13;i++)
        {
            u.InitPin(i, PinMode.Output);
        }

        StartCoroutine(BlinkAllLoop());
    }

    IEnumerator BlinkAllLoop()
    {
        while (true)
        {
            for (int i = 2; i < 11; i++)
            {
                u.digitalWrite(i, State.HIGH,"LedOn");
            }
           u.SendBundle("LedOn");
            yield return new WaitForSeconds(1);
            for (int i = 2; i < 11; i++)
            {
                u.digitalWrite(i, State.LOW, "LedOff");
            }
           u.SendBundle("LedOff");
            yield return new WaitForSeconds(1);
        }
    }
}
