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
                        //serialObject.WriteToArduino("I");
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
            StartCoroutine(ReadSerial(name, "lol"));
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

        /// <summary>
        /// 
        /// </summary>
        public void SendCommand(string target, string message)
        {
            uduinoDevices[target].WriteToArduino(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /*
        public string Read(string target, string variable = null, int timeout = 100)
        {
            return uduinoDevices[target].ReadFromArduino(variable, timeout);
        }
        */


        public void Read(string target, string variable = null, int timeout = 100)
        {
          //  StartCoroutine(ReadSerial(target, variable));
        }

        public string lol = "caca";

        public void ARead(string target, int timeout = 100)
        {
            StartCoroutine
            (
                uduinoDevices[target].AsynchronousReadFromArduino
                ( (object s) => lol = (string)s,     // Callback
                    (string s) => Debug.Log(s),   // Error callback
                    1f )                             // Timeout (seconds)
            );
        }

        public string TRead(string target, string variable = null)
        {
         //   uduinoDevices[target].TReadFromArduino(variable);
            return null;
        }

        public void OnDisable()
        {
            CloseAllPorts();
        }
      

        public int nbrCoroutines = 0;
        public IEnumerator ReadSerial(string target, string variable)
        {
            string data = uduinoDevices[target].ReadFromArduino(variable, 100);
            if (data != null)
            {
               // OnValueReceived(uduinoDevices[target].ReadFromArduino(variable, 100));
            }
            //nbrCoroutines = 0;
            yield return null;
        }

    }
}
