# Uduino — Simple and robust Arduino-Unity communication 
-------

**Uduino** is another Open Source plugin for Unity, which allow to communicate between a Arduino board with Unity. 


## Purpose & Target

This project was created after too much frustration on how to properly connected Unity & Arduino. During past past experiences I was facing some problems: difficulties to detect a specific Arduino board when several are connected, freeze and crash when communicating with serial ports, and many more...

All existing projects mixing Arduino and Unity where either not cross-platform,  too complex and complicated (using Firmata to control a LED !? Hell no! ), not compatible with all the libraries or simply not stable enough. What we really need is a simple Arduino file and simple c# functions to read analog pin or write instructions. 

**Uduino** aims to be a *comprehensive* and *easy to setup* solution for your Arduino/Unity projects. It features **simple declaration** both on Unity and Arduino side.


### How simple ?

You give a *unique name* to your arduino board, and declare which variable is readable. For example I want to access `mySensor` on the arduino board named `myArduinoName`.  On Unity, this value is accessible by using `UduinoManager.Instance.Read("myArduinoName", "mySensor");`. Uduino is handling the detection of the boards and the serial connection between the software and the hardware !


## Quick Start

1 . Import [uduino.unitypackage][package] in your project<br>
2 . Add the library `Uduino` from the `Arduino` folder to your local Arduino `libraries` folder.<br>
3 . On your Arduino project, add on the top of your code :<br>
```arduino
#include<Uduino.h>
Uduino uduino("myArduinoName"); // "myArduinoName" is your object's name !
```
4 . On your main Unity script, initialize Uduino:
```csharp
using Uduino; // adding the NameSpace

public class ExampleScript : MonoBehaviour
{
    UduinoManager u; // your Instance is initialized here !
    void Start() ... // continue your code
}
```
5 . Add the reading/writing [methods](#Unity-Methods), and you're good to go !

## Quick code

```csharp
using Uduino;

public class ExampleScript : MonoBehaviour
{
    int ledPin = 11;
    void Start() {
       UduinoManager.Instance.InitPin(ledPin, PinMode.PWM);
    }
    void Update() {
        UduinoManager.Instance.Write(ledPin, Time.time);
    }
}
```

## Setup

#### Unity

Download [uduino.unitypackage][package] and import it in your current project or open this repository with Unity Editor.

#### Arduino

The `Arduino` folder in this repo contains `Uduino`library and examples for Arduino. Merge it with your local Arduino folder.

*Uduino is partially based on [SerialCommand](https://github.com/scogswell/ArduinoSerialCommand) by [Steven Cogswell](https://github.com/scogswell/), released under GPL License. However this library is not needed to run Uduino.*

## Features

<img align="right" src="http://marcteyssier.com/data/uduino/uduino-panel.png">

### Version 1.1 : Debug panel and Log levels !

This new version has two new features, the **custom editor panel** and a **custom Logger**.

The debug panel is automatically attached to `UduinoManager.cs`. It displays all connected boards, with their name, last messages received and sent.
__Uduino now works also in *editor mode*__, you can detect the connected boards and send custom messages to try your arduino code. 

With the custom logger you can chose how much messages you want to be displayed on the console. Feature helpful for debugging, the settings can be changed in the custom inspector.


## Examples 

Examples can be found under `Assets\Uduino\Examples`. Their respective arduino code are on the library folders (`Arduino IDE\Examples\Uduino`). It might be the easiest solution to understand Uduino !

### Usage

Here is the steps to have Uduino working. 

#### Arduino 
1. Add `#include<Uduino.h>`as dependency
2. Instantiate Uduino and define the name of your board : `Uduino uduino("sensorArduino");` 
3. Add new commands in Setup()
4. Update Uduino in the loop. `if (Serial.available() > 0) uduino.readSerial();`

#### Unity
1. Add he namespace `using Uduino`at the to of your script
2. Declare `UduinoManager u;` as new variable (*note: The Instance is created the First time you call Uduino.Instance . It's recommended to declare it as variable to find you connected boards on start.*)

### Simple Read (Arduino to Unity)

Read the value of a sensor connected to the pin *A0* of the board. 

#### ReadSensor.ino (Arduino)
```arduino
#include<Uduino.h>
Uduino uduino("myArduinoName"); // Declare and name your object

void setup()
{
  Serial.begin(9600);
  uduino.addCommand("mySensor", GetVariable); // Link your sensor reading (called "mySensor") to a function
}

void GetVariable() {
  Serial.println(analogRead(A0));
}
void loop()
{
   if (Serial.available() > 0) // verify if the serial is available...
    uduino.readSerial();       // ...then process Uduino. /!\ This part is mandatory
}
```

#### ReadSensor.cs (Unity)
```csharp
using UnityEngine;
using System.Collections;
using Uduino;

public class ReadSensor : MonoBehaviour {

   UduinoManager u;

  void Awake ()
  {
        UduinoManager.Instance.OnValueReceived += OnValueReceived; //Create the Delegate
  }

  void Update ()
  {
        UduinoManager.Instance.Read("myArduinoName", "mySensor"); // Read every frame the value of the "mySensor" function on our board. 
  }

    void OnValueReceived(string data, string device)
    {
        Debug.Log(int.Parse(data)); // Use the data as you want !
    }
}
```
Note : To retrieve the data on Unity without creating any freeze of your application, you need to create a new [delegate](#Why-using-delegates) function.


### Simple Write (Unity to Arduino)

Send a PWM value from unity to arduino, to light set the light intensity of a LED. 

#### LedIntensity.ino (Arduino)
```arduino
#include<Uduino.h>
Uduino uduino("ledIntensity");

void setup()
{
  Serial.begin(9600);
  pinMode(11,OUTPUT);
  uduino.addCommand("SetLight", SetLightValue); // The function to be executed when we receive the value from Unity
}

void SetLightValue() {
  char *arg; 
  arg = uduino.next(); // The arg char buffer is read and stored 
  analogWrite(11,uduino.charToInt(arg)); // The function uduino.charToInt(); converts a char* to a int
}

void loop()
{
   if (Serial.available() > 0)
    uduino.readSerial();
    
  delay(50);    
}
```

#### SendLedIntensity.cs (Unity)
```csharp
using UnityEngine;
using System.Collections;
using Uduino;

public class SendLedIntensity : MonoBehaviour {
    [Range(0, 255)]
    public int intensity = 0;

  void Update ()
  {
        UduinoManager.Instance.Write("ledIntensity", "SetLight", intensity);
  }
}
```


### Multi arduino boards

Uduino works with multiple boards connected to the computer. Be sure to set a different board name for each Arduino board.

#### MultipleArduino.cs (Unity)

```csharp
using UnityEngine;
using System.Collections;
using Uduino;

public class MultipleArduino: MonoBehaviour
{
    UduinoManager u;
    int sensorOne = 0;
    int sensorTwo = 0;

    void Start()
    {
        UduinoManager.Instance.OnValueReceived += OnValuesReceived;
    }

  void Update ()
  {
        UduinoManager.Instance.Read("myArduinoBoard", "mySensor");
        UduinoManager.Instance.Read("myOtherArduino", "myOtherSensor");
    }

    void OnValuesReceived(string data, string device)
    {
        if (device == "myArduinoBoard") sensorOne = int.Parse(data);
        else if (device == "myOtherArduino") sensorTwo = int.Parse(data);
    }
}
``` 


### Working with external libraries

Unlike other Unity/Arduino plugins, Uduino is compatible with any other Arduino libraries ! Example below with Arduino's `Servo` library. 


####Servo.ino (Arduino)
```arduino
#include<Uduino.h>
Uduino uduino("servoBoard");

#include <Servo.h>
Servo myservo;

void setup()
{
  Serial.begin(9600);
  myservo.attach(9);
  uduino.addCommand("R", rotate);
}

void rotate() {
  char *arg;
  arg = uduino.next();
  myservo.write(uduino.charToInt(arg));
}

void loop()
{
  uduino.readSerial();
}
```

#### Servo.cs (Unity)
```csharp 
using UnityEngine;
using System.Collections;
using Uduino;

public class Servo : MonoBehaviour {

    [Range(0, 180)]
    public int servoAngle = 0;
    private int prevServoAngle = 0;

    void Update()
    {
        OptimizedWrite();
    }

    void OptimizedWrite()
    {
        if (servoAngle != prevServoAngle) // Condition to not send data each frame 
        {
            UduinoManager.Instance.Write("servoBoard", "R", servoAngle);
            prevServoAngle = servoAngle;
        }
    }
}
```


### Reading in one line

Rather than declaring a delegate function (mainly for prototyping), the `c#` language allows to declare on-the-fly actions. It can reduce the reading code to a simple line :

#### OneLineRead.cs (Unity)
```csharp 
  // ...
  public int sensorOne = 0;
   
  void Update ()
  {
        UduinoManager.Instance.Read("myArduinoBoard", "mySensor", action:((string data) => sensorOne = int.Parse(data)));
  }
}
```


## Why using delegates 

A function trying to read the of a Serial port pauses its execution until the reading is complete. If the reading never happens... the software crash !  Because Unity is mono-thread, opening a new thread to do some other calculations might not be safe. However, Uduino has a thread safe function to read and write on the serial port. The values retrieved are transmitted from the side thread to the main, and we use *delegates* to do that. You can then use safely `UduinoManager.Instance.Read(..)` in your script. 


## Arduino Methods

| Name          | Description         |
|---------------|---------------------|
|`readSerial()`| Process Uduino every clock turn. Required in the `loop()` function. |
| `addCommand(string, void)` | Attach a command to a specific function. The function will be triggerd when the event is called by Unity |
| `charToInt(args)` | Convet chars* to int |
|`clearBuffer()`| Clear Serial buffer|

## Unity Methods

### Read(target, variable = null, timeout = 100, action = null)

Send a read command to a specific Arduino board.
A Read() command will be returned on the  OnValueReceived() delegate function.
        
| Name          | Description         |
|---------------|---------------------|
|`target`| *System.String*<br> Target device name. Not defined means read everything |
|`variable`| *System.String*<br>Variable watched, if defined|
|`timeout`| *System.Integer*<br> Read Timeout, if defined |
|`action`|System.Action<string> Action callback |


### Write(target, message)

Write a command on an Arduino

| Name          | Description         |
|---------------|---------------------|
|`target`| *System.String*<br> Target device name. Not defined means read everything |
|`message`| *System.String*<br>Message to send to the Arduino board|

### Write(target, message, value)

Write a command on an Arduino with a specific value 

| Name          | Description         |
|---------------|---------------------|
|`target`| *System.String*<br> Target device name. Not defined means read everything |
|`message`| *System.String*<br>Message to send to the Arduino board|
|`value`|*System.Integer*<br>Value associated with the message|

### Write(target, message[], value[])
Send multiple messages with different values.

| Name          | Description         |
|---------------|---------------------|
|`target`| *System.String*<br> Target device name. Not defined means read everything |
|`message`| *System.String[]*<br>Messages to send to the Arduino board|
|`values`|*System.Integer[]*<br>List of values to be sent. Value #is associated with message #|


### GetPortState()

Debug.Log() the status of all connected Serial Ports devices. 


### CloseAllPorts();

Close all opened serial ports.

## Unity custom Logger Methods

To simplify the debug of serial communication, a custom Logger is available. 

### Log.Info(message), Log.Warning(message), Log.Error(message)

Custom logger with priorities. The level can be set on the inspector or at runtime with `Log.SetPriority(level)`

| Name          | Description         |
|---------------|---------------------|
|`message`| *System.String*<br>Message to debug|


### Log.SetPriority(level)

Define the logger priority. 

| Name          | Description         |
|---------------|---------------------|
|`level`| *Uduino.Level {INFO, WARNING, ERROR, NONE} *<br>Level of prioriy.|



## Uduino Utils Methods


## Update Uduino

1. Delete `Assets\Uduino` in your Unity project and replace it with the [last version][package].
2. Replace`libraries\Uduino`folder in your local `Arduino`folder by the new one on this repo. 



## FAQ

#### I experience the error : *The type or namespace name `Ports' does not exist in the namespace `System.IO'*
Be sure to set the API Compatibility level to **.NET 2.0** (and not .NEt 2.0 Subset). This setting is located under `Edit\Project settings\Player`. 

#### Nothing is happening, what is wrong ?

Be sure that Uduino custom [debug levels](#), is set not set to "NONE". If so, Uduino will hide all the messages in the console, including some important messages (*"Arduino found"*, etc ).

#### Should I attach UduinoManager.cs to a game object

It's not mandatory. If the script is not attached to any game object of your scene, the first call of `UduinoManager.Instance` will create a new game object. 

#### Something else is wrong

You can explain your problem on a new [issue](https://github.com/marcteys/Uduino/issues).

## Contribution

Uduino is an [**OPEN Open Source Project**](http://openopensource.org/). This means that:

> Individuals making significant and valuable contributions are given commit-access to the project to contribute as they see fit. This project is more like an open wiki than a standard guarded open source project.

This is an experiment and feedback is welcome. I'll be very happy to have your contribution on this library. If you create something interesting with uduino, add it to the examples folder, submit a pull-request, and we'll take a look.


----------


## Todo
* Unity: Arduino: Create a "simple" Sketch
* Unity: Write(string target, string[] message, int[] value) could take a 2D array as parameter ?
* Arduino: Introduce a custom delay, to avoid blocking situations ?
* Documentation: Explain Arduino uduino_hardwareonly mode 


----
Current Version: 1.1 <br>
Last stable update: Nov 30 2016

[package]: https://github.com/marcteys/Uduino/raw/master/uduino_1.1.unitypackage
