using UnityEngine;
using System.Collections;

namespace Uduino
{
    public class UduinoDevice : SerialArduino
    {
        public string read = null;
        public string write = null;
        public System.Action<string> callback = null;

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
            write = message;
        }
    }
}