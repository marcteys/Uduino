using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Uduino;

[CustomEditor(typeof(UduinoManager))]
public class UduinoManagerEditor : Editor {

    public string targetName = "myArduino";
    public string message = "PING";
    UduinoManager manager = null;

    public override void OnInspectorGUI()
    {
        if (manager == null) manager = (UduinoManager)target;

        DrawDefaultInspector();

        EditorGUILayout.Separator();

        //http://blog.sina.com.cn/s/blog_647422b90101de8x.html

        if (manager.uduinoDevices.Count == 0)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Button("No arduino connected", "OL Title");
            GUILayout.EndHorizontal();
        }
      


        EditorGUILayout.Separator();
        GUILayout.BeginHorizontal();
        GUILayout.Button("Debug", "OL Title");
        GUILayout.EndHorizontal();
        GUILayout.BeginVertical("Box");
        targetName = EditorGUILayout.TextField("Object Name: ", targetName);
        message = EditorGUILayout.TextField("Test message: ", message);
        if (GUILayout.Button("Send test message"))
        {
            manager.Write(targetName, message);
        }
        if (GUILayout.Button("Read Arduino"))
        {
           manager.Read(targetName);
        }
        GUILayout.EndVertical();


        EditorGUILayout.Separator();
        GUILayout.BeginHorizontal();
        GUILayout.Button("Advanced", "OL Title");
        GUILayout.EndHorizontal();
        GUILayout.BeginVertical("Box");

        if (GUILayout.Button("Discover ports"))
        {
            manager.DiscoverPorts();
        }
        if (GUILayout.Button("Get port state"))
        {
            manager.GetPortState();
        }
        if (GUILayout.Button("Close ports"))
        {
            manager.CloseAllPorts();
        }
        EditorGUILayout.Separator();
        if (GUILayout.Button("Clear console"))
        {
            var logEntries = System.Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
            var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            clearMethod.Invoke(null, null);
        }
        GUILayout.EndVertical();


    }

}