using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Uduino;


[SerializeField]
public class Pin
{
    public UduinoPanel.PinMode pinMode = UduinoPanel.PinMode.Output;
    public int sendValue = 0;
    public string arduino = "";
    public string lastReadValue = "";

    private int currentPin = 13;
    private string[] allPin = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "A0", "A1", "A2", "A3", "A4", "A5" };

    public Pin(string arduinoParent)
    {

    }

    public void Draw()
    {
        GUILayout.BeginHorizontal();
        currentPin = EditorGUILayout.Popup(currentPin, allPin, GUILayout.Width(50f));
        pinMode = (UduinoPanel.PinMode)EditorGUILayout.EnumPopup(pinMode, GUILayout.Width(100f));
        GUILayout.BeginHorizontal();

        switch (pinMode)
        {
            case UduinoPanel.PinMode.Output:
                if (GUILayout.Button("HIGH", EditorStyles.miniButtonLeft)) sendValue = 255;
                if (GUILayout.Button("LOW", EditorStyles.miniButtonRight)) sendValue = 0;
                break;
            case UduinoPanel.PinMode.Input:
                EditorGUILayout.LabelField("Digital read:", GUILayout.MaxWidth(100f));
                break;
            case UduinoPanel.PinMode.PWM:
                sendValue = EditorGUILayout.IntSlider(sendValue, 0, 255);
                break;
            case UduinoPanel.PinMode.Analog:
                EditorGUILayout.LabelField("Analog read:", GUILayout.MaxWidth(100f));
                break;
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.MaxWidth(20f)))
        {
            UduinoManagerEditor.Instance.RemovePin(this);
        }

        GUILayout.EndHorizontal();
    }
}




[CustomEditor(typeof(UduinoManager))]
public class UduinoManagerEditor : Editor {

    public static UduinoManagerEditor Instance { get; private set; }
    public static bool IsOpen
    {
        get { return Instance != null; }
    }
    public string targetName = "myArduinoName";
    public string message = "";
    LogLevel debugLevel;

    UduinoManager manager = null;

    public List<Pin> pins = new List<Pin>();


    void OnEnable()
    {
        Instance = this;
        Repaint();
    }

    public override void OnInspectorGUI()
    {
        if (manager == null) manager = (UduinoManager)target;
        Log.SetLogLevel(manager.debugLevel);



        DrawFullInspector();

        EditorGUILayout.Separator();
        GUILayout.BeginHorizontal();
        GUILayout.Button("Pin", "OL Title", GUILayout.MaxWidth(56f));
        GUILayout.Button("Mode", "OL Title", GUILayout.MaxWidth(105f));
        GUILayout.Button("Action", "OL Title");
        GUILayout.Button("×", "OL Title", GUILayout.MaxWidth(25f));
        GUILayout.EndHorizontal();

        foreach (Pin pin in pins.ToArray())
        {
            pin.Draw();
        }

        if (GUILayout.Button("Test a pin"))
        {
            pins.Add(new Pin("lol"));
        }

        GUILayout.BeginVertical();

        EditorGUILayout.LabelField("Arduino Name");
        GUILayout.BeginVertical();

        EditorGUILayout.LabelField("Last message");
        GUILayout.EndVertical();

        EditorGUILayout.LabelField("Last sent value");
        GUILayout.EndVertical();

        /*
        if (!UduinoPanel.IsOpen)
            {
            DrawFullInspector();

        }
        else
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Panel open");
                GUILayout.EndHorizontal();
                //   EditorUtility.SetDirty(target);
            }
            */
        /*
        if (GUI.changed)
            EditorUtility.SetDirty(target);*/
    }

  

    public void DrawFullInspector()
    {

        DrawDefaultInspector();

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


        if (GUILayout.Button("Open Uduino Panel"))
        {
            UduinoPanel window = (UduinoPanel)EditorWindow.GetWindow(typeof(UduinoPanel));
            window.Init();
            return;
        }
        EditorGUILayout.Separator();

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

    public void DrawPanelMessage()
    {

    }

    public void RemovePin(Pin pin)
    {
        pins.Remove(pin);
        /*
        pins.Find(pin);
        for (int i = pins.Count; i <= 0 ;i--)
        {
          //  (RemoveAt(position);

        }*/
    }

}