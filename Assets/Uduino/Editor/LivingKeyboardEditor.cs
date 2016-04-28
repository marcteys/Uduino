using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(LivingDevicesManager))]
public class LivingKeyboardEditor : Editor
{
    LivingKeyboard.Tilt tilt;
    private int angle;
    LivingKeyboard.Direction direction;
    private int distance;
    LivingDevicesManager ldm = null;
    LivingKeyboard livingKeyboard = null;

    bool autoRead = false;

   public LivingKeyboardEditor(LivingDevicesManager ldm) 
    {
       this.ldm = ldm;
    }


   public void Draw()
   {
       livingKeyboard = ldm.livingKeyboard;
       //title
       GUILayout.BeginHorizontal();
       GUILayout.Button("LivingKeyboard [" + livingKeyboard.getSerial().getPort() + "]", "OL Title");
       GUILayout.EndHorizontal();

       // Global box
       GUILayout.BeginVertical("Box");


       GUILayout.BeginVertical("Box");
       angle = EditorGUILayout.IntField("Rotation angle", angle);
       tilt = (LivingKeyboard.Tilt)EditorGUILayout.EnumPopup("Rotation tilt", tilt);
       GUILayout.BeginHorizontal();
       if (GUILayout.Button("Rotate Keyboard"))
       {
           livingKeyboard.Rotate(angle, tilt);
           if (autoRead) Read();
       }
       GUILayout.EndHorizontal();
       GUILayout.EndVertical();


       GUILayout.BeginVertical("Box");
       distance = EditorGUILayout.IntField("Translate distance", distance);
       direction = (LivingKeyboard.Direction)EditorGUILayout.EnumPopup("Translate direction", direction);
       GUILayout.BeginHorizontal();
       if (GUILayout.Button("Translate Keyboard"))
       {
           livingKeyboard.Translate(distance, direction);
           if (autoRead) Read();
       }
       GUILayout.EndHorizontal();
       GUILayout.EndVertical();


       GUILayout.BeginHorizontal("Box");
       if (GUILayout.Button("Calibrate"))
       {
           livingKeyboard.Calibrate();
           if (autoRead) Read();
       }
       GUILayout.EndHorizontal();

       GUILayout.BeginHorizontal("Box");
       if (GUILayout.Button("Read arduino"))
       {
           Read();
       }
       autoRead = EditorGUILayout.Toggle("Auto", autoRead);
       GUILayout.EndHorizontal();

       // end global box
       GUILayout.EndVertical();
   }

   void Read(int timeout = 100)
   {
           Debug.Log(livingKeyboard.ReadFromArduino(timeout));
   }
}