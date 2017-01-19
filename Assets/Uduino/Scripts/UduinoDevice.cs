using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Uduino
{
    public class UduinoDevice : SerialArduino
    {
        public string name {
            get
            {
                return _name;
            } set
            {
                if (_name == "")
                    _name = value;
            }
        }
        private string _name = "";

        public bool continuousRead = false;
        public string read = null;

        public string lastRead = null;
        public string lastWrite = null;

        public System.Action<string> callback = null;

        private List<Pin> pins = new List<Pin>();

        private Dictionary<string, List<string>> bundles = new Dictionary<string, List<string>>();
         
        public UduinoDevice(string port, int baudrate = 9600) : base(port, baudrate) { }

        /// <summary>
        /// Add a message to the bundle
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="bundle">Bundle Name</param>
        public void AddToBundle( string message , string bundle)
        {
            List<string> existing;
            if (!bundles.TryGetValue(bundle, out existing))
            {
                existing = new List<string>();
                bundles[bundle] = existing;
            }
            existing.Add(","+ message );
            Log.Info("Message " + message + " added to the bundle " + bundle);
        }

        /// <summary>
        /// Send a Bundle to the arduino
        /// </summary>
        /// TODO : Max Length, matching avec arduino
        /// <param name="bundleName">Name of the bundle to send</param>
        public void SendBundle(string bundleName)
        {
            List<string> bundleValues;

            if (bundles.TryGetValue(bundleName, out bundleValues))
            {
                string fullMessage = "b " + bundleValues.Count;

                for (int i = 0; i < bundleValues.Count; i++)
                    fullMessage += bundleValues[i];

                WriteToArduino(fullMessage);
                bundles.Remove(bundleName);
            }
            else
            {
                Log.Warning("You are tring to send the Bundle " + bundleName + " but it seems that it's empty.");
            }
        }

        public override void WritingSuccess(string message)
        {
            lastWrite = message;
        }

        public override void ReadingSuccess(string message)
        {
        //    Debug.Log("last read : " + message);
            lastRead = message;
        }

    }
}