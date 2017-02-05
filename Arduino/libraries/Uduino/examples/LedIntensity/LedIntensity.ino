#include<Uduino.h>
Uduino uduino("ledIntensity");

void setup()
{
  Serial.begin(9600);
  pinMode(11, OUTPUT);
  uduino.addCommand("SetLight", SetLightValue); // The function to be executed when we receive the value from Unity
}

void SetLightValue() {
  char *arg;
  arg = uduino.next(); // The arg char buffer is read and stored
  analogWrite(11, uduino.charToInt(arg)); // The function uduino.charToInt(); converts a char* to a int
}

void loop()
{
  uduino.readSerial();

  delay(50);
}
