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
    public static bool IsOpen
    {
        get { return Instance != null; }
    }
    public string targetName = "myArduinoName";
    public string message = "";
    LogLevel debugLevel;

    UduinoManager manager = null;

    public List<Pin> pins = new List<Pin>();

    Color headerColor = new Color(0.65f, 0.65f, 0.65f, 1);
    Color backgroundColor = new Color(0.75f, 0.75f, 0.75f);
    Color defaultButtonColor;

    bool defaultPanel = true;
    bool arduinoPanel = true;
    bool advancedPanel = true;
    bool debugPanel = false;


    void OnEnable()
    {
        defaultButtonColor = GUI.backgroundColor;
        Instance = this;
        Repaint();
        manager.DiscoverPorts();
    }

    public override void OnInspectorGUI()
    {
        if (manager == null) manager = (UduinoManager)target;
        Log.SetLogLevel(manager.debugLevel);


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

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace(); // Nededed for lastRect
        EditorGUILayout.EndHorizontal();

        DrawLogo();
      
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace(); // Nededed for lastRect
        EditorGUILayout.EndHorizontal();

        defaultPanel = DrawHeaderTitle("Uduino Settings", defaultPanel, headerColor);
        if (defaultPanel)
        {
            DrawDefaultInspector();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Paint Mode: ", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
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

        debugPanel = DrawHeaderTitle("Debug", debugPanel, headerColor);
        if (debugPanel)
        {
            DebugSettings();
        }

        //TODO : Needed to update when message sent/received. This uses a lot of passes. Maybe change that, do a variable to check if a new value is here
        EditorUtility.SetDirty(target);
    }


    public void ArduinoSetings()
    {

        if (manager.uduinoDevices.Count == 0)
        {

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Button("No arduino connected", "Box", GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
         }
       else
        {
           // GUILayout.BeginHorizontal();
          //  GUILayout.Label(manager.uduinoDevices.Count + " Arduino connected", EditorStyles.boldLabel);
          //  GUILayout.EndHorizontal();

            //TODO : Compact that in another function
            foreach (KeyValuePair<string, UduinoDevice> uduino in manager.uduinoDevices)
            {
                var boldtext = new GUIStyle(GUI.skin.label);
                boldtext.fontStyle = FontStyle.Bold;
                boldtext.alignment = TextAnchor.UpperCenter;
                SetGUIBackgroundColor("#4FC3F7");
                GUILayout.BeginVertical("Box",  GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                GUILayout.Label(uduino.Key, boldtext);
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
                SetGUIBackgroundColor();


                GUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("Last read message", uduino.Value.lastRead);
                EditorGUILayout.LabelField("Last sent value", uduino.Value.lastWrite);
                //Todo: auto read


                EditorGUILayout.Separator();
                GUILayout.BeginHorizontal();
                GUILayout.Button("Pin", "OL Titleleft", GUILayout.MaxWidth(56f));
                GUILayout.Button("Mode", "OL Titlemid", GUILayout.MaxWidth(105f));
                GUILayout.Button("Action", "OL Titlemid", GUILayout.ExpandWidth(true));
                GUILayout.Button("×", "OL Titleright", GUILayout.MaxWidth(25f));
                GUILayout.EndHorizontal();

                if(pins.Count == 0)
                {

                } else
                {

                }
                GUILayout.BeginVertical("Box");



                foreach (Pin pin in pins.ToArray())
                {
                    pin.Draw();
                }
                GUILayout.EndVertical();

                GUILayout.BeginHorizontal("TE Toolbar", GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();

                

                if (GUILayout.Button("Test a pin", GUILayout.ExpandWidth(true)))
                {
                    pins.Add(new Pin(this, uduino.Key));
                }

                GUILayout.EndVertical();

            }
        }


        DrawLine(12,0,45);

        GUILayout.BeginHorizontal();
     //   GUILayout.BeginHorizontal("IN Title");

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

    public void DebugSettings()
    {
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
    }

    public void AdvancedSettings()
    {

        GUILayout.BeginHorizontal("window");
        GUILayout.BeginVertical();
        if (GUILayout.Button("Get port state"))
        {
            manager.GetPortState();
        }
        if (GUILayout.Button("Clear console"))
        {
            var logEntries = System.Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
            var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            clearMethod.Invoke(null, null);
        }

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }




    public void DrawBox(int marginTop, int marginBottom, int height)
    {
        EditorGUILayout.Separator();
        GUILayout.Space(marginTop);
        Rect lastRect = GUILayoutUtility.GetLastRect();
        GUILayout.Space(marginBottom);
    }


    public void DrawLine(int marginTop, int marginBottom, int height)
    {
        EditorGUILayout.Separator();

        /*            GUILayout.BeginHorizontal("sv_iconselector_sep");
            GUILayout.EndHorizontal();
*/
        GUILayout.Space(marginTop);
        Rect lastRect = GUILayoutUtility.GetLastRect();
        GUI.Box(new Rect(0f, lastRect.y + 4, Screen.width, height),"");
        GUILayout.Space(marginBottom);
    }

    public void DrawLogo()
    {
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

    public void DrawPanelMessage()
    {

    }

    public void RemovePin(Pin pin)
    {
        pin.Destroy();
        pins.Remove(pin);
    }

}