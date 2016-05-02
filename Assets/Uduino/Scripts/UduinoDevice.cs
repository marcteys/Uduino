using UnityEngine;
using System.Collections;

namespace Uduino
{
    public class UduinoDevice : SerialArduino
    {
        public string read = null;

        public UduinoDevice(string port, int baudrate = 9600)
            : base(port, baudrate)
        {

        }

        public void Write(string command) { WriteToArduino(command); }
        public void SendCommand(string command, int value) { WriteToArduino(command + " " + value); }
        // TODO : refaire ça avec un nombre illimité de paramètre  // enelever "nb"  mais faire un length  de command ou value
        public void SendCommand(char[] command, int[] value, int nb)
        {
            string buffer = "";
            for (int i = 0; i < nb; i++)
            {
                buffer += command[i] + "" + value[i] + " ";
            }
            buffer += '\n';
            WriteToArduino(buffer);
        }
       
    }
}