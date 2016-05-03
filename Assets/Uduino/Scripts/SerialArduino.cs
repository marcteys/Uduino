using UnityEngine;
using System;
using System.Collections;
using System.IO.Ports;

namespace Uduino
{
    public class SerialArduino
    {
        public enum SerialStatus
        {
            UNDEF,
            OPEN,
            CLOSE
        };

        private string _port;

        private int _baudrate;

        public SerialPort serial;

        SerialStatus serialStatus = SerialStatus.UNDEF;

        public SerialArduino(string port, int baudrate = 9600)
        {
            _port = port;
            _baudrate = baudrate;
            this.Open();
        }

        /// <summary>
        /// Open a specific serial port
        /// </summary>
        public void Open()
        {
            try
            {
                serial = new SerialPort("\\\\.\\" + _port, _baudrate);
                serial.ReadTimeout = 100;
                serial.Close();
                serial.Open();
                serialStatus = SerialStatus.OPEN;
                if (UduinoManager.DebugInfos) Debug.LogWarning("Opening stream on port <color=#2196F3>[" + _port + "]</color>");
            }
            catch (Exception e)
            {
                serialStatus = SerialStatus.CLOSE;
                if (UduinoManager.DebugInfos) Debug.Log("Error on port <color=#2196F3>[" + _port + "]</color> : " + e);
            }
        }

        /// <summary>
        /// Return port status 
        /// </summary>
        /// <returns>SerialArduino.SerialStatus</returns>
        public SerialStatus getStatus()
        {
            return serialStatus;
        }

        /// <summary>
        /// Return serial port 
        /// </summary>
        /// <returns>Current opened com port</returns>
        public string getPort()
        {
            return _port;
        }

        /// <summary>
        /// Write a message to a serial port
        /// </summary>
        /// <param name="message">Message to write on this arduino serial</param>
        public void WriteToArduino(string message)
        {
            try
            {
                if (UduinoManager.DebugInfos) Debug.LogWarning("<color=#4CAF50>" + message + "</color> is sent to <color=#2196F3>[" + _port + "]</color>");
                serial.WriteLine(message);
                serial.BaseStream.Flush();
            }
            catch (Exception e)
            {
                Close();
                if (UduinoManager.DebugInfos) Debug.Log(e);
            }
        }

        /// <summary>
        /// Read Arduino serial port
        /// </summary>
        /// <param name="message">Write a message to the serial port before reading the serial</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <returns>Read data</returns>
        public string ReadFromArduino(string message = null, int timeout = 10)
        {
            if (message != "" || message != null)
                WriteToArduino(message);

            serial.ReadTimeout = timeout;
            serial.DiscardInBuffer();
            serial.DiscardOutBuffer();

            try
            {
                try
                {
                    return serial.ReadLine();
                }
                catch (TimeoutException)
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                if (UduinoManager.DebugInfos) Debug.Log(e);
                Close();
                return null;
            }
        }

        /// <summary>
        /// Close Serial port 
        /// </summary>
        public void Close()
        {
            if (serial.IsOpen)
            {
                if (UduinoManager.DebugInfos) Debug.Log("Closing port : <color=#2196F3>[" + _port + "]</color>");
                serial.Close();
                serialStatus = SerialStatus.CLOSE;
                serial = null;
            }
            else
            {
                if (UduinoManager.DebugInfos) Debug.Log(_port + " already closed.");
            }
        }

        /// Specal Handler when application quit;
        private bool isApplicationQuitting = false;

        void OnDisable()
        {
            if (isApplicationQuitting) return;
            Close();
        }

        void OnApplicationQuit()
        {
            isApplicationQuitting = true;
        }

    }
}