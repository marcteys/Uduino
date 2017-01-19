using UnityEngine;
using System.Net;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Uduino;

[SerializeField]
public class EditorPin : Pin
{
    UduinoManagerEditor editorManager = null;

    private string[] allPin = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "A0", "A1", "A2", "A3", "A4", "A5" };

    public int sendValue = 0;

    private int prevPin = -1;

    public EditorPin(string arduinoParent, int pin, PinMode mode, UduinoManagerEditor m)
            : base(arduinoParent, pin, mode)
    {
        editorManager = m;
        arduinoName = arduinoParent;
        currentPin = pin;
        ChangePinMode(mode);
    }

    public override void Draw()
    {
        #if UNITY_EDITOR
        GUILayout.BeginHorizontal();
        currentPin = EditorGUILayout.Popup(currentPin, allPin, "ToolbarDropDown", GUILayout.MaxWidth(40));
        pinMode = (PinMode)EditorGUILayout.EnumPopup(pinMode, "ToolbarDropDown", GUILayout.MaxWidth(55));
        CheckChanges();
        GUILayout.BeginHorizontal();

        EditorGUIUtility.fieldWidth -= 22;

        switch (pinMode)
        {
            case PinMode.Output:
                if (GUILayout.Button("HIGH", "toolbarButton")) sendValue = 255;
                if (GUILayout.Button("LOW", "toolbarButton")) sendValue = 0;
                break;
            case PinMode.Input_pullup:
                if (GUILayout.Button("Read", "toolbarButton", GUILayout.MaxWidth(55)))
                    SendRead();
                GUILayout.Label(lastReadValue.ToString(), "TE Toolbarbutton");
                break;
            case PinMode.PWM:
                GUILayout.BeginHorizontal("TE Toolbarbutton");
                sendValue = EditorGUILayout.IntSlider(sendValue, 0, 255);
                GUILayout.EndHorizontal();
                break;
            case PinMode.Servo:
                GUILayout.BeginHorizontal("TE Toolbarbutton");
                sendValue = EditorGUILayout.IntSlider(sendValue, 0, 180);
                GUILayout.EndHorizontal();
                break;
            case PinMode.Analog:
                if(GUILayout.Button("Read", "toolbarButton", GUILayout.MaxWidth(55)))
                        SendRead();
                GUILayout.Label(lastReadValue.ToString(), "TE Toolbarbutton");
                break;
        }
        EditorGUIUtility.fieldWidth += 22;

        if (GUILayout.Button("-", "toolbarButton", GUILayout.Width(22)))
        {
            UduinoManagerEditor.Instance.RemovePin(this);
        }
        GUILayout.EndHorizontal();

        //Send  the message
        if (prevSendValue != sendValue)
        {
            WriteMessage("w " + currentPin + " " + sendValue);
            prevSendValue = sendValue;
        }

        GUILayout.EndHorizontal();
        #endif
    }

    public override void SendRead()
    {
        if (editorManager != null)
        {
            editorManager.Read(arduinoName, "r " + currentPin, action: ParseReadData);
        }
    }

    void CheckChanges()
    {
        //If it's playing, cange the values
        if(Application.isPlaying)
        {
            foreach (Pin pinTarget in UduinoManager.Instance.pins)
            {
                if (pinTarget.PinTargetExists(arduinoName, currentPin))
                {
                    if (pinMode != prevPinMode)
                        pinTarget.ChangePinMode(pinMode);
                }
            }
        }

        if (currentPin != prevPin && currentPin != -1)
        {
            WriteMessage("s " + currentPin + " " + (int)pinMode);
            prevPin = currentPin;
        }

        if (pinMode != prevPinMode)
        {
            WriteMessage("s " + currentPin + " " + (int)pinMode);
            prevPinMode = pinMode;
        }
    }

    public override void WriteMessage(string message, string bundle = null)
    {
        if (editorManager != null) editorManager.WriteMessage(arduinoName, message);
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
    bool advancedPanel = false;
    bool blacklistedFoldout = false;

    //Style-related
    Color headerColor = new Color(0.65f, 0.65f, 0.65f, 1);
    //Color backgroundColor = new Color(0.75f, 0.75f, 0.75f);
    Color defaultButtonColor;

    GUIStyle boldtext = null;
    GUIStyle olLight = null;
    GUIStyle olInput = null;

    //Setings - Todo : could do better
    bool isUpToDate = false;
    bool isUpToDateChecked = false;

    // Settings
    public string[] baudRates = new string[] { "4800", "9600", "19200", "57600", "115200" };
    int prevBaudRateIndex = 1;
    public int baudRateIndex = 1;

    bool limitSendRate = false;


    void OnEnable()
    {
        Instance = this;
    }

    void SetColorAndStyles()
    {
        if(boldtext == null)
        {
            //Color and GUI
            defaultButtonColor = GUI.backgroundColor;
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
            
            olLight = new GUIStyle("OL Titleleft");
            olLight.fontStyle = FontStyle.Normal;
            olLight.font = GUI.skin.button.font;
            olLight.fontSize = 9;
            olLight.alignment = TextAnchor.MiddleCenter;

            olInput = new GUIStyle("TE toolbar");
            olInput.fontStyle = FontStyle.Bold;
            olInput.fontSize = 10;
            olInput.alignment = TextAnchor.MiddleLeft;
        }
    }

    public override void OnInspectorGUI()
    {
        if (manager == null)
        {
            manager = (UduinoManager)target;
            baudRateIndex = System.Array.IndexOf(baudRates, manager.BaudRate.ToString());
        }
        Log.SetLogLevel(manager.debugLevel);

        SetColorAndStyles();

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

           // manager.BaudRate = EditorGUILayout.IntField("Baud Rate", manager.BaudRate );
            baudRateIndex  = EditorGUILayout.Popup("Baud Rate", baudRateIndex,  baudRates);
            if(prevBaudRateIndex != baudRateIndex)
            {
                int result = 9600;
                int.TryParse(baudRates[baudRateIndex], out result);
                manager.BaudRate = result;
                prevBaudRateIndex = baudRateIndex;
            }
            
            manager.ReadOnThread = EditorGUILayout.Toggle("Read on threads", manager.ReadOnThread);
            EditorGUI.indentLevel--;

            GUILayout.Label("Messages", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            if(limitSendRate = EditorGUILayout.Toggle("Limit Send Rate", limitSendRate))
            {
                if (limitSendRate)
                    manager.SendRateSpeed = 20;
                else 
                    manager.SendRateSpeed = 0;
            }
            if (limitSendRate)
            {
                manager.SendRateSpeed = EditorGUILayout.IntField("Send Rate speed", manager.SendRateSpeed);
                EditorGUILayout.Separator();
            }
            EditorGUILayout.Toggle("Pack in bundles", false);
            EditorGUILayout.IntField("Send frequency", manager.DiscoverTries);

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

                if (uduino.Key == "uduinoBoard" && Application.isPlaying)
                {
                    GUILayout.Label("Pin active", EditorStyles.boldLabel);
                    GUILayout.BeginVertical("Box");

                    if(UduinoManager.Instance.pins.Count != 0) // If a pin is active
                    { 
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Pin", "OL Titleleft", GUILayout.MaxWidth(40f));
                        GUILayout.Label("Mode", "OL Titlemid", GUILayout.MaxWidth(55f));
                        GUILayout.Label("Status", "OL Titlemid", GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();

                        foreach (Pin pin in UduinoManager.Instance.pins)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(pin.currentPin.ToString(), "toolbarButton", GUILayout.MaxWidth(40f));
                            GUILayout.Label(pin.pinMode.ToString(), "toolbarButton", GUILayout.MaxWidth(55f));
                            GUILayout.BeginHorizontal();

                            EditorGUIUtility.fieldWidth -= 22;

                            switch (pin.pinMode)
                            {
                                case PinMode.Output:
                                    GUILayout.Button("HIGH", "toolbarButton");
                                    GUILayout.Button("LOW", "toolbarButton");
                                    break;
                                case PinMode.Input_pullup:
                                    EditorGUILayout.LabelField("Digital read:");
                                    GUILayout.Label(pin.lastReadValue.ToString(), "TE Toolbarbutton");
                                    break;
                                case PinMode.PWM:
                                    GUILayout.BeginHorizontal("TE Toolbarbutton");
                                    EditorGUILayout.IntSlider(pin.prevSendValue, 0, 255);
                                    GUILayout.EndHorizontal();
                                    break;
                                case PinMode.Servo:
                                    GUILayout.BeginHorizontal("TE Toolbarbutton");
                                    EditorGUILayout.IntSlider(pin.prevSendValue, 0, 180);
                                    GUILayout.EndHorizontal();
                                    break;
                                case PinMode.Analog:
                                    GUILayout.Label(pin.lastReadValue.ToString(), "TE Toolbarbutton");
                                    break;
                            }
                            EditorGUIUtility.fieldWidth += 22;


                            GUILayout.EndHorizontal();

                            GUILayout.EndHorizontal();

                        }
                    }
                    else // if no pins are active
                    {
                        GUILayout.Label("No arduino pins are currently setup by code.");
                    }

                    GUILayout.EndVertical();
                }



                GUILayout.Label("Send commands", EditorStyles.boldLabel);

                GUILayout.BeginVertical("Box");
                if (uduino.Key == "uduinoBoard") // Display the informations for default Uduino Board
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Pin", "OL Titleleft", GUILayout.MaxWidth(40f));
                    GUILayout.Label("Mode", "OL Titlemid", GUILayout.MaxWidth(55f));
                    GUILayout.Label("Action", "OL Titlemid", GUILayout.ExpandWidth(true));
                    GUILayout.Label("×", "OL Titleright", GUILayout.Width(22f));
                    GUILayout.EndHorizontal();

                    foreach (Pin pin in pins.ToArray())
                        pin.Draw();

                    if (GUILayout.Button("Add a pin", "TE toolbarbutton", GUILayout.ExpandWidth(true)))
                        pins.Add(new EditorPin(uduino.Key, 13, PinMode.Output, this));
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
        GUILayout.Label("Discovery settings", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        manager.DiscoverTries = EditorGUILayout.IntField("Number of tries", manager.DiscoverTries);

        blacklistedFoldout = EditorGUI.Foldout(GUILayoutUtility.GetRect(40f, 40f, 16f, 16f, EditorStyles.foldout), blacklistedFoldout, "Blacklisted ports", true, EditorStyles.foldout);
        if (blacklistedFoldout)
        {

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUI.indentLevel * 15 + 4); ;

            GUILayout.Label("Serial port", "OL Titleleft");
            GUILayout.Label("", "OL Titleright", GUILayout.MaxWidth(35));
            GUILayout.EndHorizontal();

            foreach (string blackList in manager.BlackListedPorts)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * 15 + 4);
                GUILayout.Label(blackList, olLight);
                if (GUILayout.Button("×", "OL Titleright", GUILayout.MaxWidth(35)))
                {
                    manager.BlackListedPorts.Remove(blackList);
                    return;
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUI.indentLevel * 15 + 4);
            EditorGUI.indentLevel--;            
            newBlackListedPort = EditorGUILayout.TextField("", newBlackListedPort, olInput, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Add", "TE Toolbarbutton", GUILayout.MaxWidth(35)))
            {
                if (newBlackListedPort == "") return;
                manager.BlackListedPorts.Add(newBlackListedPort);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        GUILayout.Label("Debug", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
      //  SetGUIBackgroundColor("#4FC3F7");
        if (GUILayout.Button("Get port state"))
        {
            manager.GetPortState();
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
      //  SetGUIBackgroundColor("#ef5350");
        if (GUILayout.Button("Clear console"))
        {
            var logEntries = System.Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
            var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            clearMethod.Invoke(null, null);
        }
       // SetGUIBackgroundColor();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();


        GUILayout.Label("Update Uduino", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        EditorGUI.indentLevel--;
        EditorGUI.indentLevel--;
        EditorGUI.indentLevel--;
        EditorGUILayout.HelpBox("Current version: " + UduinoVersion.getVersion(), MessageType.None);

        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        // SetGUIBackgroundColor("#4FC3F7");
        if (GUILayout.Button("Check for update"))
        {
            string url = "http://marcteyssier.com/data/uduino/last-version.txt";
            WebRequest myRequest = WebRequest.Create(url);
            WebResponse myResponse = myRequest.GetResponse();
            Stream myStream = myResponse.GetResponseStream();
            StreamReader myReader = new StreamReader(myStream);
            string s = myReader.ReadToEnd();
            myReader.Close();
            isUpToDateChecked = true;
            if (s == UduinoVersion.getVersion()) isUpToDate = true;
            else isUpToDate = false;
        }

        // SetGUIBackgroundColor();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        if(isUpToDateChecked)
        {
            if (isUpToDate) EditorGUILayout.HelpBox("Uduino is up to date", MessageType.Info, true);
            else EditorGUILayout.HelpBox("Uduino is not up to date. Download the last version on the Asset Store." , MessageType.Error, true);

        }

        EditorGUILayout.Separator();

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

        Texture tex = (Texture)EditorGUIUtility.Load("Assets/Uduino/Editor/Resources/uduino-logo.png");
        GUILayout.Space(0);
        Rect lastRect = GUILayoutUtility.GetLastRect();
        GUI.Box(new Rect(1, lastRect.y + 4, Screen.width, 27), tex);
        lastRect = GUILayoutUtility.GetLastRect();
        Color bgColor = new Color();
        ColorUtility.TryParseHtmlString("#cde4e0", out bgColor);
        EditorGUI.DrawRect(new Rect(lastRect.x - 15, lastRect.y + 5f, Screen.width + 1, 60f), bgColor);
        GUI.DrawTexture(new Rect(Screen.width/2 - tex.width/2, lastRect.y + 10, tex.width, tex.height), tex, ScaleMode.ScaleToFit);
        GUI.color = Color.white;
        GUILayout.Space(60f);
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

    public void WriteMessage(string targetBoard, string message)
    {
        manager.Write(targetBoard, message);
    }

    public void Read(string target = null, string variable = null, System.Action<string> action = null)
    {
        manager.DirectReadFromArduino(target, variable, action: action);
    }

    public void RemovePin(Pin pin)
    {
        pin.Destroy();
        pins.Remove(pin);
    }

}