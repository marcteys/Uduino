#include<Uduino.h>
Uduino uduino("testBoard"); // Declare and name your object

void setup()
{
  Serial.begin(9600);
  uduino.addCommand("setMode", SetMode);
  uduino.addCommand("writePin", WritePin);
  uduino.addCommand("readPin", ReadPin);
}
  //TODO : add mode "servo"



void SetMode() {
  int pinToMap;
  char *arg;
  arg = uduino.next();
  if (arg != NULL)
  {
    pinToMap = atoi(arg);
  }

  int type;
  arg = uduino.next();
  if (arg != NULL)
  {
    type = atoi(arg);

    switch (type) {
      case 0: // Output
        pinMode(pinToMap, OUTPUT);
        break;
      case 1: // Input
        pinMode(pinToMap, INPUT);
        break;
      case 2: // Analog
        pinMode(pinToMap, INPUT);
        break;
      case 3:
        break;

    }
  }

  //uduino.charToInt(arg)

}

void WritePin() {
    int pinToMap;
  char *arg;
  arg = uduino.next();
  if (arg != NULL)
  {
    pinToMap = atoi(arg);
  }

  int type;
  arg = uduino.next();
  if (arg != NULL)
  {
    type = atoi(arg);
    analogWrite(pinToMap, type);
  }

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
