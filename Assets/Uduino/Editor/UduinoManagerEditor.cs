using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Uduino;


[SerializeField]
public class Pin
{
    private UduinoManagerEditor manager;

    public string arduino = "";
    public string lastReadValue = "";

    private string[] allPin = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "A0", "A1", "A2", "A3", "A4", "A5" };

    public UduinoPanel.PinMode pinMode = UduinoPanel.PinMode.Output;
    private UduinoPanel.PinMode prevPinMode = UduinoPanel.PinMode.Output;

    public int currentPin = 13;
    private int prevPin = 13;

    public int sendValue = 0;
    private int prevSendValue = 0;


    public Pin(UduinoManagerEditor m, string arduinoParent)
    {
        manager = m;
        arduino = arduinoParent;
        SendMessage("setMode " + currentPin + " " + (int)pinMode);

    }

    void SendMessage(string message)
    {
        manager.SendMessage(arduino, message);
    }

    void CheckChanges()
    {
        if(currentPin != prevPin)
        {
            SendMessage("setMode " + currentPin + " " + (int)pinMode);
            prevPin = currentPin;
        }

        if(pinMode != prevPinMode)
        {
            SendMessage("setMode "+ currentPin + " " + (int)pinMode);
            prevPinMode = pinMode;
        }
    }

    public void Draw()
    {
        GUILayout.BeginHorizontal();
        currentPin = EditorGUILayout.Popup(currentPin, allPin, GUILayout.Width(50f));
        pinMode = (UduinoPanel.PinMode)EditorGUILayout.EnumPopup(pinMode, GUILayout.Width(100f));
        CheckChanges();
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
        //Send  the message
        if(prevSendValue != sendValue)
        {
            SendMessage("writePin " + currentPin + " " + sendValue);
            prevSendValue = sendValue;
        }

        GUILayout.EndHorizontal();
    }

    public void Destroy()
    {
        SendMessage("writePin " + currentPin + " 0");
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

    bool test = true;

    Color headerColor = new Color(0.65f, 0.65f, 0.65f, 1);
    Color backgroundColor = new Color(0.75f, 0.75f, 0.75f);


    public override void OnInspectorGUI()
    {
        if (manager == null) manager = (UduinoManager)target;
        Log.SetLogLevel(manager.debugLevel);

        DrawFullInspector();


        //Colors
        if (!EditorGUIUtility.isProSkin)
        {
            headerColor = new Color(165 / 255f, 165 / 255f, 165 / 255f, 1);
            backgroundColor = new Color(193 / 255f, 193 / 255f, 193 / 255f, 1);
        }
        else
        {
            headerColor = new Color(41 / 255f, 41 / 255f, 41 / 255f, 1);
            backgroundColor = new Color(56 / 255f, 56 / 255f, 56 / 255f, 1);
        }


        test = DrawHeaderTitle("Tool Properties", test, headerColor);
        if (test)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Paint Mode: ", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
        }


        EditorGUILayout.BeginVertical();
        GUILayout.Space(5);
        EditorGUILayout.BeginVertical("HelpBox");
        EditorGUILayout.BeginHorizontal("HelpBox");
        GUILayout.FlexibleSpace();
        GUILayout.Label("GUISkin - " , EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();


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


    public void DrawHeaderTitle(string title, Color backgroundColor)
    {
        GUILayout.Space(0);
        GUI.color = backgroundColor;
        GUI.Box(new Rect(1, GUILayoutUtility.GetLastRect().y + 4, 100.0f, 27), "");
        GUI.color = Color.white;
        GUILayout.Space(4);
        GUILayout.Label(title, EditorStyles.largeLabel);
    }

    public bool DrawHeaderTitle(string title, bool foldoutProperty, Color backgroundColor)
    {
        GUILayout.Space(0);
        Rect lastRect = GUILayoutUtility.GetLastRect();
        GUI.Box(new Rect(1, lastRect.y + 4, 100.0f, 27), "");
        lastRect = GUILayoutUtility.GetLastRect();
        EditorGUI.DrawRect(new Rect(lastRect.x, lastRect.y + 5f, 100.0f + 1, 25f), headerColor);
        GUI.Label(new Rect(lastRect.x, lastRect.y + 10, 100.0f, 25), title);
        GUI.color = Color.clear;
        if (GUI.Button(new Rect(0, lastRect.y + 4, 100.0f, 27), ""))
        {
            foldoutProperty = !foldoutProperty;
        }
        GUI.color = Color.white;
        GUILayout.Space(30);
        if (foldoutProperty) { GUILayout.Space(5); }
        return foldoutProperty;
    }


    public void DrawFullInspector()
    {

        DrawDefaultInspector();

        EditorGUILayout.Separator();

        if (manager.uduinoDevices.Count == 0)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Button("No arduino connected", "MeTransitionBlock");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            GUILayout.Button("No arduino connected", "GroupBox");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Button("No arduino connected", "GroupBox");

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Button("No arduino connected", "MeTransOffLeft");
            GUILayout.Button("No arduino connected", "MeTransOffRight");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Button("No arduino connected", "ObjectFieldThumb");

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Button("No arduino connected", "PreToolbar");

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Button("No arduino connected", "ProfilerBadge");

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Button("No arduino connected", "ProgressBarBack");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Button("No arduino connected", "ProjectBrowserHeaderBgMiddle");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Button("No arduino connected", "RectangleToolHBar");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Button("No arduino connected", "ShurikenEmitterTitle");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Button("No arduino connected", "SelectionRect");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Button("No arduino connected", "RectangleToolSelection");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Button("No arduino connected", "RectangleToolSelection");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Button("No arduino connected", "WindowBackground");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Button("No arduino connected", "U2D.createRect");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Button("No arduino connected", "TE toolbarbutton");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Button("No arduino connected", "ShurikenModuleTitle");
            GUILayout.EndHorizontal();


        }
        else
        {
            GUILayout.BeginHorizontal();
            GUILayout.Button(manager.uduinoDevices.Count + " Arduino connected", "button");
            GUILayout.EndHorizontal();

            //TODO : Compact that in another function
            foreach (KeyValuePair<string, UduinoDevice> uduino in manager.uduinoDevices)
            {
                GUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("Arduino Name", uduino.Key);
                EditorGUILayout.LabelField("Last read message", uduino.Value.lastRead);
                EditorGUILayout.LabelField("Last sent value", uduino.Value.lastWrite);
                //Todo: auto read
                GUILayout.EndVertical();


                EditorGUILayout.Separator();
                GUILayout.BeginHorizontal();
                GUILayout.Button("Pin", "OL Box", GUILayout.MaxWidth(56f));
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
                    pins.Add(new Pin(this, uduino.Key));
                }

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
        if (GUILayout.Button("Writeread Arduino"))
        {
            manager.Write(targetName, message);
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

        //TODO : Needed to update when message sent/received. This uses a lot of passes. Maybe change that, do a variable to check if a new value is here
        EditorUtility.SetDirty(target);
    }

    public void SendMessage(string targetBoard, string message)
    {
        manager.Write(targetBoard, message);
    }

    public void DrawPanelMessage()
    {

    }

    public void RemovePin(Pin pin)
    {
        pin.Destroy();
        pins.Remove(pin);
    }

}