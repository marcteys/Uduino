/*
 * Uduino - Arduino-Unity Library
 * Version 1.3, Jan 2017, Marc Teyssier
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;

namespace Uduino
{

    #region Enums
    public enum PinMode
    {
        Output,
        PWM,
        Analog,
        Input_pullup,
        Servo
    };

    public enum AnalogPin { A0 = 14, A1 = 15, A2 = 16, A3 = 17, A4 = 18, A5 = 19 };

    public enum State
    {
        LOW,
        HIGH
    };

    public enum SerialStatus
    {
        UNDEF,
        OPEN,
        FOUND,
        CLOSE
    };

    public enum LogLevel
    {
        DEBUG,
        INFO,
        WARNING,
        ERROR,
        NONE
    };


    #endregion

    public class UduinoManager : MonoBehaviour {

        #region Singleton
        /// <summary>
        /// UduinoManager unique instance.
        /// Create  a new instance if any UduinoManager is present on the scene.
        /// Set the Uduinomanager only on the first time.
        /// </summary>
        /// <value>UduinoManager static instance</value>
        public static UduinoManager Instance
        {
            get {
                //UduinoManager Instance StackOverflowException 
                UduinoManager[] uduinoManagers = FindObjectsOfType(typeof(UduinoManager)) as UduinoManager[];
                if (uduinoManagers.Length == 0 )
                {
                    Log.Warning("UduinoManager not present on the scene. Creating a new one.");
                    UduinoManager manager = new GameObject("UduinoManager").AddComponent<UduinoManager>();
                    _instance = manager;
                    return _instance;
                }
                else
                    return _instance;
            }
            set {
                if(UduinoManager.Instance == null)
                    _instance = value;
                else
                {
                    Log.Warning("You can only use one UduinoManager. Destroying the new one attached to the GameObject " + value.gameObject.name);
                    DestroyImmediate(value);
                }
            }
        }
        private static UduinoManager _instance = null;

        #endregion

        #region Variables
        /// <summary>
        /// Dictionnary containing all the connected Arduino devices
        /// </summary>
        public Dictionary<string, UduinoDevice> uduinoDevices = new Dictionary<string, UduinoDevice>();

        /// <summary>
        /// List containing all active pins
        /// </summary>
        public List<Pin> pins = new List<Pin>();

        /// <summary>
        /// Create a delegate event to trigger the function OnValueReceived()
        /// Takes one parameter, the returned data.
        /// </summary>
        public delegate void OnValueReceivedEvent(string data, string device);
        public event OnValueReceivedEvent OnValueReceived;

        /// <summary>
        /// Log level
        /// </summary>
        [SerializeField]
        public LogLevel debugLevel;

        /// <summary>
        /// BaudRate
        /// </summary>
        [SerializeField]
        private int baudRate = 9600;
        public int BaudRate {
            get { return baudRate; }
            set { baudRate = value; }
        }

        /// <summary>
        /// Enable the reading of serial port in a different Thread.
        /// Might be usefull for optimization and not block the runtime during a reading. 
        /// </summary>
        [SerializeField]
        private bool readOnThread = true;
        public bool ReadOnThread
        {
            get { return readOnThread; }
            set { readOnThread = value; }
        }

        /// <summary>
        /// Limitation of the send rate
        /// Packing into bundles
        /// </summary>
        [SerializeField]
        private bool limitSendRate = false;
        public bool LimitSendRate
        {
            get { return limitSendRate; }
            set {
                if (limitSendRate == value)
                    return;
               if (Application.isPlaying)
               {
                    if (value && !autoSendIsRunning)
                    {
                        Log.Debug("Start auto read");
                        StartCoroutine("AutoSendBundle");
                        autoSendIsRunning = true;
                    }
                    else
                    {
                        Log.Debug("Stop auto read");
                        StopCoroutine("AutoSendBundle");
                        autoSendIsRunning = false;
                    }
               }
                limitSendRate = value;
            }
        }
        private bool autoSendIsRunning = false;


        /// <summary>
        /// SendRateSpeed
        /// </summary>
        [SerializeField]
        private int sendRateSpeed = 20;
        public int SendRateSpeed
        {
            get { return sendRateSpeed; }
            set { sendRateSpeed = value; }
        }

        /// <summary>
        /// Number of tries to discover the attached serial ports
        /// </summary>
        [SerializeField]
        private int discoverTries = 20;
        public int DiscoverTries
        {
            get { return discoverTries; }
            set { discoverTries = value; }
        }

        /// <summary>
        /// Stop all digital/analog pin on quit
        /// </summary>
        public bool stopAllOnQuit = true;

        /// <summary>
        /// List of black listed ports
        /// </summary>
        [SerializeField]
        private List<string> blackListedPorts = new List<string>();
        public List<string> BlackListedPorts {
            get { return blackListedPorts; }
            set { blackListedPorts = value; }
        }
        #endregion

        #region Init
        void Awake()
        {
            Instance = this;
            Log.SetLogLevel(debugLevel);
            DiscoverPorts();
            if(readOnThread) StartThread();
            OnValueReceived += DefaultOnValueReceived;

            StopCoroutine("AutoSendBundle");
            if (limitSendRate) StartCoroutine("AutoSendBundle");
        }

        #endregion

        #region Ports discovery
        /// <summary>
        /// Get the ports names, dependings of the current OS
        /// </summary>
        public void DiscoverPorts()
        {
            CloseAllPorts();
        #if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
			Discover(GetUnixPortNames());
        #else
            Discover(SerialPort.GetPortNames());
        #endif
        }

		/// <summary>
		/// Get the ports names if the system is on unix
		/// </summary>
		private string[] GetUnixPortNames ()
		{
			int p = (int)System.Environment.OSVersion.Platform;
			List<string> serial_ports = new List<string> ();

			if (p == 4 || p == 128 || p == 6) {
				string[] ttys = System.IO.Directory.GetFiles ("/dev/", "tty.*");
				return ttys;
                //TODO : Test on Linux and MacOS
                /*
				foreach (string dev in ttys) {
					if (dev.StartsWith ("/dev/tty.*")) // TODO : Test if (portName.StartsWith ("/dev/tty.usb") || portName.StartsWith ("/dev/ttyUSB"))
						serial_ports.Add (dev);
				}*/
			} 
			return serial_ports.ToArray();
		}

        /// <summary>
        /// Discover all active serial ports connected.
        /// When a new serial port is connected, send the IDENTITY request, to get the name of the arduino
        /// </summary>
        /// <param name="portNames">All Serial Ports names, dependings of the current OS</param>
        void Discover(string[] portNames)
        {
            if (portNames.Length == 0) Log.Error("Found 0 ports open. Are you sure your arduino is connected ?");

            foreach (string portName in portNames)
            {
                if(!blackListedPorts.Contains(portName))
                    StartCoroutine(FindBoardPort(portName));
                else
                    Log.Info("Port " + portName + " is blacklisted");
            }
        }

        /// <summary>
        /// Find a board connected to a specific port
        /// </summary>
        /// <param name="portName">Port open</param>
        IEnumerator FindBoardPort(string portName)
        {
            UduinoDevice uduinoDevice = new UduinoDevice(portName, baudRate);
            int tries = 0;
            do
            {
                if (uduinoDevice.getStatus() == SerialStatus.OPEN)
                {
                    string reading = uduinoDevice.ReadFromArduino("IDENTITY", 200);
                    if (reading != null && reading.Split(new char[0])[0] == "uduinoIdentity")
                    {
                        string name = reading.Split(new char[0])[1];
                        uduinoDevice.name = name;
                        lock(uduinoDevices)
                            uduinoDevices.Add(name, uduinoDevice); //Add the new device to the devices array
                        if (!ReadOnThread) StartCoroutine(ReadSerial(name)); // Initiate the Async reading of variables 
                        Log.Warning("Board <color=#ff3355>" + name + "</color> <color=#2196F3>[" + uduinoDevice.getPort() + "]</color> added to dictionnary");
                        uduinoDevice.UduinoFound();
                        InitAllPins();
                        break;
                    }
                    else
                    {
                        Log.Info("Impossible to get name on <color=#2196F3>[" + portName + "]</color>. Retrying.");
                    }
                }
                yield return new WaitForSeconds(0.05f);    //Wait one frame with yield return null
            } while (uduinoDevice.getStatus() != SerialStatus.UNDEF && tries++ < discoverTries);

            if(uduinoDevice.getStatus() != SerialStatus.FOUND)
            {
                Log.Warning("Impossible to get name on <color=#2196F3>[" + portName + "]</color>. Closing.");
                uduinoDevice.Close();
                uduinoDevice = null;
            }
        }

        /// <summary>
        /// Debug all ports state.
        /// TODO : Really neccessary ? They are always open...
        /// TODO : Put that in the utils.cs ? 
        /// </summary>
        public void GetPortState()
        {
            if (uduinoDevices.Count == 0)
            {
                Log.Error("No port currently open");
            }
            foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
            {
                string state = uduino.Value.serial.IsOpen ? "open " : "closed";
                Log.Info("" + uduino.Value.getPort() + " (" + uduino.Key + ")" + " is " + state);
            }
        }

        #endregion

        #region Simple commands : Pin setup
        /// <summary>
        /// Init a pin 
        /// </summary>
        /// <param name="pin">Pin to initialize</param>
        /// <param name="mode">PinMode to init pin</param>
        public void InitPin(int pin, PinMode mode)
        {
            InitPin(null, pin, mode);
        }

        /// <summary>
        /// Init a pin
        /// </summary>
        /// <param name="pin">Analog pin to initialize</param>
        /// <param name="mode">PinMode to init pin</param>
        public void InitPin(AnalogPin pin, PinMode mode)
        {
            InitPin(null, (int)pin, mode);
        }

        /// <summary>
        /// Create a new Pin and setup the mode if the pin is not registered.
        /// If the pin exsists, change only the mode
        /// </summary>
        /// <param name="string">Target Name</param>
        /// <param name="pin">Pin to init</param>
        /// <param name="mode">PinMode to init pin</param>
        public void InitPin(string target, int pin, PinMode mode)
        {
            if (target == null) target = "";
            bool pinExists = false;

            foreach (Pin pinTarget in pins)
            {
                if (pinTarget.PinTargetExists(target, pin))
                {
                    pinTarget.ChangePinMode(mode);
                    pinExists = true;
                }
            }
            if (!pinExists)
            {
                Pin newPin = new Pin(target, pin, mode);
                pins.Add(newPin);
                if (UduinoTargetExists(target) || (target == "" && uduinoDevices.Count != 0))
                    newPin.Init();
            }
        }

        /// <summary>
        /// Init a pin
        /// </summary>
       /// <param name="string">Target Name</param>
        /// <param name="pin">Pin to init</param>
        /// <param name="mode">PinMode to init pin</param>
        public void InitPin(string target, AnalogPin pin, PinMode mode)
        {
            InitPin((int)pin, mode);
        }

        // TODO : Refactor that. Should not work with multiple boards !
        /// <summary>
        /// Init all Pins when the arduino boards are found
        /// </summary>
        public void InitAllPins()
        {
            Log.Debug("Init all pins");
            foreach(Pin pin in pins)
            {
                pin.Init();
            }
            SendBundle("init");
        }

        #endregion

        #region Simple commands : Write

        /// <summary>
        /// DigitalWrite or AnalogWrite to arduino
        /// </summary>
        /// <param name="target"></param>
        /// <param name="pin"></param>
        /// <param name="value"></param>
        public void arduinoWrite(string target, int pin, int value, string typeOfPin, string bundle = null)
        {
            foreach (Pin pinTarget in pins)
            {
                if (pinTarget.PinTargetExists(target, pin))
                    pinTarget.SendPinValue(value, typeOfPin, bundle);
            }
        }

        /// <summary>
        /// Write a digital command to the arduino
        /// </summary>
        /// <param name="target"></param>
        /// <param name="pin"></param>
        /// <param name="value"></param>
        public void digitalWrite(string target, int pin, int value, string bundle = null)
        {
            if (value <= 150) value = 0;
            else value = 255;
            arduinoWrite(target,pin,value,"d", bundle);
        }

        /// <summary>
        /// Write a command on an Arduino
        /// </summary>
        public void digitalWrite(int pin, int value, string bundle = null)
        {
            digitalWrite("", pin, value, bundle);
        }

        /// <summary>
        /// Write a command on an Arduino
        /// </summary>
        public void digitalWrite(int pin, State state = State.LOW, string bundle = null)
        {
            arduinoWrite("", pin, (int)state * 255,"d", bundle);
        }

        /// <summary>
        /// Write an analog value to Arduino
        /// </summary>
        /// <param name="pin">Arduino Pin</param>
        /// <param name="value">Value</param>
        public void analogWrite(int pin, int value, string bundle = null)
        {
            arduinoWrite(null, pin, value, "a", bundle);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target">Arduino board</param>
        /// <param name="pin">Arduino Pin</param>
        /// <param name="value">Value</param>
        public void analogWrite(string target, int pin, int value, string bundle = null)
        {
            arduinoWrite(target, pin, value, "a", bundle);
        }

        #endregion

        #region Simple commands: Read
        public int analogRead(string target, int pin, string bundle = null)
        {
            int readVal = 0;

            foreach (Pin pinTarget in pins)
            {
                if (pinTarget.PinTargetExists(target, pin))
                {
                    pinTarget.SendRead(bundle, ParseAnalogReadValue);
                    readVal = pinTarget.lastReadValue;
                }
            }

            return readVal;
        }

        public int analogRead(int pin, string bundle = null)
        {
            return analogRead("", pin, bundle);
        }

        public int analogRead(AnalogPin pin, string bundle = null)
        {
            return analogRead("", (int)pin, bundle);
        }

        //TODO : Add ref to the card 
        public void ParseAnalogReadValue(string data/*, string target =null*/)
        {
            string[] parts = data.Split('-');
            int max = 0;
            if (parts.Length == 1) max = 1;
            else max = parts.Length - 1;
            for (int i=0;i< max; i++)
            {
                int recivedPin = -1;
                int.TryParse(parts[i].Split(' ')[0], out recivedPin);
               
                int value = 0;
                int.TryParse(parts[i].Split(' ')[1], out value);
                if (recivedPin != -1)
                    dispatchValueForPin("", recivedPin, value);
            }
        }

        /// <summary>
        /// Dispatch received value for a pin
        /// </summary>
        /// <param name="target"></param>
        /// <param name="pin"></param>
        /// <param name="message"></param>
        /// <returns>Value</returns>
        public int dispatchValueForPin(string target, int pin, int readVal)
        {
            foreach (Pin pinTarget in pins)
            {
                if (pinTarget.PinTargetExists(target, pin))
                {
                   pinTarget.lastReadValue = readVal;
                }
            }
            return readVal;
        }

        #endregion

        #region Commands
        /// <summary>
        /// Send a read command to a specific arduino.
        /// A read command will be returned in the OnValueReceived() delegate function
        /// TODO : I should remove that from here ? 
        /// </summary>
        /// <param name="target">Target device name. Not defined means read everything</param>
        /// <param name="variable">Variable watched, if defined</param>
        /// <param name="timeout">Read Timeout, if defined </param>
        /// <param name="callback">Action callback</param>
        public void Read(string target = null, string message = null, int timeout = 100, System.Action<string> action = null, string bundle = null)
        {
            if (bundle != null)
            {
                if (UduinoTargetExists(target))
                {
                    uduinoDevices[target].callback = action;
                    uduinoDevices[target].AddToBundle(message, bundle);
                }
                else
                    foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                    {
                        uduino.Value.callback = action;
                        uduino.Value.AddToBundle(message, bundle);
                    }
            }
            else
            {
                if (UduinoTargetExists(target))
                {
                    uduinoDevices[target].callback = action;
                    uduinoDevices[target].read = message;
                }
                else
                {
                    foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                    {
                        uduino.Value.callback = action;
                        uduino.Value.read = message;
                    }
                }
            }
        }

        public void DirectReadFromArduino(string target = null, string message = null, System.Action<string> action = null, int timeout = 100, string bundle = null)
        {
            if (bundle != null)
            {
                if (UduinoTargetExists(target))
                    uduinoDevices[target].AddToBundle(message, bundle);
                else
                    foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                        uduino.Value.AddToBundle(message, bundle);
            }
            else
            {
                if (UduinoTargetExists(target))
                {
                    uduinoDevices[target].callback = action;
                    uduinoDevices[target].ReadFromArduino(message, timeout);
                }
                else
                {
                    foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                    {
                        uduino.Value.callback = action;
                        uduino.Value.ReadFromArduino(message, timeout);
                    }
                }
            }
        }

        //TODO : Too much overload ? Bundle ? 
        public void Read(int pin, string target=null, System.Action<string> action = null, int timeout = 100)
        {
            DirectReadFromArduino(action: action);
        }

        public void Read(int pin, System.Action<string> action = null)
        {
            DirectReadFromArduino(action: action);
        }
        #endregion

        #region Write advanced commands
        /// <summary>
        /// Write a command on an Arduino
        /// </summary>
        /// <param name="target">Target device</param>
        /// <param name="message">Message to write in the serial</param>
        public void Write(string target = null, string message = null, string bundle = null)
        {
            if(bundle != null || limitSendRate)
            {
                if (limitSendRate) bundle = "LimitSend";
                if (UduinoTargetExists(target))
                    uduinoDevices[target].AddToBundle(message, bundle);
                else
                    foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                        uduino.Value.AddToBundle(message, bundle);
            }
            else
            {
                if (UduinoTargetExists(target))
                    uduinoDevices[target].WriteToArduino(message);
                else
                    foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                        uduino.Value.WriteToArduino(message);
            }
        }

        /// <summary>
        /// Write a command on an Arduino with a specific value  
        /// </summary>
        /// <param name="target">Target device</param>
        /// <param name="message">Message to write in the serial</param>
        /// <param name="value">Optional value</param>
        public void Write(string target, string message, int value) {
            if (UduinoTargetExists(target))
                uduinoDevices[target].WriteToArduino(message, value);
            else
                foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                    uduino.Value.WriteToArduino(message,value);
        }


        /// <summary>
        /// Verify if the target exists when we want to get a value
        /// </summary>
        /// <param name="target">Target Uduino Name</param>
        /// <returns>Re</returns>
        private bool UduinoTargetExists(string target)
        {
            UduinoDevice uduino = null;
            if (target == "" || target == null) return false;
            if (uduinoDevices.TryGetValue(target, out uduino))
                return true;
            else
            {
				//TODO: Restart a loop to find all objects
                if(target != null && target != "") Log.Warning("The object " + target + " cannot be found. Are you sure it's connected and correctly detected ?");
                return false;
            }
        }

        #endregion

        #region Bundle
        /// <summary>
        /// Send an existing message bundle to Arduino
        /// </summary>
        /// <param name="target">Target arduino</param>
        /// <param name="bundleName">Bundle name</param>
        public void SendBundle(string target, string bundleName)
        {
            if (UduinoTargetExists(target))
                uduinoDevices[target].SendBundle(bundleName);
            else
                foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                    uduino.Value.SendBundle(bundleName);
        }

        /// <summary>
        /// Send an existing message bundle to Arduino
        /// </summary>
        /// <param name="bundleName">Bundle name</param>
        public void SendBundle(string bundleName)
        {
            SendBundle(null, bundleName);
        }

        /// <summary>
        /// Automatically send bundles
        /// </summary>
        /// <returns>Delay before next sending</returns>
        IEnumerator AutoSendBundle()
        {
            while (true)
            {
                if (!LimitSendRate)
                    yield return null;

                yield return new WaitForSeconds(sendRateSpeed / 1000.0f);
                List<string> keys = new List<string>(uduinoDevices.Keys);
                foreach (string key in keys)
                    uduinoDevices[key].SendAllBundles();
            }
        }

        #endregion

        #region Hardware reading
        /// <summary>
        /// Threading variables
        /// </summary>
        private System.Threading.Thread _Thread = null;
        private bool readAllPorts = true;

        /// <summary>
        /// Initialisation of the Thread reading on Awake()
        /// </summary>
        void StartThread()
        {
            try
            {
                _Thread = new System.Threading.Thread(ReadPorts);
                _Thread.Start();
            }
            catch (System.Threading.ThreadStateException e)
            {
                //TODO : Parse the errors and display a correct message
                Log.Error(e);
            }
        }
        

        void Update()
        {
            Debug.Log(_Thread.ThreadState);
        }

        /// <summary>
        ///  Read the Serial Port data in a new thread.
        /// </summary>
        public void ReadPorts()
        {
            while (readAllPorts)
            {
                lock (uduinoDevices)
                {
                    string[] keys = new string[uduinoDevices.Count];
                    for (int i=0;i < uduinoDevices.Count;i++)
                    {
                        print(i);
                        uduinoDevices.Keys.CopyTo(keys,i);

                    }
                    foreach(string key in keys)
                    {
                        if (uduinoDevices[key].read != null)
                        {
                            string data = uduinoDevices[key].ReadFromArduino(uduinoDevices[key].read, 50);
                            uduinoDevices[key].read = null;
                            ReadData(data, key);
                        }
                    }
                    /*
                    foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                    {

                        if (uduino.Value.read != null)
                        {
                            string data = uduino.Value.ReadFromArduino(uduino.Value.read, 50);
                            uduino.Value.read = null;
                            ReadData(data, uduino.Key);
                        }

                    }*/
                }
                //       foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                //  {
                /*
                if (uduino.Value.read != null)
                {
                    string data = uduino.Value.ReadFromArduino(uduino.Value.read, 50);
                    uduino.Value.read = null;
                    ReadData(data, uduino.Key);
                }*/
                // }
            }
        }

        /// <summary>
        /// Retreive the Data from the Serial Prot using Unity Coroutines
        /// </summary>
        /// <param name="target"></param>
        /// <returns>null</returns>
        public IEnumerator ReadSerial(string target)
        {
            while (true)
            {
                UduinoDevice uduino = null;
                if (uduinoDevices.TryGetValue(target, out uduino))
                {
                    if (uduino.read != null)
                    {
                        string data = uduino.ReadFromArduino(uduino.read, 50);
                        uduino.read = null;
                        yield return null;
                        ReadData(data, target);
                    }
                    else
                    {
                        yield return null;
                    }
                }
                else
                {
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Parse the received data
        /// TODO : Rename in dataReaded
        /// </summary>
        /// <param name="data">Received data</param>
        /// <param name="target">TODO : for the moment target is unused</param>
        void ReadData(string data, string target = null)
        {
            if (data != null && data != "" && data != "Null")
            {
                UduinoDevice uduino = uduinoDevices[target]; //TODO : And if it's null ?
                uduino.lastRead = data;
                if (uduino.callback != null) uduino.callback(data);
                else OnValueReceived(data, target);
            }
        }
        /// <summary>
        /// Default delegate OnValueReceived 
        /// </summary>
        /// TODO : Maybe it's better to use delegate rather than CallbackFunction
        /// <param name="data">Data received</param>
        /// <param name="device">Device emmiter</param>
        void DefaultOnValueReceived(string data, string device)
        {
        //   Debug.Log(data);
        }

        #endregion

        #region Close Ports
        /// <summary>
        /// Close all opened serial ports
        /// </summary>
        public void CloseAllPorts()
        {
            if (uduinoDevices.Count == 0)
            {
                Log.Debug("Ports already closed.");
                return;
            }

            if(stopAllOnQuit)
            {
                foreach (Pin pinTarget in pins)
                    pinTarget.Destroy();
            }

            List<string> keys = new List<string>(uduinoDevices.Keys);
            foreach (string key in keys)
            {
                SerialArduino device = uduinoDevices[key];
                device.Close();
                uduinoDevices.Remove(key);
            }
        }

        public void OnDisable()
        {
            if (readOnThread)
            {
                DisableThread();
            }
            if(uduinoDevices.Count != 0) CloseAllPorts();
        }

        void DisableThread()
        {
            readAllPorts = false;
            if (_Thread != null) _Thread.Abort();
            _Thread = null;
        }
        #endregion
    }

    #region Version
    public static class UduinoVersion
    {
        static int major = 1;
        static int minor = 3;
        static string update = "Jan 2017";

        public static string getVersion()
        {
            return major + "." + minor;
        }

        public static string lastUpdate()
        {
            return update;
        }
    }
    #endregion
}