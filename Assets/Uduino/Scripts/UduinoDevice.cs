using UnityEngine;
using System.Collections;

namespace Uduino
{
    public class UduinoDevice : SerialArduino
    {
        public bool read = false;

        public UduinoDevice(string port, int baudrate = 9600)
            : base(port, baudrate)
        {

        }

        public void SendCommand(char command) { WriteToArduino(command.ToString()); }
        public void SendCommand(char command, int value) { WriteToArduino(command + " " + value); }
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


        /*

        public void AsynchronousReadFromArduino(int timeout = 10)
        {
           // serialArduino.AsynchronousReadFromArduino(timeout);
        }

        public string TReadFromArduino(string variable, int timeout = 10)
        {
            serialArduino.TRead(variable);

            return "e";
           // return serialArduino.TReadFromArduino(variable, timeout);
        }

        

        public string ReadFromArduino(string variable, int timeout = 10)
        {
            return serialArduino.ReadFromArduino(variable, timeout);
        }


   */
       
    }
}