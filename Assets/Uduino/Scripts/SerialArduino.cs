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
    private SerialPort serial;
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
            serial = new SerialPort(_port, _baudrate);
            serial.ReadTimeout = 50;
            serial.Close();
            serial.Open();
            serialStatus = SerialStatus.OPEN;
            Debug.LogWarning("Opening stream on port <color=#2196F3>[" + _port + "]</color>");
        }
        catch (Exception e)
        {
            serialStatus = SerialStatus.CLOSE;
            Debug.Log(e);
            Debug.Log("Error on port <color=#2196F3>[" + _port + "]</color>");
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

    public string ReadFromArduino(string variable, int timeout = 10)
    {
        Debug.Log(variable);
        WriteToArduino(variable);
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

    public string WriteToArduinoThenRead(string message, int timeout = 10)
    {
        serial.WriteLine(message);
        serial.ReadTimeout = timeout;
        try
        {
            return serial.ReadLine();
        }
        catch (TimeoutException)
        {
            return null;
        }
    }


    public IEnumerator AsynchronousReadFromArduino(Action<object> callback, Action<string> fail = null, float timeout = float.PositiveInfinity)
    {
        DateTime initialTime = DateTime.Now;
        DateTime nowTime;
        TimeSpan diff = default(TimeSpan);

        string dataString = null;
       
        do
        {
            try
            {
                dataString = serial.ReadLine();
            }
            catch (TimeoutException)
            {
                dataString = null;
            }

            if (dataString != null)
            {
                callback(dataString);
                yield return null;
            }
            else
                yield return new WaitForSeconds(0.05f);

            nowTime = DateTime.Now;
            diff = nowTime - initialTime;

        } while (diff.Milliseconds < timeout);

        if (fail != null && dataString == null)
            fail(_port);

        yield return null;
    }

    public void Close()
    {
        if (serial.IsOpen)
        {
            Debug.Log("Closing port : <color=#2196F3>[" + _port + "]</color>");
            serial.Close();
            serialStatus = SerialStatus.CLOSE;
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