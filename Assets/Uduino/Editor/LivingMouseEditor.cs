using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(LivingDevicesManager))]
public class LivingMouseEditor : Editor
{
    private int targetX;
    private int targetY;
    LivingDevicesManager ldm = null;
    LivingMouse livingMouse = null;
    bool autoRead;

    public LivingMouseEditor(LivingDevicesManager ldm)
    {
        this.ldm = ldm;
    }

    public void Draw()
    {
        livingMouse = ldm.livingMouse;
        //title
        GUILayout.BeginHorizontal();
        GUILayout.Button("LivingMouse  [" + livingMouse.getSerial().getPort() + "]", "OL Title");
        GUILayout.EndHorizontal();

        // Global box
        GUILayout.BeginVertical("Box");


        GUILayout.BeginHorizontal("Box");


        if (GUILayout.Button("Center"))
        {
            livingMouse.MoveToPercent(0.5f, 0.5f);
            if (autoRead) Read(100);
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("L"))
        {
            int x = livingMouse.GetX();
            int y = livingMouse.GetY();
            livingMouse.MoveTo(x+200, y);
            livingMouse.MoveTo(x +200, y - 100);
            if (autoRead) Read(100);
        }
        if (GUILayout.Button("M"))
        {
            int x = livingMouse.GetX();
            int y = livingMouse.GetY();
            livingMouse.MoveTo(x-200, y);
            livingMouse.MoveTo(x - 100, y - 100);
            livingMouse.MoveTo(x - 200, y - 200);
            livingMouse.MoveTo(x , y -200);
            if (autoRead) Read(100);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("N"))
        {
            int x = livingMouse.GetX();
            int y =livingMouse.GetY();
            livingMouse.MoveTo(x-200, y);
            livingMouse.MoveTo(x , y-100);
            livingMouse.MoveTo(x - 200, y -100);
            if (autoRead) Read(100);
        }
        if (GUILayout.Button("U"))
        {
            int x = livingMouse.GetX();
            int y = livingMouse.GetY();
            livingMouse.MoveTo(x+200, y);
            livingMouse.MoveArc(x + 200, y + 100, 180);
            livingMouse.MoveTo(x, y-200);
            if (autoRead) Read(100);
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Vibrate"))
        {
            int x = livingMouse.GetX();
            int y = livingMouse.GetY();
            livingMouse.MoveTo(x + 20, y);
            livingMouse.MoveTo(x - 20, y);
            livingMouse.MoveTo(x + 20, y);
            if (autoRead) Read(100);
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical("Box");
        targetX = EditorGUILayout.IntField("X", targetX);
        targetY = EditorGUILayout.IntField("Y", targetY);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Move To"))
        {
            livingMouse.MoveTo(targetX, targetY);
        }
        if (GUILayout.Button("Move Of"))
        {
            int x = livingMouse.GetX();
            int y = livingMouse.GetY();
            livingMouse.MoveTo(x + targetX, y + targetY);
            if (autoRead) Read(100);
        }
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();


        GUILayout.BeginHorizontal("Box");
        if (GUILayout.Button("Get X"))
        {
            Debug.Log(livingMouse.GetX());
        }
        if (GUILayout.Button("Get Y"))
        {
            Debug.Log(livingMouse.GetY());
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("Box");
        if (GUILayout.Button("Calibrate"))
        {
            livingMouse.Calibrate();
            if (autoRead) Read(10);
        }
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal("Box");
        if (GUILayout.Button("Read arduino"))
        {
            Read(10);
        }
        autoRead = EditorGUILayout.Toggle("Auto", autoRead);
        GUILayout.EndHorizontal();


        // end global box
        GUILayout.EndVertical();
    }

    void Read(int timeout = 100)
    {
            Debug.Log(livingMouse.ReadFromArduino(timeout));
    }

}

/*(ABCDEFGHIJKLMNOPQRSTUVWXYZ)
(A)
N10 G0 Z2
N20 G0 X0 Y0
N30 G1 Z-1
N40 G1 X10 Y30
N50 G1 X20 Y0
N60 G0 Z2
N70 G0 X17.222 Y8.333
N80 G1 Z-1
N90 G1 X2.778 Y8.333
(B)
N100 G0 Z2
N110 G0 X30 Y16.667
N120 G1 Z-1
N130 G1 X38.333 Y16.667
N140 G3 X38.333 Y30 I0 J6.667
N150 G1 X30 Y30
N160 G1 X30 Y0
N170 G1 X38.333 Y0
N180 G3 X38.333 Y16.667 I0 J8.333
(C)
N190 G0 Z2
N200 G0 X70 Y0
N210 G1 Z-1
N220 G1 X65 Y0
N230 G2 X56.667 Y8.333 I0 J8.333
N240 G1 X56.667 Y21.667
N250 G2 X65 Y30 I8.333 J0
N260 G1 X70 Y30
(D)
N270 G0 Z2
N280 G0 X80 Y0
N290 G1 Z-1
N300 G1 X80 Y30
N310 G1 X88.333 Y30
N320 G2 X96.667 Y21.667 I0 J-8.333
N330 G1 X96.667 Y8.333
N340 G2 X88.333 Y0 I-8.333 J0
N350 G1 X80 Y0
(E)
N360 G0 Z2
N370 G0 X120 Y0
N380 G1 Z-1
N390 G1 X106.667 Y0
N400 G1 X106.667 Y16.667
N410 G1 X116.667 Y16.667
N420 G0 Z2
N430 G0 X106.667 Y16.667
N440 G1 Z-1
N450 G1 X106.667 Y30
N460 G1 X120 Y30
(F)
N470 G0 Z2
N480 G0 X130 Y0
N490 G1 Z-1
N500 G1 X130 Y16.667
N510 G1 X143.333 Y16.667
N520 G0 Z2
N530 G0 X130 Y16.667
N540 G1 Z-1
N550 G1 X130 Y30
N560 G1 X143.333 Y30
(G)
N570 G0 Z2
N580 G0 X166.667 Y16.667
N590 G1 Z-1
N600 G1 X170 Y16.667
N610 G1 X170 Y0
N620 G1 X161.667 Y0
N630 G2 X153.333 Y8.333 I0 J8.333
N640 G1 X153.333 Y21.667
N650 G2 X161.667 Y30 I8.333 J0
N660 G1 X170 Y30
(H)
N670 G0 Z2
N680 G0 X180 Y0
N690 G1 Z-1
N700 G1 X180 Y30
N710 G0 Z2
N720 G0 X180 Y16.667
N730 G1 Z-1
N740 G1 X196.667 Y16.667
N750 G0 Z2
N760 G0 X196.667 Y0
N770 G1 Z-1
N780 G1 X196.667 Y30
(I)
N790 G0 Z2
N800 G0 X208.333 Y0
N810 G1 Z-1
N820 G1 X208.333 Y30
(J)
N830 G0 Z2
N840 G0 X220 Y0
N850 G1 Z-1
N860 G1 X221.667 Y0
N870 G3 X230 Y8.333 I0 J8.333
N880 G1 X230 Y30
(K)
N890 G0 Z2
N900 G0 X240 Y0
N910 G1 Z-1
N920 G1 X240 Y30
N930 G0 Z2
N940 G0 X240 Y13.333
N950 G1 Z-1
N960 G1 X256.667 Y30
N970 G0 Z2
N980 G0 X243.333 Y16.667
N990 G1 Z-1
N1000 G1 X256.667 Y0
(L)
N1010 G0 Z2
N1020 G0 X266.667 Y30
N1030 G1 Z-1
N1040 G1 X266.667 Y0
N1050 G1 X280 Y0
(M)
N1060 G0 Z2
N1070 G0 X290 Y0
N1080 G1 Z-1
N1090 G1 X290 Y30
N1100 G1 X300 Y13.333
N1110 G1 X310 Y30
N1120 G1 X310 Y0
(N)
N1130 G0 Z2
N1140 G0 X320 Y0
N1150 G1 Z-1
N1160 G1 X320 Y30
N1170 G1 X336.667 Y0
N1180 G1 X336.667 Y30
(O)
N1190 G0 Z2
N1200 G0 X346.667 Y21.667
N1210 G1 Z-1
N1220 G2 X363.333 Y21.667 I8.333 J0
N1230 G1 X363.333 Y8.333
N1240 G2 X346.667 Y8.333 I-8.333 J0
N1250 G1 X346.667 Y21.667
(P)
N1260 G0 Z2
N1270 G0 X373.333 Y0
N1280 G1 Z-1
N1290 G1 X373.333 Y30
N1300 G1 X381.667 Y30
N1310 G2 X381.667 Y13.333 I0 J-8.333
N1320 G1 X373.333 Y13.333
(Q)
N1330 G0 Z2
N1340 G0 X400 Y21.667
N1350 G1 Z-1
N1360 G2 X416.667 Y21.667 I8.333 J0
N1370 G1 X416.667 Y8.333
N1380 G2 X400 Y8.333 I-8.333 J0
N1390 G1 X400 Y21.667
N1400 G0 Z2
N1410 G0 X410 Y6.667
N1420 G1 Z-1
N1430 G1 X420 Y0
(R)
N1440 G0 Z2
N1450 G0 X430 Y0
N1460 G1 Z-1
N1470 G1 X430 Y30
N1480 G1 X438.333 Y30
N1490 G2 X438.333 Y13.333 I0 J-8.333
N1500 G1 X430 Y13.333
N1510 G0 Z2
N1520 G0 X438.333 Y13.333
N1530 G1 Z-1
N1540 G1 X446.667 Y0
(S)
N1550 G0 Z2
N1560 G0 X456.667 Y8.333
N1570 G1 Z-1
N1580 G3 X465 Y16.667 I8.333 J0
N1590 G2 X471.667 Y23.333 I0 J6.667
(T)
N1600 G0 Z2
N1610 G0 X483.333 Y30
N1620 G1 Z-1
N1630 G1 X500 Y30
N1640 G0 Z2
N1650 G0 X491.667 Y30
N1660 G1 Z-1
N1670 G1 X491.667 Y0
(U)
N1680 G0 Z2
N1690 G0 X510 Y30
N1700 G1 Z-1
N1710 G1 X510 Y8.333
N1720 G3 X526.667 Y8.333 I8.333 J0
N1730 G1 X526.667 Y30
(V)
N1740 G0 Z2
N1750 G0 X536.667 Y30
N1760 G1 Z-1
N1770 G1 X546.667 Y0
N1780 G1 X556.667 Y30
(W)
N1790 G0 Z2
N1800 G0 X566.667 Y30
N1810 G1 Z-1
N1820 G1 X573.333 Y0
N1830 G1 X580 Y20
N1840 G1 X586.667 Y0
N1850 G1 X593.333 Y30
(X)
N1860 G0 Z2
N1870 G0 X603.333 Y0
N1880 G1 Z-1
N1890 G1 X623.333 Y30
N1900 G0 Z2
N1910 G0 X603.333 Y30
N1920 G1 Z-1
N1930 G1 X623.333 Y0
(Y)
N1940 G0 Z2
N1950 G0 X633.333 Y30
N1960 G1 Z-1
N1970 G1 X643.333 Y16.667
N1980 G1 X643.333 Y0
N1990 G0 Z2
N2000 G0 X643.333 Y16.667
N2010 G1 Z-1
N2020 G1 X653.333 Y30
(Z)
N2030 G0 Z2
N2040 G0 X663.333 Y30
N2050 G1 Z-1
N2060 G1 X683.333 Y30
N2070 G1 X663.333 Y0
N2080 G1 X683.333 Y0
*/