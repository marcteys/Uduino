/* 
 * Uduino - Yet Another Arduin-Unity Library
 * Version 1.1, 2016, Marc Teyssier
 *  
 *  ================
 *       TODOs
 *  ================
 *  UduinoManager.Instance.Read("sensorArduino", "SENSOR", 2000); -> Verifiy that the timeout is working
 *  Option to "write" on all arduino when no one is specified (set the param as option)
 *  Public values for the number of tries ?
 *  Function to discover manually a specific port ?
 *  TODO : Create a "utils" script, with some helper functions (ex conver a string to int, send an array of string, etc)
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;

namespace Uduino
{
    public enum PinMode
    {
        Output,
        PWM,
        Analog,
        Input_pullup,
        Servo
    }

    public enum AnalogPin { A0 = 12, A1 = 10, A2 = 3, A3 = 1, A4 = 2, A5 = 4 }


    public class UduinoManager : MonoBehaviour {

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
                    Debug.Log("UduinoManager not present on the scene. Creating a new one.");
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
                    Debug.Log("You can only use one UduinoManager. Destroying the new one attached to the GameObject " + value.gameObject.name);
                    DestroyImmediate(value);
                }
            }
        }
        private static UduinoManager _instance = null;

        /// <summary>
        /// Dictionnary containing all the connected Arduino devices
        /// </summary>
        public Dictionary<string, UduinoDevice> uduinoDevices = new Dictionary<string, UduinoDevice>();

        /// <summary>
        /// Create a delegate event to trigger the function OnValueReceived()
        /// Takes one parameter, the returned data.
        /// </summary>
        public delegate void OnValueReceivedEvent(string data, string device);
        public event OnValueReceivedEvent OnValueReceived;

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
        /// BaudRate
        /// </summary>
        [SerializeField]
        private int baudRate = 9600;
        public int BaudRate {
            get { return baudRate; }
            set { baudRate = value; }
        }

        /// <summary>
        /// Log level
        /// </summary>
        [SerializeField]
        public LogLevel debugLevel;

        /// <summary>
        /// Number of tries to discover the attached serial ports
        /// </summary>
        private int discoverTries = 20;
        [SerializeField]
        public int DiscoverTries
        {
            get { return discoverTries; }
            set { discoverTries = value; }
        }

        public List<string> blackListedPorts = new List<string>();
        public List<string> BlackListedPorts {
            get { return blackListedPorts; }
            set { blackListedPorts = value; }
        }
        
        void Awake()
        {
            Instance = this;
            Log.SetLogLevel(debugLevel);
            DiscoverPorts();
            if(readOnThread) StartThread();
        }

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
                StartCoroutine(FindBoardPort(portName));
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
                if (uduinoDevice.getStatus() == SerialArduino.SerialStatus.OPEN)
                {
                    string reading = uduinoDevice.ReadFromArduino("IDENTITY", 200);
                    if (reading != null && reading.Split(new char[0])[0] == "uduinoIdentity")
                    {
                        string name = reading.Split(new char[0])[1];
                        uduinoDevices.Add(name, uduinoDevice); //Add the new device to the devices array
                        if (!ReadOnThread) StartCoroutine(ReadSerial(name)); // Initiate the Async reading of variables 
                        Log.Info("Board <color=#ff3355>" + name + "</color> <color=#2196F3>[" + uduinoDevice.getPort() + "]</color> added to dictionnary");
                        uduinoDevice.UduinoFound();
                        break;
                    }
                    else
                    {
                        Log.Warning("Impossible to get name on <color=#2196F3>[" + portName + "]</color>. Retrying.");
                    }
                }
                yield return new WaitForSeconds(0.1f);    //Wait one frame
            } while (uduinoDevice.getStatus() != SerialArduino.SerialStatus.UNDEF && tries++ < discoverTries);

            if(uduinoDevice.getStatus() != SerialArduino.SerialStatus.FOUND)
            {
                Log.Error("Impossible to get name on <color=#2196F3>[" + portName + "]</color>. Closing.");
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



        /// <summary>
        /// Init a pin
        /// </summary>
        public void InitPin(int pin, PinMode mode)
        {

        }

        /// <summary>
        /// Send a read command to a specific arduino.
        /// A read command will be returned in the OnValueReceived() delegate function
        /// </summary>
        /// <param name="target">Target device name. Not defined means read everything</param>
        /// <param name="variable">Variable watched, if defined</param>
        /// <param name="timeout">Read Timeout, if defined </param>
        /// <param name="callback">Action callback</param>
        public void Read(string target = null, string variable = null, int timeout = 100, System.Action<string> action = null)
        {
            if (UduinoTargetExists(target))
            {
                uduinoDevices[target].read = variable;
                uduinoDevices[target].callback = action;
            }
            else
            {
                foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                {
                    uduino.Value.read = variable;
                    uduino.Value.callback = action;
                }
            }
        }

        public void Read(int pin, System.Action<string> action = null)
        {
            foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
            {
                uduino.Value.ReadFromArduino();
                uduino.Value.callback = action;
            }
        }

        /// <summary>
        /// Write a command on an Arduino
        /// </summary>
        /// <param name="target">Target device</param>
        /// <param name="message">Message to write in the serial</param>
        public void Write(string target = null, string message = null)
        {
            if (UduinoTargetExists(target))
                uduinoDevices[target].WriteToArduino(message);
            else
                foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                    uduino.Value.WriteToArduino(message);
        }

        /// <summary>
        /// Write a command on an Arduino
        /// </summary>
        /// <param name="target">Target device</param>
        /// <param name="message">Message to write in the serial</param>
        public void Write(int pin, float floatVal = -1f)
        {
            foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                uduino.Value.WriteToArduino(Mathf.Round(floatVal).ToString());
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
        /// Write a command on an Arduino with several commands and values
        /// </summary>
        /// TODO : To improve
        public void Write(string target = null, string[] message = null, int[] values = null) {
            if (UduinoTargetExists(target))
                uduinoDevices[target].AdvancedWriteToArduino(message, values);
            foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                uduino.Value.AdvancedWriteToArduino(message, values);
        }

        /// <summary>
        /// Verify if the target exists when we want to get a value
        /// </summary>
        /// <param name="target">Target Uduino Name</param>
        /// <returns>Re</returns>
        private bool UduinoTargetExists(string target)
        {
            UduinoDevice uduino = null;
            if (uduinoDevices.TryGetValue(target, out uduino))
                return true;
            else
            {
				//TODO: Restart a loop to find all objects
                Log.Warning("The object " + target + " cannot be found. Are you sure it's connected and correctly detected ?");
                return false;
            }
        }

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

        /// <summary>
        ///  Read the Serial Port data in a new thread.
        /// </summary>
        public void ReadPorts()
        {
            while (readAllPorts)
            {
                foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                {
                    if (uduino.Value.read != null)
                    {
                        string data = uduino.Value.ReadFromArduino(uduino.Value.read, 50);
                        uduino.Value.read = null;
                        ReadData(data, uduino.Key);
                    }
                }
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
        /// </summary>
        /// <param name="data">Received data</param>
        /// <param name="target">TODO : for the moment target is unused</param>
        void ReadData(string data, string target = null)
        {
            if (data != null && data != "" && data != "Null")
            {
                UduinoDevice uduino = uduinoDevices[target];
                uduino.lastRead = data;
                if (uduino.callback != null) uduino.callback(data);
                else OnValueReceived(data, target);
            }
        }

        /// <summary>
        /// Close all opened serial ports
        /// </summary>
        public void CloseAllPorts()
        {
            if (uduinoDevices.Count == 0)
            {
                 Log.Info("All ports are closed.");
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

    }
}