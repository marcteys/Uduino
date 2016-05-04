#include<Uduino.h>
Uduino uduino("leo");

int variable = 00;

void setup()
{
  Serial.begin(9600);
  uduino.addCommand("read", GetVariable);
}

void GetVariable() {
  Serial.println(variable);
}

void loop()
{
   if (Serial.available() > 0)
    uduino.readSerial();

  variable ++;
  if(variable == 10) variable = 0;
  
  delay(100);    
}
