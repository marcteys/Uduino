using UnityEngine;
using System.Collections;

namespace Uduino
{
    public class UduinoDevice
    {
        private SerialArduino serialArduino;

        public UduinoDevice(SerialArduino sA)
        {
            this.serialArduino = sA;
        }

        public SerialArduino getSerial() {
            return serialArduino;
        }

        public void Close()
        {
            serialArduino.WriteToArduino("STOP");
            serialArduino.Close();
        }

        public void AsynchronousReadFromArduino(int timeout = 10)
        {
           // serialArduino.AsynchronousReadFromArduino(timeout);
        }


        public string ReadFromArduino(string variable, int timeout = 10)
        {
            return serialArduino.ReadFromArduino(variable, timeout);
        }

        public void SendCommand(char command)
        {
            serialArduino.WriteToArduino(command.ToString());
        }

        public void SendCommand(char command, int value)
        {
            serialArduino.WriteToArduino(command + " " + value);
        }

        // TODO : refaire ça avec un nombre illimité de paramètre  // enelever "nb"  mais faire un length  de command ou value
        public void SendCommand(char[] command, int[] value, int nb)
        {
            string buffer = "";
            for (int i = 0; i < nb; i++)
            {
                buffer += command[i] + "" + value[i] + " ";
            }
            buffer += '\n';
            serialArduino.WriteToArduino(buffer);
        }

        void OnDisable()
        {
            Close();
        }
    }
}