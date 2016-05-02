using UnityEngine;
using System;
using System.Collections;
using System.IO.Ports;

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

    public void Open()
    {
        try
        {
            serial = new SerialPort("\\\\.\\" + _port, _baudrate);
            serial.ReadTimeout = 100;
            serial.Close();
            serial.Open();
            serialStatus = SerialStatus.OPEN;
            Debug.LogWarning("Opening stream on port <color=#2196F3>[" + _port + "]</color>");
        }
        catch (Exception e)
        {
            serialStatus = SerialStatus.CLOSE;
            Debug.Log("Error on port <color=#2196F3>[" + _port + "]</color> : " + e);
        }
    }

    public SerialStatus getStatus()
    {
        return serialStatus;
    }

    public SerialPort getStream()
    {
        return serial;
    }

    public string getPort()
    {
        return _port;
    }

    public bool isSerialOpen()
    {
        return serial.IsOpen;
    }

    public void WriteToArduino(string message)
    {
        try
        {
            Debug.LogWarning("<color=#4CAF50>" + message + "</color> is sent to <color=#2196F3>[" + _port + "]</color>");
            serial.WriteLine(message);
            serial.BaseStream.Flush();
        }
        catch (Exception e)
        {
            Close();
            Debug.Log(e);
        }
    }

    public string ReadFromArduino(string message = null, int timeout = 10)
    {
        if(message != "" || message != null)
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
            Debug.Log(e);
            Close();
            return null;
        }
    }

    public void Close()
    {
        WriteToArduino("STOP");
        if (serial.IsOpen)
        {
            Debug.Log("Closing port : <color=#2196F3>[" + _port + "]</color>");
            serial.Close();
            serialStatus = SerialStatus.CLOSE;
            serial = null;
        }
        else
        {
            Debug.Log(_port + " already closed.");
        }
    }

    //Specal Handler when application quit;
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