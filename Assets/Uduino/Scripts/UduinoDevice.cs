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

        // TODO : refaire ça avec un nombre illimité de paramètre  // enelever "nb"  mais faire un length  de command ou value
        public void AdvancedWriteToArduino(string[] command, int[] value, int nb)
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