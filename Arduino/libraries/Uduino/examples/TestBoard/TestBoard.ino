#include<Uduino.h>
Uduino uduino("testBoard"); // Declare and name your object

void setup()
{
  Serial.begin(9600);
  uduino.addCommand("setMode", SetMode);
  uduino.addCommand("writePin", WritePin);
  uduino.addCommand("readPin", ReadPin);
  uduino.addCommand("test", Test);
}

void Test() { 
  char *arg; 
  arg = uduino.next();
  if (arg != NULL)
  {
    Serial.print("One "); 
    Serial.print(arg); 
  } 

  arg = uduino.next();
  if (arg != NULL)
  {
    Serial.print("Two "); 
    Serial.println(arg); 
  } 
}

void SetMode() { 
  //TODO : add mode "servo"
  Serial.println(analogRead(A0));
}

void WritePin() {
  Serial.println(analogRead(A0));
}

void ReadPin() {
  Serial.println(analogRead(A0));
}

void loop()
{
   if (Serial.available() > 0)
    uduino.readSerial(); 

   delay(50);
}
