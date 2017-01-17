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
                if (_name == "") _name = value;
            }
        }
        private string _name = "";

        public bool continuousRead = false;
        public string read = null;

        public string lastRead = null;
        public string lastWrite = null;

        public System.Action<string> callback = null;

        private List<Pin> pins = new List<Pin>();

        private Dictionary<string, string[]> bundles = new Dictionary<string, string[]>();
         
        public UduinoDevice(string port, int baudrate = 9600)
            : base(port, baudrate)
        {

        }

        public void AdvancedWriteToArduino(string[] command, int[] value)
        {
            string buffer = "";
            for (int i = 0; i < value.Length; i++)
            {
                buffer += command[i] + " " + value[i] + " ";
            }
            buffer += '\n';
            WriteToArduino(buffer);
        }


        /*
        public void AddToBundle( string message )
        {

        }*/

        public void SendBundle(string bundleName)
        {
            /*
            string[] bundleValues;

            if (bundles.TryGetValue(bundleName, out bundleValues))
            {
                //success!


                Log.Info("Sending the bundle " + bundleName);
                bundles.Remove(bundleName); 

            }
            else
            {
                Log.Warning("You are tring to send the Bundle " + bundleName + " but it seems that it's empty.");
            }
            */
        }

        public override void WritingSuccess(string message)
        {
            lastWrite = message;
        }


        public override void ReadingSuccess(string message)
        {
            
        }

    }
}