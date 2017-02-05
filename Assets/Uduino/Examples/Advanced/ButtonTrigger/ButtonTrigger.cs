using UnityEngine;
using System.Collections;
using Uduino;

public class ButtonTrigger : MonoBehaviour
{

    UduinoManager u;

    void Awake()
    {
        UduinoManager.Instance.OnValueReceived += OnValueReceived; //Create the Delegate
    //    UduinoManager.Instance.AutoRead("uduinoButton"); 
    }

    void Update()
    {

    }

    void OnValueReceived(string data, string device)
    {
        Debug.Log(data); // Use the data as you want !
    }
}