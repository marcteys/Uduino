#include<Uduino.h>
Uduino uduino("sensorArduino");

int variable = 10;

void setup()
{
  pinMode(13,OUTPUT);
  Serial.begin(9600);

  uduino.addCommand("SENSOR", GetVariable);
}

void GetVariable() {
  Serial.println(variable);
}

void loop()
{
   if (Serial.available() > 0)
    uduino.readSerial();

  variable = random(10, 200);

  delay(10);    
}
