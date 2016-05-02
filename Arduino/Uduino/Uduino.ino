#include<Uduino.h>
Uduino uduino("myArduino");

int variable = 10;

bool test = false;

void setup()
{
  pinMode(13,OUTPUT);
  Serial.begin(9600);
  while (!Serial);
  uduino.addCommand("PING", pong);
  uduino.addCommand("LIGHT", light);
  uduino.addCommand("SENSOR", GetVariable);
}

void GetVariable() {
  Serial.println(variable);
}

void lol() {
  Serial.println("test");
}
void identityHandler () {
  Serial.println("uduinoIdentity myArduino");
}
void pong () {
  Serial.println("pong");
}

void light() {
    char *arg;
      arg = uduino.next();
   String msgString = String(arg);
  if(msgString.toInt()==0) digitalWrite(13,LOW);
  else digitalWrite(13,HIGH);
  Serial.println("Change");
}


void loop()
{
   if (Serial.available() > 0)
    uduino.readSerial();

  variable = random(10, 200);

  delay(50);    
}

//TODO : en faire une méthode arduino
int charToInt(char* arg) {
  String msgString = String(arg);
  return msgString.toInt();
}

