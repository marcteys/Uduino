using UnityEditor;
using UnityEngine;
using Uduino;

public class UduinoPanel : EditorWindow
{
    public static UduinoPanel Instance { get; private set; }
    public static bool IsOpen
    {
        get { return Instance != null; }
    }

    string myString = "Hello World";
    bool groupEnabled;
    bool myBool = true;
    float myFloat = 1.23f;

    UduinoManager manager = null;
    Editor managerEditor = null; // might not be needed

    [MenuItem("Window/Uduino")]
    public static void ShowWindow()
    {
        GetWindow<UduinoPanel>("Uduino Panel");
        EditorWindow.GetWindow(typeof(UduinoPanel));
    }

    void OnEnable()
    {
        Instance = this;
        Repaint();
    }

    public void Init() //USELESS : To Remove
    {
        UduinoManager manager = (UduinoManager)FindObjectOfType(typeof(UduinoManager));
    }

    void GetUduinoManager()
    {
        if(manager == null || managerEditor == null)
        {
            UduinoManager m = (UduinoManager)Object.FindObjectOfType(typeof(UduinoManager));
            if(m != null)
            {
                managerEditor = Editor.CreateEditor(m);
                manager = (UduinoManager)managerEditor.target;
            } else
            {
                Debug.Log("aaaargh do something please !!!");
            }
        }
    }


    void OnGUI()
    {
        GetUduinoManager();

        managerEditor.DrawDefaultInspector();

        myString = EditorGUILayout.TextField("Text Field", myString);

        groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
        myBool = EditorGUILayout.Toggle("Toggle", myBool);
        myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
        EditorGUILayout.EndToggleGroup();

    }
}