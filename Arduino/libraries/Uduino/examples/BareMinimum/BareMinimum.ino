#include<Uduino.h>
Uduino uduino("myArduinoName"); // Declare and name your object

void setup()
{
  Serial.begin(9600);
  uduino.addCommand("writeFunction", writeFunction);
  uduino.addCommand("readFunction", readFunction);
}

void writeFunction() {
  Serial.println(analogRead(A0));
}

void readFunction() {
  char *arg;
  arg = uduino.next();
  Serial.print("We just read: ");
  Serial.println((atoi(arg));
}

void loop()
{
  uduino.readSerial();  
  delay(15);
}
