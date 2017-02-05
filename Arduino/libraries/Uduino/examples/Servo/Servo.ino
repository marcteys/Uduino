#include<Uduino.h>
Uduino uduino("advancedBoard");

void setup()
{
  Serial.begin(9600);

  uduino.addCommand("turnLeft", turnLeft);
  uduino.addCommand("off", disable);
}

void turnLeft() {
  char *arg;
  arg = uduino.next();
  myservo.write(atoi(arg));
}


void disable() {
  digitalWrite(13, LOW);
}

void loop()
{
  uduino.readSerial();

  delay(15);

}
