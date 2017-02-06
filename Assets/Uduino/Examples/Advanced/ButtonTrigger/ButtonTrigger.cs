using UnityEngine;
using System.Collections;
using Uduino;

public class ButtonTrigger : MonoBehaviour
{

    UduinoManager u;

    void Awake()
    {
        UduinoManager.Instance.AlwaysRead("uduinoButton", ButonTrigger); 
    }

    void Update()
    {

    }

    void ButonTrigger(string data)
    {
        Debug.Log(data); // Use the data as you want !
    }
}