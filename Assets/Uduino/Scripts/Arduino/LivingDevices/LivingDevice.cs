using UnityEngine;
using System.Collections;

public class LivingDevice
{
    private SerialArduino serialArduino;

    public LivingDevice(SerialArduino sA)
    {
        this.serialArduino = sA;
    }

    public SerialArduino getSerial() {
        return serialArduino;
    }

    public void Reset()
    {
        serialArduino.WriteToArduino("R");
    }

    public virtual void Calibrate()
    {
        serialArduino.WriteToArduino("C");
    }

    public void Close()
    {
        serialArduino.WriteToArduino("STOP");
        serialArduino.Close();
    }

    public string ReadFromArduino(int timeout)
    {
        return serialArduino.ReadFromArduino(null, timeout);
    }

    public void SendCommand(char command)
    {
        serialArduino.WriteToArduino(command.ToString());
    }

    public void SendCommand(char command, int value)
    {
        serialArduino.WriteToArduino(command + " " + value);
    }

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
