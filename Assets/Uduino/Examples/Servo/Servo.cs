using UnityEngine;
using System.Collections;
using Uduino;

public class Servo : MonoBehaviour {

    [Range(0, 180)]
    public int servoAngle = 0;

    void Update()
    {
        OptimizedWrite();
    }

    //Regular send
    void RegularWrite()
    {
        UduinoManager.Instance.Write("servo", "R", servoAngle);
    }

    //Optimized Send
    private int prevServoAngle = 0;
    void OptimizedWrite()
    {
        if (servoAngle != prevServoAngle)
        {
            UduinoManager.Instance.Write("servo", "R", servoAngle);
            prevServoAngle = servoAngle;
        }
    }
}
