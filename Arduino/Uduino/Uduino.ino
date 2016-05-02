#include<Uduino.h>
//Uduino uduino("myArduino");

int variable = 10;
#include <SerialCommand.h>

SerialCommand SCmd;   // The demo SerialCommand object

void setup()
{
      pinMode(13,OUTPUT);

  Serial.begin(9600);
  while (!Serial);
/*
  uduino.addVariable("variable", variable);
 // uduino.addCommand("I", identityHandler);
  uduino.addCommand("PING", pong);
  uduino.addCommand("Z", pong);
  uduino.addCommand("LIGHT", light);*/
  SCmd.addCommand("PING", pong);
  SCmd.addCommand("LIGHT", light);
    SCmd.addCommand("IDENTITY", identityHandler);

}
void identityHandler () {
  Serial.println("uduinoIdentity myArduino");
}
void pong () {

  Serial.println("pong");
}

void light() {
    char *arg;
      arg = SCmd.next();
   String msgString = String(arg);
  if(msgString.toInt()==0) digitalWrite(13,LOW);
  else digitalWrite(13,HIGH);
  Serial.println("Change");
}


void loop()
{
   if (Serial.available() > 0)
    SCmd.readSerial();
    
  delay(10);    
}

//TODO : en faire une méthode arduino
int charToInt(char* arg) {
  String msgString = String(arg);
  return msgString.toInt();
}

