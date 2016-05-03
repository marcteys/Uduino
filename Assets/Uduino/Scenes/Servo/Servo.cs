using UnityEngine;
using System.Collections;
using Uduino;

public class Servo : MonoBehaviour {

    [Range(0, 180)]
    public int servoAngle = 0;

    void Update()
    {
        UduinoManager.Instance.Write("servo", "R", servoAngle);
    }
}
