using UnityEngine;
using System;
using System.Collections;
using System.IO.Ports;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Uduino
{
    public class SerialArduino
    {

        private string _port;

        private int _baudrate;

        public SerialPort serial;

        SerialStatus serialStatus = SerialStatus.UNDEF;

        private System.Threading.Thread _Thread = null;
        private bool readArduino = true;

        public string read = null; // Value to Read

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
                #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                _port = "\\\\.\\" + _port;
                #endif
                serial = new SerialPort(_port, _baudrate, Parity.None, 8, StopBits.One);
                serial.ReadTimeout = 100;
                serial.WriteTimeout = 100;
                serial.Close();
                serial.Open();
                serialStatus = SerialStatus.OPEN;
                StartThread();

                Log.Info("Opening stream on port <color=#2196F3>[" + _port + "]</color>");
            }
            catch (Exception e)
            {
                Log.Error("Error on port <color=#2196F3>[" + _port + "]</color> : " + e);
                serialStatus = SerialStatus.CLOSE;
            }
        }

        #region Public functions
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
        /// A board with Uduino is found 
        /// </summary>
        public void UduinoFound()
        {
            serialStatus = SerialStatus.FOUND;
            #if UNITY_EDITOR
            if(Application.isPlaying) EditorUtility.SetDirty(UduinoManager.Instance);
            #endif
        }
        #endregion


        #region Thread 
        /// <summary>
        /// Initialisation of the Thread reading on Awake()
        /// </summary>
        void StartThread()
        {
            try
            {
                Log.Debug("Starting Thread for " + _port);
               _Thread = new System.Threading.Thread(ReadPorts);
               _Thread.Start();
            }
            catch (System.Threading.ThreadStateException e)
            {
                Log.Error(e);
            }
        }


        /// <summary>
        ///  Read the Serial Port data in a new thread.
        /// </summary>
        public void ReadPorts()
        {
            try
            {
                while (IsLooping())
                {
                    if (read != null)
                    {
                        string data = ReadFromArduino(read, 50);
                        read = null;
                        ReadData(data);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Unknown exception: " + e.Message + " " + e.StackTrace);
            }

        }

        public virtual void ReadData(string data) { }


        /// <summary>
        /// Retreive the Data from the Serial Prot using Unity Coroutines
        /// </summary>
        /// <param name="target"></param>
        /// <returns>null</returns>
        /// TODO : Launch it from 
        public IEnumerator ReadSerialCoroutine(string target)
        {
            while (true)
            {
                if (read != null)
                {
                    string data = ReadFromArduino(read, 50);
                    read = null;
                    yield return null;
                    ReadData(data);
                }
                else
                {
                    yield return null;
                }
            }
        }

        #endregion
        #region Commands

        /// <summary>
        /// Write a message to a serial port
        /// </summary>
        /// <param name="message">Message to write on this arduino serial</param>
        public void WriteToArduino(string message, object value = null)
        {

            if (serial == null || !serial.IsOpen || message == null || message == "" )
                return;

            if (value != null)
                message = " " + value.ToString();

            try
            {
                try
                {
                    serial.WriteLine(message + "\r\n");
                    serial.BaseStream.Flush();
                    Log.Info("<color=#4CAF50>" + message + "</color> is sent to <color=#2196F3>[" + _port + "]</color>");
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
        /// Read Arduino serial port
        /// </summary>
        /// <param name="message">Write a message to the serial port before reading the serial</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <returns>Read data</returns>
        public string ReadFromArduino(string message = null, int timeout = 10)
        {
          //  if (readInProcess)
            //    return null;
            if (message != null)
                WriteToArduino(message);

            if (serial == null || !serial.IsOpen)
                return null;

            serial.ReadTimeout = timeout;
            serial.DiscardInBuffer(); // TODO : To remove ?
            serial.DiscardOutBuffer();

            try
            {
                try
                {
                    string readedLine = serial.ReadLine();
                    ReadingSuccess(readedLine);
                    return readedLine;
                }
                catch (TimeoutException e)
                {
                 //   Log.Warning("Error for message: " + message);
                   // Log.Warning(e);
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
        /// Callback function when a message is written 
        /// </summary>
        /// <param name="message">Message successfully writen</param>
        public virtual void WritingSuccess(string message) { }

        /// <summary>
        /// Callback function when a message is read 
        /// </summary>
        /// <param name="message">Message successfully read</param>
        public virtual void ReadingSuccess(string message) { }
        #endregion

        #region Close

        /// <summary>
        /// Close Serial port 
        /// </summary>
        public void Close()
        {
            CloseThread();
            if (serial != null && serial.IsOpen)
            {
                Log.Warning("Closing port : <color=#2196F3>[" + _port + "]</color>");
                serial.Close();
                serialStatus = SerialStatus.CLOSE;
                serial = null;
            }
            else
            {
                Log.Info(_port + " already closed.");
            }
        }


        public bool IsLooping()
        {
            lock (this)
            {
                return readArduino;
            }
        }

        public void StopThread()
        {
            lock (this)
            {
                readArduino = false;
            }
        }

        public void CloseThread()
        {
            StopThread();
            if (_Thread != null) {
                _Thread.Join();
            }
            _Thread = null;
            Log.Debug("Killing Thread of " + _port);
        }
        #endregion

    }
}