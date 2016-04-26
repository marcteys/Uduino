#include <SerialCommand.h>
SerialCommand sCmd;

//Motors
const int limitleftpin  = 3;
const int limitrightpin = 2;
const int motorpin1     = 5;
const int motorpin2     = 4;
const int motorsenable  = 6;

//DC motors
const unsigned int rackwidth = 644; //mm
unsigned int motorspeed      =  77; // mm/s

//Servo motor
const int servopin = 7;
int servodelay = 20;

//buffer stuff

unsigned int value = 0;

// TODO / REMOVE THIS 
#define POSITIVE 0
#define NEGATIVE 1
int sign = POSITIVE;
int a;
unsigned int pos;

void setup()
{
  //Motors
  init_motors();

  //Communication
  Serial.begin(9600);
  while (!Serial);

  //  action commad
  sCmd.addCommand("T", translateHandler);
  sCmd.addCommand("M", moveHandler);
  sCmd.addCommand("R", rotateHandler);

  //  return command
  sCmd.addCommand("I", identityHandler);
  sCmd.addCommand("Z", shakeHandler);
  sCmd.addCommand("C", calibrate);
  sCmd.addCommand("A", getAngle);
  sCmd.addCommand("S", getMotorSpeed);
  sCmd.addCommand("P", getPosition);
  sCmd.addCommand("W", getWidth);
  sCmd.addCommand("D", getServoDelay);

  sCmd.addCommand("STOP", stopHandler);

}

void loop()
{
  if (Serial.available() > 0)
    sCmd.readSerial();

  delay(100);
}

void identityHandler () {
  Serial.println("LivingScreen");
}

void translateHandler() {
  char *arg;
  arg = sCmd.next();
    if (arg != NULL) {
      translate(charToInt(arg),0);
      /*
      Serial.print('t');
      Serial.print('_');
      Serial.println(charToInt(arg));*/
    }
}

void shakeHandler() {
  Serial.println("S");
  char *arg;
  arg = sCmd.next();
    if (arg != NULL) {
      translate(charToInt(arg),5);
    }
}

void moveHandler() {
  char *arg;
  arg = sCmd.next();
  if (arg != NULL) {
    int val = charToInt(arg);
    if (val != pos)
      translate(val - pos,0);
  }
}

void rotateHandler() {
  char *arg;
  arg = sCmd.next();
  if (arg != NULL) rotate(charToInt(arg));
}

void getAngle() {
 int an = angle();
  Serial.println(a);
}
void getMotorSpeed() {
  Serial.println(motorspeed);
}
void getPosition() {
  Serial.println(pos);
}
void getWidth() {
  Serial.println(rackwidth);
}
void getServoDelay() {
  Serial.println(servodelay);
}

int charToInt(char* arg) {
  String msgString = String(arg);
  return msgString.toInt();
}

