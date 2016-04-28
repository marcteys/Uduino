#include<Uduino.h>
Uduino uduino("myArduino");

void setup()
{
  Serial.begin(9600);
  while (!Serial);

  uduino.addCommand("I", identityHandler);
}

void identityHandler () {
  Serial.println(uduino.getIdentity());
}


void loop()
{
   if (Serial.available() > 0)
    uduino.readSerial();
    
  delay(100);
}

