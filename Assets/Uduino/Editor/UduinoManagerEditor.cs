using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Uduino;


[SerializeField]
public class Pin
{
    //Pin stuff
    public enum PinMode
    {
        Output,
        PWM,
        Analog,
        Input_pullup,
        Servo
    }

    private UduinoManagerEditor manager;

    public string arduino = "";
    public string lastReadValue = "";

    private string[] allPin = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "A0", "A1", "A2", "A3", "A4", "A5" };

    public PinMode pinMode = PinMode.Output;
    private PinMode prevPinMode = PinMode.Output;

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
        pinMode = (PinMode)EditorGUILayout.EnumPopup(pinMode, GUILayout.Width(100f));
        CheckChanges();
        GUILayout.BeginHorizontal();

        switch (pinMode)
        {
            case PinMode.Output:
                if (GUILayout.Button("HIGH", EditorStyles.miniButtonLeft)) sendValue = 255;
                if (GUILayout.Button("LOW", EditorStyles.miniButtonRight)) sendValue = 0;
                break;
            case PinMode.Input_pullup:
                EditorGUILayout.LabelField("Digital read:", GUILayout.MaxWidth(100f));
                break;
            case PinMode.PWM:
                sendValue = EditorGUILayout.IntSlider(sendValue, 0, 255);
                break;
            case PinMode.Servo:
                sendValue = EditorGUILayout.IntSlider(sendValue, 0, 180);
                break;
            case PinMode.Analog:
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

    UduinoManager manager = null;

    string message = "";
    string newBlackListedPort = "";

    LogLevel debugLevel;

    List<Pin> pins = new List<Pin>();


    bool defaultPanel = true;
    bool arduinoPanel = true;
    bool advancedPanel = true;
    bool blacklistedFoldout = true;

    //Style-related
    Color headerColor = new Color(0.65f, 0.65f, 0.65f, 1);
    //Color backgroundColor = new Color(0.75f, 0.75f, 0.75f);
    Color defaultButtonColor;

    GUIStyle boldtext;

    void OnEnable()
    {
        defaultButtonColor = GUI.backgroundColor;
        Instance = this;
        Repaint();
      //  manager.DiscoverPorts();
    }

    public override void OnInspectorGUI()
    {
        if (manager == null) manager = (UduinoManager)target;
        Log.SetLogLevel(manager.debugLevel);


        //Set the Style
        if (!EditorGUIUtility.isProSkin)
        {
            headerColor = new Color(165 / 255f, 165 / 255f, 165 / 255f, 1);
          //  backgroundColor = new Color(193 / 255f, 193 / 255f, 193 / 255f, 1);
        }
        else
        {
            headerColor = new Color(41 / 255f, 41 / 255f, 41 / 255f, 1);
        //    backgroundColor = new Color(56 / 255f, 56 / 255f, 56 / 255f, 1);
        }


        boldtext = new GUIStyle(GUI.skin.label);
        boldtext.fontStyle = FontStyle.Bold;
        boldtext.alignment = TextAnchor.UpperCenter;

        DrawLogo();

        defaultPanel = DrawHeaderTitle("Uduino Settings", defaultPanel, headerColor);
        if (defaultPanel)
        {
            GUILayout.Label("General", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            manager.debugLevel = (LogLevel)EditorGUILayout.EnumPopup("Log Level", manager.debugLevel);
            EditorGUI.indentLevel--;
            GUILayout.Label("Arduino", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            manager.BaudRate = EditorGUILayout.IntField("Baud Rate", manager.BaudRate );
            manager.ReadOnThread = EditorGUILayout.Toggle("Read on threads", manager.ReadOnThread);
            EditorGUI.indentLevel--;

            EditorGUILayout.Separator();
        }

        arduinoPanel = DrawHeaderTitle("Adruino", arduinoPanel, headerColor);
        if (arduinoPanel)
        {
            ArduinoSetings();
        }

        advancedPanel = DrawHeaderTitle("Advanced", advancedPanel, headerColor);
        if (advancedPanel)
        {
            AdvancedSettings();
        }

        //TODO : Needed to update when message sent/received. This uses a lot of passes. Maybe change that, do a variable to check if a new value is here
        EditorUtility.SetDirty(target);
    }

    public void ArduinoSetings()
    {
        if (manager.uduinoDevices.Count == 0)
        {
            SetGUIBackgroundColor("#ef5350");
            GUILayout.BeginVertical("Box", GUILayout.ExpandWidth(true));
            GUILayout.Label("No Arduino connected", boldtext);
            GUILayout.EndVertical();
            SetGUIBackgroundColor();
       }
       else
       {
            //TODO : Compact that in another function
            foreach (KeyValuePair<string, UduinoDevice> uduino in manager.uduinoDevices)
            {
                SetGUIBackgroundColor("#4FC3F7");
                GUILayout.BeginVertical("Box",  GUILayout.ExpandWidth(true));
                GUILayout.Label(uduino.Key, boldtext);
                GUILayout.EndVertical();
                SetGUIBackgroundColor();

                GUILayout.Label("Board informations", EditorStyles.boldLabel);

                GUILayout.BeginVertical("Box");
                EditorGUILayout.TextField("Last read message", uduino.Value.lastRead);
                EditorGUILayout.TextField("Last sent value", uduino.Value.lastWrite);
                //Todo: auto read
                GUILayout.EndVertical();

                GUILayout.Label("Send commands", EditorStyles.boldLabel);

                GUILayout.BeginVertical("Box");
                if (uduino.Key == "testBoard") // Display the informations for testBoard
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Pin", "OL Titleleft", GUILayout.MaxWidth(56f));
                    GUILayout.Label("Mode", "OL Titlemid", GUILayout.MaxWidth(105f));
                    GUILayout.Label("Action", "OL Titlemid", GUILayout.ExpandWidth(true));
                    GUILayout.Label("×", "OL Titleright", GUILayout.MaxWidth(25f));
                    GUILayout.EndHorizontal();

                    if (pins.Count == 0)
                    {

                    }
                    else
                    {

                    }


                    foreach (Pin pin in pins.ToArray())
                    {
                        GUILayout.BeginVertical("TE Toolbar", GUILayout.Height(105f));
                        pin.Draw();
                        GUILayout.EndVertical();
                    }


                    if (GUILayout.Button("Test a pin", "TE toolbarbutton", GUILayout.ExpandWidth(true)))
                    {
                        pins.Add(new Pin(this, uduino.Key));
                    }
                }
                else // If it's a "Normal" Arduino
                {
                    message = EditorGUILayout.TextField("Command to send", message);
                    if (GUILayout.Button("Send command"))
                    {
                        manager.Write(uduino.Key, message);
                        manager.Read(uduino.Key);
                    }
                }
                GUILayout.EndVertical();

            }
        }

        DrawLine(12,0,45);

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();
        SetGUIBackgroundColor("#4FC3F7");
        if (GUILayout.Button("Discover ports"))
        {
            manager.DiscoverPorts();
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        SetGUIBackgroundColor("#ef5350");
        if (GUILayout.Button("Close ports"))
        {
            manager.CloseAllPorts();
        }
        SetGUIBackgroundColor();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        EditorGUILayout.Separator();

    }

    public void AdvancedSettings()
    {
        GUILayout.BeginVertical();
        GUILayout.Label("Discovery settings", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        manager.DiscoverTries = EditorGUILayout.IntField("Discovery tries", manager.DiscoverTries);

        blacklistedFoldout = Foldout(blacklistedFoldout, "Blacklisted ports", true, EditorStyles.foldout);
        if (blacklistedFoldout)
        {

            GUILayout.BeginVertical();



            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUI.indentLevel * 15 + 4); ;

            GUILayout.Label("Serial port", "OL Titleleft", GUILayout.Width(Screen.width / 1.5f));
            GUILayout.Label("Action", "OL Titleleft");
            GUILayout.EndHorizontal();

            foreach (string blackList in manager.BlackListedPorts)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * 15 + 4);
                GUILayout.Label(blackList,"OL Titleleft", GUILayout.Width(Screen.width / 1.5f));
                if (GUILayout.Button("-", "OL Titleright"))
                {
                    manager.BlackListedPorts.Remove(blackList);
                    return;
                }
                GUILayout.EndHorizontal();

            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUI.indentLevel * 15 + 4);
            EditorGUI.indentLevel--;

            newBlackListedPort = EditorGUILayout.TextField("", "TE toolbar", newBlackListedPort, GUILayout.Width(Screen.width/1.5f));
            if (GUILayout.Button("Add", "TE Toolbarbutton", GUILayout.ExpandWidth(true)))
            {
                if (newBlackListedPort == "") return;
                manager.BlackListedPorts.Add(newBlackListedPort);
            }
            GUILayout.EndHorizontal();

           GUILayout.EndVertical();

        }

        GUILayout.EndVertical();


        GUILayout.BeginVertical();
        GUILayout.Label("Discovery settings", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Get port state"))
        {
            manager.GetPortState();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear console"))
        {
            var logEntries = System.Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
            var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            clearMethod.Invoke(null, null);
        }
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }


    public static bool Foldout(bool foldout, GUIContent content, bool toggleOnLabelClick, GUIStyle style)
    {
        Rect position = GUILayoutUtility.GetRect(40f, 40f, 16f, 16f, style);
        // EditorGUI.kNumberW == 40f but is internal
        return EditorGUI.Foldout(position, foldout, content, toggleOnLabelClick, style);
    }

    public static bool Foldout(bool foldout, string content, bool toggleOnLabelClick, GUIStyle style)
    {
        return Foldout(foldout, new GUIContent(content), toggleOnLabelClick, style);
    }

    public void DrawLine(int marginTop, int marginBottom, int height)
    {
        EditorGUILayout.Separator();
        GUILayout.Space(marginTop);
        Rect lastRect = GUILayoutUtility.GetLastRect();
        GUI.Box(new Rect(0f, lastRect.y + 4, Screen.width, height),"");
        GUILayout.Space(marginBottom);
    }

    public void DrawLogo()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace(); // Nededed for lastRect
        EditorGUILayout.EndHorizontal();

        Texture tex = (Texture)EditorGUIUtility.Load("Assets/Uduino/Editor/Resources/logo.png");
        GUILayout.Space(0);
        Rect lastRect = GUILayoutUtility.GetLastRect();
        GUI.Box(new Rect(1, lastRect.y + 4, Screen.width, 27), tex);
        lastRect = GUILayoutUtility.GetLastRect();
        Color bgColor = new Color();
        ColorUtility.TryParseHtmlString("#2EB4BE", out bgColor);
        EditorGUI.DrawRect(new Rect(lastRect.x - 15, lastRect.y + 5f, Screen.width + 1, 80f), bgColor);
        GUI.DrawTexture(new Rect(lastRect.x - 15, lastRect.y + 5f, Screen.width + 1, 80f), tex, ScaleMode.ScaleToFit);
        GUI.color = Color.white;
        GUILayout.Space(80f);
    }

    public bool DrawHeaderTitle(string title, bool foldoutProperty, Color backgroundColor)
    {
        GUILayout.Space(0);
        Rect lastRect = GUILayoutUtility.GetLastRect();
        GUI.Box(new Rect(1, lastRect.y + 4, Screen.width, 27), "");
        lastRect = GUILayoutUtility.GetLastRect();
        EditorGUI.DrawRect(new Rect(lastRect.x - 15, lastRect.y + 5f, Screen.width + 1, 25f), headerColor);
        GUI.Label(new Rect(lastRect.x, lastRect.y + 10, Screen.width, 25), title);
        GUI.color = Color.clear;
        if (GUI.Button(new Rect(0, lastRect.y + 4, Screen.width, 27), ""))
        {
            foldoutProperty = !foldoutProperty;
        }
        GUI.color = Color.white;
        GUILayout.Space(30);
        if (foldoutProperty) { GUILayout.Space(5); }
        return foldoutProperty;
    }

    void SetGUIBackgroundColor(string hex)
    {
        Color color = new Color();
        ColorUtility.TryParseHtmlString(hex, out color);
        GUI.backgroundColor = color;
    }

    void SetGUIBackgroundColor(Color color)
    {
        GUI.backgroundColor = color;
    }
    void SetGUIBackgroundColor()
    {
        GUI.backgroundColor = defaultButtonColor;
    }

    public void SendMessage(string targetBoard, string message)
    {
        manager.Write(targetBoard, message);
    }

    public void RemovePin(Pin pin)
    {
        pin.Destroy();
        pins.Remove(pin);
    }
}