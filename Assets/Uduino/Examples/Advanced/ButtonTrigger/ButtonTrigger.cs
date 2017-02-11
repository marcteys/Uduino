using UnityEngine;
using System.Collections;
using Uduino;

public class ButtonTrigger : MonoBehaviour
{
    public GameObject button;

    UduinoManager u;

    void Awake()
    {
     //   UduinoManager.Instance.OnValueReceived += OnValueReceived; //Create the Delegate

      //  UduinoManager.Instance.AlwaysRead("uduinoButton");
        UduinoManager.Instance.AlwaysRead("uduinoButton", ButonTrigger);
    }

    void PressDown()
    {
        button.GetComponent<Renderer>().material.color = Color.red;
        button.transform.Translate(Vector3.down / 10);

    }

    void PressUp()
    {
        button.GetComponent<Renderer>().material.color = Color.green;
        button.transform.Translate(Vector3.up / 10);
    }

    void OnValueReceived(string data, string device)
    {
        if (data == "0")
            PressDown();
        else if (data == "1")
            PressUp();
    }
    
    void ButonTrigger(string data)
    {
        if (data == "0")
            PressDown();
        else if (data == "1")
            PressUp();
    }
}