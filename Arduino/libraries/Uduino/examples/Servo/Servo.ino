#include<Uduino.h>
Uduino uduino("servoBoard");

#include <Servo.h>
Servo myservo;

void setup()
{
  Serial.begin(9600);
  myservo.attach(9);
   myservo.write(90);
  uduino.addCommand("R", rotate);
}

void rotate() {
  char *arg;
  arg = uduino.next();
  myservo.write(atoi(arg));
}

void loop()
{
  if (Serial.available() > 0)
    uduino.readSerial();

  delay(15);    

}
