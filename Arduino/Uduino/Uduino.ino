#include<Uduino.h>
Uduino uduino("myArduino");

int variable = 10;
#include <SerialCommand.h>

SerialCommand SCmd;   // The demo SerialCommand object

void setup()
{
      pinMode(13,OUTPUT);

  Serial.begin(9600);
  while (!Serial);

  uduino.addVariable("variable", variable);
 // uduino.addCommand("I", identityHandler);
  uduino.addCommand("PING", pong);
  uduino.addCommand("Z", pong);
  uduino.addCommand("LIGHT", light);
  SCmd.addCommand("LIGHT", light);
}

void pong () {
  char *arg;
  arg = uduino.next();
  Serial.println("caca");
}

void light() {
    char *arg;
   String msgString = String(arg);
  if(msgString.toInt()==0) digitalWrite(13,LOW);
  else digitalWrite(13,HIGH);
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
  delay(10);    
}



//TODO : en faire une méthode arduino
int charToInt(char* arg) {
  String msgString = String(arg);
  return msgString.toInt();
}

