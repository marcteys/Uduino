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

## Usage



#### Arduino
On the Arduino side, you have to define 
The name of the arduino will be used

```arduino
#include<Uduino.h>
Uduino uduino("myArduinoName");

void setup()
{
  Serial.begin(9600);
  uduino.addCommand("FUNCTION", ReadPin);
}

void ReadPin() {
  Serial.println(analogRead(A0));
}

void loop()
{
   if (Serial.available() > 0)
    uduino.readSerial();

  delay(10);    
}
```


```csharp

```


##Examples 


## Methods

### Read data from serial


### Send a command to arduino

Without parameter
With parameter




## FAQ
- 

## Contribution

Uduino is an [**OPEN Open Source Project**](http://openopensource.org/). This means that:

> Individuals making significant and valuable contributions are given commit-access to the project to contribute as they see fit. This project is more like an open wiki than a standard guarded open source project.

This is an experiment and feedback is welcome. I'll be very happy to have your contribution on this library. If you create something interesting with uduino, add it to the examples folder, submit a pull-request, and we'll take a look.


## Todo
* Create a global #SerialDebug value on Unity.   
