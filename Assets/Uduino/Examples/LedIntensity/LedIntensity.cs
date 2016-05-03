using UnityEngine;
using System.Collections;
using Uduino;

public class LedIntensity : MonoBehaviour {

    [Range(0, 255)]
    public int intensity = 0;

	void Update ()
	{
        UduinoManager.Instance.Write("ledIntensity", "LIGHT",intensity);
	}
}