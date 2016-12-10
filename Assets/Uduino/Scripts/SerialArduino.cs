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
                #if UNITY_STANDALONE
                //	_port = "\\\\.\\" + _port; // TODO : For com port greater than 8 !
                #endif
                serial = new SerialPort(_port, _baudrate, Parity.None, 8, StopBits.One);
                serial.ReadTimeout = 100;
                serial.WriteTimeout = 100;
                serial.Close();
                serial.Open();
                serialStatus = SerialStatus.OPEN;
                Log.Warning("Opening stream on port <color=#2196F3>[" + _port + "]</color>");
            }
            catch (Exception e)
            {
                Log.Error("Error on port <color=#2196F3>[" + _port + "]</color> : " + e);
                serialStatus = SerialStatus.CLOSE;
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
            if (serial == null || !serial.IsOpen || message == null || message == "" ) return;
            try
            {
                Log.Info("<color=#4CAF50>" + message + "</color> is sent to <color=#2196F3>[" + _port + "]</color>");
                try
                {
                    serial.WriteLine(message + "\r\n");
                    serial.BaseStream.Flush();
                }
                catch (System.IO.IOException e) {
                    Log.Warning("Impossible to send a message to <color=#2196F3>[" + _port + "]</color>," + e);
                    Close();
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                Close();
            }
            WritingSuccess(message);
        }

        /// <summary>
        /// Callback function when a message is written 
        /// </summary>
        /// <param name="message">Message successfully writen</param>
        public virtual void WritingSuccess(string message)
        {
        }

        /// <summary>
        /// Read Arduino serial port
        /// </summary>
        /// <param name="message">Write a message to the serial port before reading the serial</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <returns>Read data</returns>
        public string ReadFromArduino(string message = null, int timeout = 10)
        {
            WriteToArduino(message);

            if (serial == null) return null;

            serial.ReadTimeout = timeout;
            serial.DiscardInBuffer(); // TODO : To remove ?
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
                Log.Error(e);
                Close();
                return null;
            }
        }

        /// <summary>
        /// Close Serial port 
        /// </summary>
        public void Close()
        {
            if (serial != null && serial.IsOpen)
            {
                Log.Warning("Closing port : <color=#2196F3>[" + _port + "]</color>");
                serial.Close();
                serialStatus = SerialStatus.CLOSE;
                serial = null;
            }
            else
            {
                Log.Warning(_port + " already closed.");
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