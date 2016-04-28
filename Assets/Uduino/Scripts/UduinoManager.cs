using UnityEngine;
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

        Dictionary<string, UduinoDevice> uduinoDevices = new Dictionary<string, UduinoDevice>();

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
                SerialArduino serialObject = new SerialArduino(portName, 9600);
                int tries = 0;
                do
                {
                    if (serialObject.getStatus() == SerialArduino.SerialStatus.OPEN)
                    {
                        //serialObject.WriteToArduino("I");
                        string reading = serialObject.ReadFromArduino("I", 100);
                        if (reading != null && reading.Split(new char[0])[0] == "uduinoIdentity") 
                        {
                            this.ArduinoFound(reading.Split(new char[0])[1], serialObject);
                            break;
                        }
                        else
                        {
                            Debug.LogWarning("Impossible to get name on <color=#2196F3>[" + portName + "]</color>. Retrying (" + tries + "/50)");
                        }
                    }
                } while (serialObject.getStatus() != SerialArduino.SerialStatus.UNDEF && tries++ < 50);

                if (serialObject.getStatus() == SerialArduino.SerialStatus.UNDEF || serialObject.getStatus() == SerialArduino.SerialStatus.CLOSE)
                {
                    serialObject = null;
                }
            }
        }

        void ArduinoFound(string name, SerialArduino serialArduino)
        {
            uduinoDevices.Add(name, new UduinoDevice(serialArduino));
            Debug.Log("Object <color=#ff3355>" + name + "</color> <color=#2196F3>[" + serialArduino.getPort() + "]</color> added to dictionnary");
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
                 SerialArduino device = uduinoDevices[key].getSerial();
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
            foreach (KeyValuePair<string, UduinoDevice> liv in uduinoDevices)
            {
                SerialArduino device = liv.Value.getSerial();
                string state = device.getStream().IsOpen ? "open " : "closed";
                Debug.Log(device.getPort() + " (" + liv.Key + ")" + " is " + state);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public void SendCommand(string target, string message)
        {
            uduinoDevices[target].getSerial().WriteToArduino(message);
        }

        /// <summary>
        /// 
        /// </summary>
        public string Read(string target, string variable = null, int timeout = 100)
        {
            return uduinoDevices[target].ReadFromArduino(variable, timeout);
        }


        public void OnDisable()
        {
            CloseAllPorts();
        }
    }
}
