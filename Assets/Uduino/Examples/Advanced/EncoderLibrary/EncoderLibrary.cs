using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;
using UnityEngine.UI;

public class EncoderLibrary : MonoBehaviour {

    public Slider slider;
	void Start () {
        UduinoManager.Instance.AlwaysRead("myEncoder", ReadEncoder);
		
	}
	
	void ReadEncoder (string data) {
        slider.value = int.Parse(data);
    }
}
