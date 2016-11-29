Uduino — Simple and robust Arduino-Unity communication
-------

**Uduino** is another Open Source plugin for Unity, which allow to communicate between a Arduino board with Unity. 

## Purpose & Target

This project was created after too much frustration on how to properly connected Unity & Arduino. During past past experiences I was facing some problems: difficulties to detect a specific Arduino board when several are connected, small freeze when reading analog pins, no direct feedbacks when writing arduino, and most important : **not human-readable**.

All existing projects mixing Arduino and Unity where either not cross-platform,  too complex and complicated (using Firmata to control a LED !? Hell no! ) or simply not stable enough. What we really need for our project is a simple Arduino file and some clear c# functions to read or write instructions. 

**Uduino** aims to be a *comprehensive* and *easy to setup* solution for your Arduino/Unity projects. It features **simple declaration** both on Unity and Arduino side.


### How simple ?

You give a *unique name* to your arduino card, and declare which variable is readable. For example I want to access *"mySensor"* on the card named *"myArduinoCard"*  Then on Unity, you can access to this value by using `UduinoManager.Instance.Read("myArduinoCard", "mySensor");`. Uduino is handling the Serial connection between the software and the hardware !



## Quick Start

1. Import [uduino.unitypackage]() in your project
2. Add  the libraries `Uduino` and `SerialCommand`([link](https://github.com/scogswell/ArduinoSerialCommand)) to your Arduino `libraries` folder.
3. On your Arduino project, add on the top of your code :
````arduino
#include<Uduino.h>
Uduino uduino("myArduinoName"); // "myArduinoName" is your object's name !
````
4. On your main Unity script, initialize Uduino:
```csharp
using Uduino; // adding the NameSpace

public class ExampleScript : MonoBehaviour
{
    UduinoManager u; // your Instance is initialized here !
    void Start() ... // continue your code
}
```
5. Add the [methods](#methods), and you're good to go !

## Setup

Download or clone the current repository. The repo is a Unity3D project and can be openend with Unity Editor. The `Arduino` folder contains the library and examples for Arduino, it can be merged with your Arduino user folder. 

#### Arduino

**Uduino** uses [SerialCommand](https://github.com/scogswell/ArduinoSerialCommand) library, which can be downloaded on [GitHub](https://github.com/scogswell/ArduinoSerialCommand). You can download this library and add it to your Arduino's `libraries` folder. 

#### Unity

Download the Unity [Uduino Package]() and import it in your current project or open this project with Unity.



## Examples 

Examples can be found under `Assets\Uduino\Examples`. Their respective arduino code are on the library folders (`Arduino IDE\Examples\Uduino`). 

## Usage


### Read Analog value
On the Arduino side, you have to define a name to your card. 
The name of the arduino will be used

```arduino
#include<Uduino.h> //Add the reference to the library
Uduino uduino("myArduinoName"); // Rename your object

void setup()
{
  Serial.begin(9600); // Start Serial
}

void loop()
{
   if (Serial.available() > 0)
    uduino.readSerial();
  Serial.print(analogRead(A0)); // Write the 
  delay(100);
}
```


```csharp
using UnityEngine;
using System.Collections;
using Uduino;

public class SimpleUduino : MonoBehaviour {

    [Range(0, 255)]
    public int intensity = 0;

	void Update ()
	{
        UduinoManager.Instance.Write(""myArduinoName");
	}
}

```


## Arduino Methods

| Name          | Description         |
|---------------|---------------------|
|`readSerial()`| Process Uduino every clock turn. Required in the `loop()` function. |
| `addCommand(string, void)` | Attach a command to a specific function. The function will be triggerd when the event is called by Unity |
|`clearBuffer()`| Clear Serial buffer|

## Unity Mehods

### Read(string target, string variable = null, int timeout = 100, System.Action<string> action = null)

Send a read command to a specific Arduino board.
A Read() command will be returned on the  OnValueReceived() delegate function
        
| Name          | Description         |
|---------------|---------------------|
|`target`| *System.String*<br> Target device name. Not defined means read everything |
|`variable`| *System.String*<br>Variable watched, if defined|
|`timeout`| *System.Integer*<br> Read Timeout, if defined |
|`timeout`|System.Action<string> Action callback |




### Write(string target, string message)

Write a command on an Arduino

| Name          | Description         |
|---------------|---------------------|
|`target`| *System.String*<br> Target device name. Not defined means read everything |
|`message`| *System.String*<br>Message to send to the Arduino board|

### Write(string target, string message, int value)

Write a command on an Arduino with a specific value 

| Name          | Description         |
|---------------|---------------------|
|`target`| *System.String*<br> Target device name. Not defined means read everything |
|`message`| *System.String*<br>Message to send to the Arduino board|
|`value`|*System.Integer*<br>Value associated with the message|

### Write(string target, string[] message, int[] value)

| Name          | Description         |
|---------------|---------------------|
|`target`| *System.String*<br> Target device name. Not defined means read everything |
|`message`| *System.String[]*<br>Messages to send to the Arduino board|
|`values`|*System.Integer[]*<br>List of values to be sent. Value #is associated with message #|


####Returns


## Contribution

Uduino is an [**OPEN Open Source Project**](http://openopensource.org/). This means that:

> Individuals making significant and valuable contributions are given commit-access to the project to contribute as they see fit. This project is more like an open wiki than a standard guarded open source project.

This is an experiment and feedback is welcome. I'll be very happy to have your contribution on this library. If you create something interesting with uduino, add it to the examples folder, submit a pull-request, and we'll take a look.


## Todo
* Create a "simple" Sketch
* Create a global #SerialDebug value on Unity.
* Introduce a custom delay, to avoid blocking situations ?
