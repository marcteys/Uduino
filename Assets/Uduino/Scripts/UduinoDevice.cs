using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Uduino
{
    public class UduinoDevice : SerialArduino
    {
        public bool continuousRead = false;
        public string read = null;

        public string lastRead = null;
        public string lastWrite = null;

        public System.Action<string> callback = null;

        private List<Pin> pins = new List<Pin>();

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

        public override void WritingSuccess(string message)
        {
            lastWrite = message;
        }


        public override void ReadingSuccess(string message)
        {
            //TODO : ne sert a rien pour l'instant
        }

    }
}