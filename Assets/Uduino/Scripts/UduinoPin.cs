using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Uduino
{
    // We use a class Pin to optimize
    public class Pin
    {
        public UduinoManager manager = null;

        public string arduinoName = null; // TODO : we don't need that !! 
        public UduinoDevice uduinoDevice = null;

        public PinMode pinMode;
        public PinMode prevPinMode;

        public int currentPin = -1;
        public int prevSendValue = 0;

        public int lastReadValue = 0;

        public Pin(string arduinoParent, int pin, PinMode mode)
        {
            manager = UduinoManager.Instance;
            arduinoName = arduinoParent;
            currentPin = pin;
            pinMode = mode;
        }

        public void Init()
        {
            ChangePinMode(pinMode,"init");
        }

        public virtual void WriteReadMessage(string message)
        {
            manager.Write(arduinoName, message);
            //TODO : ref to bundle? 
            //TODO : Add ref to arduinocard
        }

        public virtual void WriteMessage(string message, string bundle = null)
        {
          manager.Write(arduinoName, message, bundle);
        }

        public bool PinTargetExists(string parentArduinoTarget, int currentPinTarget)
        {
            if ((arduinoName == null || arduinoName == "" || parentArduinoTarget == null || parentArduinoTarget == "" || parentArduinoTarget == arduinoName) && currentPinTarget == currentPin )
                return true;
            else
                return false;
        }

        /// <summary>
        /// Change Pin mode
        /// </summary>
        /// <param name="mode">Mode</param>
        public void ChangePinMode(PinMode mode, string bundle = null)
        {
            pinMode = mode;
            WriteMessage("s " + currentPin + " " + (int)pinMode, bundle);
        }

        /// <summary>
        /// Send OptimizedValue
        /// </summary>
        /// <param name="sendValue">Value to send</param>
        public virtual void SendRead(string bundle = null, System.Action<string> action = null)
        {
            string cmd = "r";
            if (bundle != null) cmd = "br";
            manager.Read(arduinoName, cmd + " " + currentPin, action: action, bundle: bundle);
        }

        /// <summary>
        /// Send OptimizedValue
        /// </summary>
        /// <param name="sendValue">Value to send</param>
        public void SendPinValue(int sendValue, string typeOfPin, string bundle = null)
        {
            if (sendValue != prevSendValue)
            {
                WriteMessage(typeOfPin + " " + currentPin + " " + sendValue, bundle);
                prevSendValue = sendValue;
            }
        }

        public void Destroy()
        {
            WriteMessage("d " + currentPin + " 0");
        }

        public virtual void Draw()
        {
            //Function overrided by the Editor
        }

    }
}