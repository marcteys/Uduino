using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Uduino
{

    [SerializeField]
    public class Pin
    {
        private UduinoManager manager = null;

        public string arduino = "";
        public string lastReadValue = "";

        private string[] allPin = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "A0", "A1", "A2", "A3", "A4", "A5" };

        public PinMode pinMode = PinMode.Output;
        private PinMode prevPinMode = PinMode.Output;

        public int currentPin = -1;
        private int prevPin = -1;

        public int sendValue = 0;
        private int prevSendValue = 0;

        public Pin(string arduinoParent)
        {
            manager = UduinoManager.Instance;
            arduino = arduinoParent;
            SendMessage("s " + currentPin + " " + (int)pinMode);
        }

        void SendMessage(string message)
        {
            manager.SendMessage(arduino, message);
        }


        void CheckChanges()
        {
            if (currentPin != prevPin)
            {
                SendMessage("s " + currentPin + " " + (int)pinMode);
                prevPin = currentPin;
            }

            if (pinMode != prevPinMode)
            {
                SendMessage("s " + currentPin + " " + (int)pinMode);
                prevPinMode = pinMode;
            }
        }

        public void Destroy()
        {
            SendMessage("w " + currentPin + " 0");
        }

        public virtual void Draw()
        {
        }

    }

}