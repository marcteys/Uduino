#include<Uduino.h>
Uduino uduino("myArduino");

int variable;

void setup()
{
      pinMode(13,OUTPUT);

  Serial.begin(9600);
  while (!Serial);

  uduino.addVariable("variable", 10);
  uduino.addCommand("I", identityHandler);
  uduino.addCommand("PING", pong);
}

void pong () {
  char *arg;
  arg = uduino.next();
  Serial.print("lol");
}

void identityHandler () {
  Serial.flush();
  Serial.println(uduino.getIdentity());
  delay(500);
}

void readSensor () {
  variable = analogRead(A0);
}

void loop()
{
   if (Serial.available() > 0)
    uduino.readSerial();
    
  readSensor ();
  delay(400);
    digitalWrite(13,LOW);

}



//TODO : en faire une méthode arduino
int charToInt(char* arg) {
  String msgString = String(arg);
  return msgString.toInt();
}

