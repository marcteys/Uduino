# Uduino — Simple and robust Arduino-Unity communication 
-------

**Uduino** is another Open Source plugin for Unity, which allow to communicate between a Arduino board with Unity. 

## Where to find it

While waiting for a sustainable open-source alternative, I removed Uduino source code from Github. 
You can now get Uduino on [Unity Asset Store](https://www.assetstore.unity3d.com/#!/content/78402) and get more informations on the [official website](http://marcteyssier.com/uduino/). 


## Purpose & Target

This project was created after too much frustration on how to properly connected Unity & Arduino. During past past experiences I was facing some problems: difficulties to detect a specific Arduino board when several are connected, freeze and crash when communicating with serial ports, and many more...

All existing projects mixing Arduino and Unity where either not cross-platform,  too complex and complicated (using Firmata to control a LED !? Hell no! ), not compatible with all the libraries or simply not stable enough. What we really need is a simple Arduino file and simple c# functions to read analog pin or write instructions. 

**Uduino** aims to be a *comprehensive* and *easy to setup* solution for your Arduino/Unity projects. It features **simple declaration** both on Unity and Arduino side.

