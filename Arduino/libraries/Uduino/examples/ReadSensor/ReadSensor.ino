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
  uduino.readSerial();       //!\ This part is mandatory

  delay(50);
}
