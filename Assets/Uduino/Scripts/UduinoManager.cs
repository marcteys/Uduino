using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;

/*
    TODO : Enum pour sélectionner le baudrAte (a faire sur l'éditor uniquement ? )
    TODO : public values for the number of tries ?
    TODO : Verbose mode
    TODO : Quand on le call et qu'il n'ets pas instancié, l'instancier sur la  scène
    TODO : Faire un script avec des "utils" our le OnValueReceived, comme parser un array de string, etc, convertir string to int, etc
 */

namespace Uduino
{
    public class UduinoManager : MonoBehaviour {
        
        /// <summary>
        /// Dictionnary containing all the connected Arduino devices
        /// </summary>
        public Dictionary<string, UduinoDevice> uduinoDevices = new Dictionary<string, UduinoDevice>();

        /// <summary>
        /// Create a delegate event to trigger the function OnValueReceived()
        /// Takes one parameter, the returned data.
        /// </summary>
        public delegate void OnValueReceivedEvent(string data);
        public event OnValueReceivedEvent OnValueReceived;

        // TODO for this two : crééer des helpers qui vont pouvoir filtrer par exemple [data]

        /// <summary>
        /// Create a delegate event to trigger the function OnExtendedValueReceivedEvent()
        /// </summary>
        public delegate void OnExtendedValueReceivedEvent(string[] data, string device, string vaiable);
        public event OnExtendedValueReceivedEvent OnExtendedValueReceived;

        /// <summary>
        /// Enable the reading of serial port in a different Thread.
        /// Might be usefull for optimization and not block the runtime during a reading. 
        /// </summary>
        public bool readOnThread = false;

        /// <summary>
        /// Uduino manager instance
        /// </summary>
        /// <value>The unique Uduino Manager Instance</value>
        public static UduinoManager Instance
        {
            get {
                return _instance;
            }
            set { _instance = value; }
        }
        private static UduinoManager _instance = null;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != null && Instance != this)
            {
                Debug.Log("You can only use one UduinoManager. Destroying the new one atached to the GameObject " + this.gameObject.name);
                Destroy(this.gameObject);
                return;
            }
            DiscoverPorts();
            if(readOnThread) StartThread();
        }

        /// <summary>
        /// Get the ports names, dependings of the current OS
        /// </summary>
        public void DiscoverPorts()
        {
        #if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                    Discover(new string[] {"/dev/ttyUSB0", "/dev/ttyUSB1", "/dev/ttyUSB2", "/dev/ttyUSB3", "/dev/ttyUSB4", "/dev/ttyUSB5"});
        #else
                    Discover(SerialPort.GetPortNames());
        #endif
        }

        /// <summary>
        /// Discover all active serial ports connected.
        /// When a new serial port is connected, send the IDENTITY request, to get the name of the arduino
        /// </summary>
        /// <param name="portNames">All Serial Ports names, dependings of the current OS</param>
        void Discover(string[] portNames)
        {
            if (portNames.Length == 0) Debug.Log("Are you sure your arduino is connected ?");

            foreach (string portName in portNames)
            {
                UduinoDevice uduinoDevice = new UduinoDevice(portName, 9600);
                int tries = 0;
                do
                {
                    if (uduinoDevice.getStatus() == SerialArduino.SerialStatus.OPEN)
                    {
                        //Changer ça et faire un read async, qui prend comme callback l'initialisation de Uniduino
                        string reading = uduinoDevice.ReadFromArduino("IDENTITY", 200);
                        if (reading != null && reading.Split(new char[0])[0] == "uduinoIdentity") 
                        {

                            uduinoDevices.Add(reading.Split(new char[0])[1], uduinoDevice); //Add the new device to the devices array
                            if (!readOnThread) StartCoroutine(ReadSerial(name)); // Initiate the Async reading of variables 
                            Debug.Log("Object <color=#ff3355>" + name + "</color> <color=#2196F3>[" + uduinoDevice.getPort() + "]</color> added to dictionnary");
                            break;
                        }
                        else
                        {
                            Debug.LogWarning("Impossible to get name on <color=#2196F3>[" + portName + "]</color>. Retrying (" + tries + "/10)");
                        }
                    }
                } while (uduinoDevice.getStatus() != SerialArduino.SerialStatus.UNDEF && tries++ < 10);

                if (uduinoDevice.getStatus() == SerialArduino.SerialStatus.UNDEF || uduinoDevice.getStatus() == SerialArduino.SerialStatus.CLOSE)
                {
                    uduinoDevice.Close();
                    uduinoDevice = null;
                }
            }
        }

        /// <summary>
        /// Debug all ports state.
        /// TODO : Really neccessary ? They are always open...
        /// </summary>
        public void GetPortState()
        {
            if (uduinoDevices.Count == 0)
            {
                Debug.Log("No port currently open");
            }
            foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
            {
                string state = uduino.Value.isSerialOpen() ? "open " : "closed";
                Debug.Log(uduino.Value.getPort() + " (" + uduino.Key + ")" + " is " + state);
            }
        }

        /// <summary>
        /// Send a read command to a specific arduino.
        /// A read command will be returned in the OnValueReceived() delegate function
        /// </summary>
        /// <param name="target">Target device name</param>
        /// <param name="variable">Variable watched, if defined</param>
        /// <param name="timeout">Read Timeout, if defined </param>
        public void Read(string target, string variable = null, int timeout = 100)
        {
            UduinoDevice uduino = null;
            if (uduinoDevices.TryGetValue(target, out uduino))
                uduino.read = variable;
            else ErrorNotConnected(target);
        }

        /// <summary>
        /// Write a specific string in the serial of a given arduino
        /// </summary>
        /// <param name="target">Target device</param>
        /// <param name="message">Message to write in the serial</param>
/// TODO : Overload this function !!
        public void Write(string target, string message)
        {
            UduinoDevice uduino = null;
            if (uduinoDevices.TryGetValue(target, out uduino))
                uduino.WriteToArduino(message);
            else ErrorNotConnected(target);
        }

        void ErrorNotConnected(string target)
        {
            Debug.LogError("Error ! The object " + target + " cannot be found. Are you sur it is connected and correctly set up ?");
        }

        /// <summary>
        /// new thread
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
                Debug.LogError(e);
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
                        ReadData(data);
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
                if (uduinoDevices[target].read != null)
                {
                    string data = uduinoDevices[target].ReadFromArduino(uduinoDevices[target].read, 50);
                    uduinoDevices[target].read = null;
                    yield return null;
                    ReadData(data);
                }
                else
                {
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Parse the received data
        /// TODO : Need extra work here
        /// </summary>
        /// <param name="data">Received data</param>
        void ReadData(string data)
        {
            if (data != null && data != "" && data != "Null")
            {
                OnValueReceived(data);
            }
        }

        /// <summary>
        /// Close all opened serial ports
        /// </summary>
        public void CloseAllPorts()
        {
            if (uduinoDevices.Count == 0)
            {
                Debug.Log("All ports are now closed");
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
                readAllPorts = false;
                _Thread.Abort();
            }
            CloseAllPorts();
        }

    }
}