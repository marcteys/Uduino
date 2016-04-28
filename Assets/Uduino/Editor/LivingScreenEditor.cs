using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(LivingDevicesManager))]
public class LivingScreenEditor : Editor
{
    private int angle;
    private int distance;
    private int position;
    LivingScreen.Direction direction;
    LivingDevicesManager ldm = null;
    LivingScreen livingScreen = null;
    bool autoRead = false;
    bool coutinousRead = false;

    public LivingScreenEditor(LivingDevicesManager ldm)
    {
        this.ldm = ldm;
    }

    public void Draw()
    {
        livingScreen = ldm.livingScreen;
        //title
        GUILayout.BeginHorizontal();
        GUILayout.Button("LivingScreen [" + livingScreen.getSerial().getPort() + "]", "OL Title");
        GUILayout.EndHorizontal();

        // Global box
        GUILayout.BeginVertical("Box");

        GUILayout.BeginHorizontal("Box");
        angle = EditorGUILayout.IntField("Rotation angle", angle);
        if (GUILayout.Button("Rotate screen"))
        {
            livingScreen.Rotate(angle);
            if (autoRead) Read();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("Box");
        position = EditorGUILayout.IntField("Move to", position);
        if (GUILayout.Button("Move screen"))
        {
            livingScreen.MoveTo(position);
            if (autoRead) Read();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginVertical("Box");
        distance = EditorGUILayout.IntField("Translate distance", distance);
        direction = (LivingScreen.Direction)EditorGUILayout.EnumPopup("Translate direction", direction);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Translate screen"))
        {
            livingScreen.Translate(distance, direction);
            if (autoRead) Read();
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Skake screen"))
        {
            livingScreen.ShakeScreen(distance);
            if (autoRead) Read();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.BeginHorizontal("Box");
        if (GUILayout.Button("Calibrate"))
        {
            livingScreen.Calibrate();
            if (autoRead) Read();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("Box");
        if (GUILayout.Button("Read arduino"))
        {
            Read(10);
        }
        autoRead = EditorGUILayout.Toggle("Auto", autoRead);
       // coutinousRead = EditorGUILayout.Toggle("Continous", coutinousRead);
        GUILayout.EndHorizontal();

        if (coutinousRead) Debug.Log(livingScreen.ReadFromArduino(10));

        // end global box
        GUILayout.EndVertical();
    }

    void Read(int timeout = 100)
    {
            Debug.Log(livingScreen.ReadFromArduino(timeout));
    }
}
