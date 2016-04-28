using UnityEngine;
using System.Collections;

public class LivingScreen : LivingDevice {

    public enum Direction { RIGHT, LEFT };
    public static int maxWidth = 645; //TODO : should be returned once calibration done

    int offset = -10;

    public LivingScreen(SerialArduino sA)  : base(sA)
    {

    }

    public void Rotate(int angle) {
        int newAngle = angle + offset;
        if (newAngle <= 0) newAngle = 0;
        if (newAngle > 180) newAngle = 180;

        SendCommand('R', newAngle);
    }

    public void MoveTo(int pos)
    {
        SendCommand('M', pos);
    }

    public void MoveTo(float pos)
    {
        int intPos = Mathf.FloorToInt(pos.Map(0, 1, 0, maxWidth));
        SendCommand('M', intPos);
    }

    public void Translate(int distance,Direction d = Direction.RIGHT )
    {
        int dis = distance;
        if (d == Direction.LEFT)
            dis *= -1;
       SendCommand('T', dis);
    }

    public void ShakeScreen(int intensity)
    {
        SendCommand('Z', intensity);
    }

    public int GetWidth()
    {
        SendCommand('W');
        //  StartCoroutine(serialObject.AsynchronousReadFromArduino((string s) => this.LivingObjectFound(s, serialObject), (string pName) => Debug.Log("Impossible to get name on " + pName), 3f));
        return int.Parse(ReadFromArduino(0));
    }

    public int GetPosition()
    {
        SendCommand('P');
        //  StartCoroutine(serialObject.AsynchronousReadFromArduino((string s) => this.LivingObjectFound(s, serialObject), (string pName) => Debug.Log("Impossible to get name on " + pName), 3f));
        return int.Parse(ReadFromArduino(10));
    }
}
