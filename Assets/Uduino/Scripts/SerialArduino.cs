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

        private bool readInProcess = false;

        private Queue readQueue, writeQueue, messagesToRead;
        int maxQueueLength = 100;

        public SerialArduino(string port, int baudrate = 9600)
        {
            _port = port;
            _baudrate = baudrate;

            readQueue = Queue.Synchronized(new Queue());
            writeQueue = Queue.Synchronized(new Queue());
            messagesToRead = Queue.Synchronized(new Queue());

            Open();
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
                serial.WriteTimeout = 50;
                serial.Close();
                serial.Open();
                serialStatus = SerialStatus.OPEN;

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

        #region Commands

        /// <summary>
        /// Write a message to a serial port
        /// </summary>
        /// <param name="message">Message to write on this arduino serial</param>
        public void WriteToArduino(string message, object value = null)
        {

            if (message == null || message == "" )
                return;

            if (value != null)
                message = " " + value.ToString();

            if(!writeQueue.Contains(message) && writeQueue.Count < maxQueueLength)
                writeQueue.Enqueue(message);
        }

        /// <summary>
        /// Loop every thead request to write a message on the arduino (if any)
        /// </summary>
        public void WriteToArduinoLoop()
        {

            if (serial == null || !serial.IsOpen)
                return;

            if (writeQueue.Count == 0)
                return;

            string message = (string)writeQueue.Dequeue();

            try
            {
                try
                {
                    serial.WriteLine(message + "\r\n");
                    serial.BaseStream.Flush();
                    Log.Info("<color=#4CAF50>" + message + "</color> is sent to <color=#2196F3>[" + _port + "]</color>");
                }
                catch (System.IO.IOException e)
                {
                    writeQueue.Enqueue(message);
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
        public string ReadFromArduino(string message = null, int timeout = 200)
        {
            if (serial == null || !serial.IsOpen)
                return null;

            //TODO : Better ?
            serial.ReadTimeout = timeout;

            if (message != null)
                messagesToRead.Enqueue(message);

            if (readQueue.Count == 0)
                return null;

            string finalMessage = (string)readQueue.Dequeue();

            return finalMessage;
        }


        public void ReadFromArduinoLoop()
        {
            if (serial == null || !serial.IsOpen)
                return;

            if (messagesToRead.Count > 0)
                WriteToArduino((string)messagesToRead.Dequeue());
            else
            {
                //TODO "It read a message only if a message is sent");
                return;
            }

            try
            {
                try
                {
                    string readedLine = serial.ReadLine();
                    ReadingSuccess(readedLine);
                    if (readedLine != null && readQueue.Count < maxQueueLength)
                        readQueue.Enqueue(readedLine);
                }
                catch (TimeoutException e)
                {
                    Log.Info(e);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                Close();
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
            readQueue.Clear();
            writeQueue.Clear();
            messagesToRead.Clear();

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
        #endregion

    }
}