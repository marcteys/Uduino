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
    private SerialPort stream;
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
            stream = new SerialPort(_port, _baudrate);
            stream.ReadTimeout = 50;
            stream.Close();
            stream.Open();
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
        return stream;
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
            stream.WriteLine(message);
            stream.BaseStream.Flush();
        }
        catch (Exception e)
        {
            Close();
            Debug.Log(e);
        }

    }

    public string ReadFromArduino(int timeout = 0)
    {
        try
        {
            stream.ReadTimeout = timeout;
            try
            {
                return stream.ReadLine();
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

    public string WriteToArduinoThenRead(string message, int timeout = 2)
    {
        stream.WriteLine(message);
        stream.BaseStream.Flush();

        stream.ReadTimeout = timeout;
        try
        {
            return stream.ReadLine();
        }
        catch (TimeoutException)
        {
            return null;
        }
    }


    public IEnumerator AsynchronousReadFromArduino(Action<string> callback, Action<string> fail = null, float timeout = float.PositiveInfinity)
    {
        DateTime initialTime = DateTime.Now;
        DateTime nowTime;
        TimeSpan diff = default(TimeSpan);

        string dataString = null;
       
        do
        {
            try
            {
                dataString = stream.ReadLine();
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
        if (stream.IsOpen)
        {
            Debug.Log("Closing port : <color=#2196F3>[" + _port + "]</color>");
            stream.Close();
            serialStatus = SerialStatus.CLOSE;
        }
        else
        {
            Debug.Log(_port + " already closed.");
        }
    }


    //Specal Handler when application qui;
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