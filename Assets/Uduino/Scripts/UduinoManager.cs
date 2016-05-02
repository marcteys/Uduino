using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;


namespace Uduino
{
    public class UduinoManager : MonoBehaviour {
        
        // TODO : Enum pour sélectionner le baudrAte (a faire sur l'éditor uniquement ? )
        // TODO : public values for the number of tries ?
        // TODO : Verbose mode
        // TODO : Quand on le call et qu'il n'ets pas instancié, l'instancier sur la  scène
        public Dictionary<string, UduinoDevice> uduinoDevices = new Dictionary<string, UduinoDevice>();


        public delegate void OnValueReceivedEvent(object data);
        public event OnValueReceivedEvent OnValueReceived;

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


        public bool readOnThread = false;
        private System.Threading.Thread _Thread = null;
        private bool readAllPorts = true;
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
        ///  Thread function
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
                        if (data != null || data != "")
                        {
                            OnValueReceived((object)data);
                        }
                    }
                }
            }
        }

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
                        string reading = uduinoDevice.ReadFromArduino("IDENTITY", 100);
                        if (reading != null && reading.Split(new char[0])[0] == "uduinoIdentity") 
                        {
                            this.ArduinoFound(reading.Split(new char[0])[1], uduinoDevice);
                            break;
                        }
                        else
                        {
                            Debug.LogWarning("Impossible to get name on <color=#2196F3>[" + portName + "]</color>. Retrying (" + tries + "/10)");
                        }
                    }
                } while (uduinoDevice.getStatus() != SerialArduino.SerialStatus.UNDEF && tries++ < 50);

                if (uduinoDevice.getStatus() == SerialArduino.SerialStatus.UNDEF || uduinoDevice.getStatus() == SerialArduino.SerialStatus.CLOSE)
                {
                    uduinoDevice = null;
                }
            }
        }

        void ArduinoFound(string name, UduinoDevice uduinoDevice)
        {
            uduinoDevices.Add(name, uduinoDevice);
             if (!readOnThread) StartCoroutine(ReadSerial(name));
            Debug.Log("Object <color=#ff3355>" + name + "</color> <color=#2196F3>[" + uduinoDevice.getPort() + "]</color> added to dictionnary");
        }

        public void DiscoverPorts()
        {
        #if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            Discover(new string[] {"/dev/ttyUSB0", "/dev/ttyUSB1", "/dev/ttyUSB2", "/dev/ttyUSB3", "/dev/ttyUSB4", "/dev/ttyUSB5"});
        #else
            Discover(SerialPort.GetPortNames());
        #endif
        }

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

        public void SendCommand(string target, string message)
        {
            uduinoDevices[target].WriteToArduino(message);
        }


        public void Read(string target, string variable = null, int timeout = 100)
        {
            uduinoDevices[target].read = variable;
        }


        public IEnumerator ReadSerial(string target)
        {
            while (true)
            {
                if (uduinoDevices[target].read != null)
                {
                    string data = uduinoDevices[target].ReadFromArduino(uduinoDevices[target].read, 50);
                    uduinoDevices[target].read = null;

                    yield return null;
                    if (data != null || data != "")
                    {
                        OnValueReceived((object)data);
                    }
                }
                else
                {
                    yield return null;
                }
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
