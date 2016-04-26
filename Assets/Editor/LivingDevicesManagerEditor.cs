using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(LivingDevicesManager))]
public class LivingDevicesManagerEditor : Editor 
{
    public string targetName;
    public string message;
    LivingDevicesManager ldm = null;
    LivingKeyboardEditor lk = null;
    LivingScreenEditor ls = null;
    LivingMouseEditor lm = null;

    public override void OnInspectorGUI()
    {
        if (ldm == null) ldm = (LivingDevicesManager)target;
        if (lk == null) lk = new LivingKeyboardEditor(ldm);
        if (lm == null) lm = new LivingMouseEditor(ldm);
        if (ls == null) ls = new LivingScreenEditor(ldm);

        DrawDefaultInspector();

        EditorGUILayout.Separator();

        //http://blog.sina.com.cn/s/blog_647422b90101de8x.html
           
        if(ldm.livingDesktopDevices.Count == 0) {
            GUILayout.BeginHorizontal();
            GUILayout.Button("No arduino connected", "OL Title");
            GUILayout.EndHorizontal();
        }
        else
        {
            foreach (KeyValuePair<string, LivingDevice> liv in ldm.livingDesktopDevices)
            {
                string device = liv.Key.ToString();

                switch (device)
                {
                    case "LivingKeyboard":
                        lk.Draw();
                        break;
                    case "LivingMouse":
                        lm.Draw();
                        break;
                    case "LivingScreen":
                        ls.Draw();
                        break;
                }
            }
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
            ldm.SendTestMessage(targetName, message);
        }
        GUILayout.EndVertical();


        EditorGUILayout.Separator();
        GUILayout.BeginHorizontal();
        GUILayout.Button("Advanced", "OL Title");
        GUILayout.EndHorizontal();
        GUILayout.BeginVertical("Box");
      
        if (GUILayout.Button("Discover ports"))
        {
            ldm.DiscoverPorts();
        }
        if (GUILayout.Button("Get port state"))
        {
            ldm.GetPortState();
        }
        if (GUILayout.Button("Close ports"))
        {
            ldm.CloseAllPorts();
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
