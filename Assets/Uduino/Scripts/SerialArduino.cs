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
    public System.Threading.Thread _Thread = null;

    public SerialArduino(string port, int baudrate = 9600)
    {
        _port = port;
        _baudrate = baudrate;
        this.Open();
        try
        {
       //    _Thread = new System.Threading.Thread(Read);
          // _Thread.Start();
        }
        catch (System.Threading.ThreadStateException e)
        {
            Debug.LogError(e);
        }
    }

    public string tread = null;
    public bool running = true;

    public string ReadPort()
    {
        while (running)
        {
            if (tread != null)
            {
                tread = null;
                return ReadFromArduino(tread);
            }
            else
            {
                return null;
            }
        }
        return null;

    }

    public void TRead(string variable)
    {
        tread = variable;
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

    public string ReadFromArduino(string variable, int timeout = 10)
    {
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


    public bool isReading = false;
    public IEnumerator AsynchronousReadFromArduino(Action<object> callback, Action<string> fail = null, float timeout = float.PositiveInfinity)
    {

        DateTime initialTime = DateTime.Now;
        DateTime nowTime;
        TimeSpan diff = default(TimeSpan);

        if (!isReading)
        {
            isReading = true;
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
                    isReading = false;
                    yield return null;
                }
                else
                    yield return new WaitForSeconds(0.05f);

                nowTime = DateTime.Now;
                diff = nowTime - initialTime;

            } while (diff.Milliseconds < timeout);

            if (fail != null && dataString == null)
            {
                isReading = false;
                fail(_port);
            }
        }


        yield return null;
    }

    public void Close()
    {
        WriteToArduino("STOP");
        running = false;
     //   _Thread.Abort();
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