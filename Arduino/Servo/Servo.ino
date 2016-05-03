#include<Uduino.h>
Uduino uduino("servo");

#include <Servo.h>
Servo myservo;


void setup()
{
 Serial.begin(9600);

  myservo.attach(9);
  uduino.addCommand("R", rotate);
}

void rotate() {
  char *arg;
  arg = uduino.next();
  myservo.write(uduino.charToInt(arg));
}


void loop()
{
  uduino.readSerial();
}

