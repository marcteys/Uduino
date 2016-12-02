using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Uduino;


public class UduinoPanel : EditorWindow
{
    //Pin stuff
    public enum PinMode
    {
        Output,
        Input, 
        PWM, 
        Analog
    }


    public List<Pin> pins = new List<Pin>();

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
            UduinoManager m = (UduinoManager)UnityEngine.Object.FindObjectOfType(typeof(UduinoManager));
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

        EditorGUILayout.Separator();
        GUILayout.BeginHorizontal();
        GUILayout.Button("Pin", "OL Title",GUILayout.MaxWidth(56f));
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