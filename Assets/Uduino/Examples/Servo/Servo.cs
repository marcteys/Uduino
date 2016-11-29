using UnityEngine;
using System.Collections;
using Uduino;

public class Servo : MonoBehaviour {

    [Range(0, 180)]
    public int servoAngle = 0;
    private int prevServoAngle = 0;

    void Update()
    {
        OptimizedWrite();
    }

    void OptimizedWrite()
    {
        if (servoAngle != prevServoAngle) // Use this condition to not write at each frame 
        {
            UduinoManager.Instance.Write("servoBoard", "R", servoAngle);
            prevServoAngle = servoAngle;
        }
    }
}
