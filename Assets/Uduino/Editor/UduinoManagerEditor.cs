using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Uduino;

[CustomEditor(typeof(UduinoManager))]
public class UduinoManagerEditor : Editor {

	public string targetName = "myArduinoName";
    public string message = "";
    LogLevel debugLevel;

    UduinoManager manager = null;

    void OnEnable()
    {
       // UduinoManager.debugLevel = debugLevel;
       // Debug.Log("caca");
    }

    public override void OnInspectorGUI()
    {
        if (manager == null) manager = (UduinoManager)target;

        DrawDefaultInspector();

        Log.SetLogLevel(manager.debugLevel);

        EditorGUILayout.Separator();


        if (manager.uduinoDevices.Count == 0)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Button("No arduino connected", "OL Title");
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.BeginHorizontal();
            GUILayout.Button(manager.uduinoDevices.Count + " Arduino connected", "OL Title");
            GUILayout.EndHorizontal();

            foreach (KeyValuePair<string, UduinoDevice> uduino in manager.uduinoDevices)
            {
                GUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("Arduino Name", uduino.Key);
                EditorGUILayout.LabelField("Last message", uduino.Value.read);
                EditorGUILayout.LabelField("Last sent value", uduino.Value.write);
                GUILayout.EndVertical();
            }
        }

        EditorGUILayout.Separator();
        GUILayout.BeginHorizontal();
        GUILayout.Button("Debug", "OL Title");
        GUILayout.EndHorizontal();
        GUILayout.BeginVertical("Box");
        targetName = EditorGUILayout.TextField("Arduino Name", targetName);
        message = EditorGUILayout.TextField("Test message", message);
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

        if (GUI.changed)
            EditorUtility.SetDirty(target);

    }

}