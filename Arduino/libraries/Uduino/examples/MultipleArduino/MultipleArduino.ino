#include<Uduino.h>
Uduino uduino("myArduinoName");

int variable = 10;

void setup()
{
  Serial.begin(9600);
  uduino.addCommand("myVariable", GetVariable);
}

void GetVariable() {
  Serial.println(variable);
}

void loop()
{
  uduino.readSerial();

  variable ++;
  if (variable == 20) variable = 10;

  delay(100);
}
