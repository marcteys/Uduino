using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;

public class LivingDevicesManager : MonoBehaviour {

    bool _isDiscovered = false;
    public Dictionary<string, LivingDevice> livingDesktopDevices = new Dictionary<string, LivingDevice>();
    public LivingMouse livingMouse = null;
    public LivingKeyboard livingKeyboard = null;
    public LivingScreen livingScreen = null;

    public static LivingDevicesManager Instance {
        get {
            //if(_instance) _instance.CalibrateAll();
            return _instance;
        }
        set {
            _instance = value;
        }
    }
    private static LivingDevicesManager _instance = null;


    bool isCalibrated = false;

	void Start ()
	{

        DontDestroyOnLoad(this.gameObject);
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != null && Instance != this)
        {
            Debug.Log("LivingDevicesManager already here : Destroing...");
            Destroy(this.gameObject);
            return;
        }

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        Discover(new string[] {"/dev/ttyUSB0", "/dev/ttyUSB1", "/dev/ttyUSB2", "/dev/ttyUSB3", "/dev/ttyUSB4", "/dev/ttyUSB5"});
#else
        Discover(SerialPort.GetPortNames());
        #endif
    }

    void Discover(string[] portNames)
    {
        if (portNames.Length == 0) Debug.Log("arduino not found");

        foreach (string portName in portNames)
        {
          SerialArduino serialObject =  new SerialArduino(portName, 9600);
          int tries = 0;
            do
            {
                if (serialObject.getStatus() == SerialArduino.SerialStatus.OPEN)
                {
                    serialObject.WriteToArduino("I");
                    string reading = serialObject.ReadFromArduino(50);
                    if (reading != null)
                    {
                        this.LivingObjectFound(reading, serialObject);
                        break;
                    }
                    else
                    {
                         Debug.LogWarning("Impossible to get name on <color=#2196F3>[" + portName +"]</color>. Retrying (" + tries + "/50)");
                    }
                }
            } while (serialObject.getStatus() != SerialArduino.SerialStatus.UNDEF && tries++ < 50);

            if (serialObject.getStatus() == SerialArduino.SerialStatus.UNDEF || serialObject.getStatus() == SerialArduino.SerialStatus.CLOSE)
            {
                serialObject = null;
            }
         }
        _isDiscovered = true;
    }

    void LivingObjectFound(string name, SerialArduino serialArduino)
    {
        switch (name)
        {
            case "LivingKeyboard":
                livingKeyboard = new LivingKeyboard(serialArduino);
                livingDesktopDevices.Add(name, livingKeyboard);
            break;
            case "LivingScreen":
            livingScreen = new LivingScreen(serialArduino);
                livingDesktopDevices.Add(name, livingScreen);
            break;
            case "LivingMouse":
            livingMouse = new LivingMouse(serialArduino);
                livingDesktopDevices.Add(name, livingMouse);
            break;
            default:
                Debug.Log("Error !");
            break;
        }
        Debug.Log("Object <color=#ff3355>" + name + "</color> <color=#2196F3>[" + serialArduino.getPort() + "]</color> added to dictionnary");
    }

    public void SendTestMessage(string target, string message)
    {
        livingDesktopDevices[target].getSerial().WriteToArduino(message);
    }

    public void DiscoverPorts()
    {
        #if UNITY_WIN || UNITY_LINUX || UNITY_EDITOR
        Discover(SerialPort.GetPortNames());
        #else
        Discover(new string[] {"/dev/ttyUSB0", "/dev/ttyUSB1", "/dev/ttyUSB2", "/dev/ttyUSB3", "/dev/ttyUSB4", "/dev/ttyUSB5"});
        #endif 
    }

    public void CloseAllPorts()
    {
        if (livingDesktopDevices.Count == 0)
        {
            Debug.Log("All ports closed");
        }

        Dictionary<string, LivingDevice> tmpDic = new Dictionary<string, LivingDevice>(livingDesktopDevices);
        foreach (KeyValuePair<string, LivingDevice> liv in tmpDic)
        {
            SerialArduino device = liv.Value.getSerial();
            device.Close();
           livingDesktopDevices.Remove(liv.Key);
        }
    }

    public void GetPortState()
    {
        if (livingDesktopDevices.Count == 0)
        {
            Debug.Log("No port open");
        }
        foreach (KeyValuePair<string, LivingDevice> liv in livingDesktopDevices)
        {
            SerialArduino device = liv.Value.getSerial();
            string state = device.getStream().IsOpen ? "open " : "closed";
            Debug.Log(device.getPort() + " (" + liv.Key + ")" + " is " + state);
        }
    }

    public void CalibrateAll(bool forceCalibration = false)
    {
        if (livingMouse != null)
        {
            if (!isCalibrated || forceCalibration)
            {
                livingMouse.Calibrate();
                isCalibrated = true;
            }
        }
         if (livingKeyboard != null) {
             if (!isCalibrated || forceCalibration)
             {
                livingKeyboard.Calibrate();
                isCalibrated = true;
            }
        } 
        if (livingScreen != null) {
            if (!isCalibrated || forceCalibration)
            {
                livingScreen.Calibrate();
                isCalibrated = true;
            }
        }
    }

    public void OnDisable()
    {
        CloseAllPorts();
    }
}