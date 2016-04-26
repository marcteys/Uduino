using UnityEngine;
using System.Collections;

public class LivingKeyboard : LivingDevice {

    public enum Tilt { LEFT, RIGHT };
    public enum Direction { FORWARD, BACKWARD };

    public LivingKeyboard(SerialArduino sA) : base(sA)
    {

    }

    public void Rotate(int angle, Tilt t = Tilt.LEFT)
    {
        int a = angle;
        if (t == Tilt.LEFT)
            a *= -1;
        SendCommand('R', a);
    }

    public void Translate(int distance, Direction d = Direction.FORWARD)
    {
        int dis = distance;
        if (d == Direction.BACKWARD)
            dis *= -1;
       SendCommand('T', dis);
    }

}
