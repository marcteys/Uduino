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

    /*
     * TODO : SthiThis is not working. SHould remove all references to advancedWrite
    IEnumerator BlinkTestLoop()
    {
        while (true)
        {
            u.Write("uduinoBoard",new string[] {"d 3","d 4","d 5","d 6","d 7"}, new int[] { 255,255,255,255,255});
            yield return new WaitForSeconds(1);
            u.Write("uduinoBoard", new string[] { "d 3", "d 4", "d 5", "d 6", "d 7" }, new int[] { 0, 0, 0, 0, 0 });
            u.SendBundle("LedOff");
            yield return new WaitForSeconds(1);
        }
    }
    */

    IEnumerator BlinkAllLoop()
    {
        while (true)
        {
            for (int i = 2; i < 13; i++)
            {
                u.digitalWrite(i, State.HIGH,"LedOn");
            }
            u.SendBundle("LedOn");
            yield return new WaitForSeconds(1);
            for (int i = 2; i < 13; i++)
            {
                u.digitalWrite(i, State.LOW, "LedOff");
            }
            u.SendBundle("LedOff");
            yield return new WaitForSeconds(1);
        }
    }
}
