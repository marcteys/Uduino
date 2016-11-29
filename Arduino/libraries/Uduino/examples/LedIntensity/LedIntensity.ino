#include<Uduino.h>
Uduino uduino("ledIntensity");

int ledPin = 11;

void setup()
{
  Serial.begin(9600);
  pinMode(13,OUTPUT);
  uduino.addCommand("LIGHT", light);
}

void light() {
  char *arg;
  arg = uduino.next();
  analogWrite(11,uduino.charToInt(arg));
}

void loop()
{
   if (Serial.available() > 0)
    uduino.readSerial();
    
  delay(50);    
}

