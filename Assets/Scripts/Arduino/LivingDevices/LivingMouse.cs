using UnityEngine;
using System.Collections;

public class LivingMouse : LivingDevice {

    int posX, posY, toolDistance;
    static int maxX = 1400;
    static int maxY = 1700;

    public LivingMouse(SerialArduino sA)  : base(sA)
    {
        // this command only launc init_stepper() on arduino side
       //    Calibrate();
        posX = PlayerPrefs.GetInt("mouseX");
        posY = PlayerPrefs.GetInt("mouseY");
    }

    public void MoveTo(int x, int y, bool toolup = true)
    {
        x = verifyX(x);
        y = verifyY(y);
        //  x = correctX(x);
       // y = correctX(y);
        // 310 * 390
        // x : 1400
        // y : 1700
        int toolupToInt = toolup ? 1 : 0;
        char[] commands = {'G', 'X', 'Y', 'Z'};
        int[] values = { 0, x, y, toolupToInt * toolDistance };
        SendCommand(commands, values, 4);
        posX = x;
        posY = y;
        SetPos();
    }


    public void MoveOf(int x, int y, bool toolup = true)
    {
        x = verifyX(x);
        y = verifyY(y);
        int toolupToInt = toolup ? 1 : 0;
        char[] commands = { 'G', 'X', 'Y', 'Z' };
        int[] values = { 0, posX + x, posY+y, toolupToInt * toolDistance };
        SendCommand(commands, values, 4);
        posX += x;
        posY += y;
        SetPos();
    }

    public void MoveToPercent(float fx, float fy, bool toolup = true)
    {

        int x = Mathf.FloorToInt(fx.Map(0.0f, 1.0f, 0.0f, maxX));
        int y = Mathf.FloorToInt(fy.Map(0.0f, 1.0f, 0.0f, maxY));
      //  x = correctX(x);
     //   y = correctX(y);

        int toolupToInt = toolup ? 1 : 0;
        char[] commands = { 'G', 'X', 'Y', 'Z' };
        int[] values = { 0, x, y, toolupToInt * toolDistance };
        SendCommand(commands, values, 4);
        posX = x;
        posY = y;
        SetPos();
    }

    public void MoveArc(int centerx, int centery, int angle, bool toolup = true)
    {

     //   centerx = correctX(centerx);
       // centery = correctX(centery);
        centerx = verifyX(centerx);
        centery = verifyY(centery);

        int g;
        if (angle > 0)
            g = 2;
        else
            g = 3;

        int toolupToInt = toolup ? 1 : 0;

        float a = angle * Mathf.PI / 180.0f;
        int deltax = Mathf.FloorToInt(posX - centerx);
        int deltay = Mathf.FloorToInt(posY - centery);
        float currentangle = Mathf.Atan2(deltay, deltax);
        float radius = Mathf.Sqrt(Mathf.Pow(deltax, 2.0f) + Mathf.Pow(deltay, 2.0f));
        int destx = Mathf.FloorToInt(centerx + radius * Mathf.Cos(currentangle + a));
        int desty = Mathf.FloorToInt(centery + radius * Mathf.Sin(currentangle + a));

        char[] commands = {'G', 'X', 'Y', 'Z', 'I', 'J'};
        int[] values = {g, destx, desty, toolupToInt * toolDistance, -deltax, -deltay};
        SendCommand(commands, values, 6);
        posX = destx;
        posY = desty;
        SetPos();
    }

    public override void Calibrate() {
        base.Calibrate();
        posX = 0;
        posY = 0;
        SetPos();
    }

    public int GetX()
    {
        return posX;
    }

    public int GetY()
    {
        return posY;
    }

    public int GetMaxX()
    {
        return maxX;
    }

    public int GetMaxY()
    {
        return maxY;
    }

    int correctX(int x)
    {
        int i = maxX - x;
        return i;
    }

    int correctY(int y)
    {
        int i = maxY - y;
        return i;
    }

    int verifyX(int x)
    {
        int i = x;
        if (i > maxX) i = maxX;
        return i;
    }

    int verifyY(int y)
    {
        int i = y;
        if (i > maxY) i = maxY;
        return i;
    }

    void SetPos()
    {
        PlayerPrefs.SetInt("mouseX", posX);
        PlayerPrefs.SetInt("mouseY", posY);
    }
}
