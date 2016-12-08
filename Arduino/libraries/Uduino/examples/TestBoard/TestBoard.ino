// Uduino Test board

#include<Uduino.h>
Uduino uduino("testBoard"); // Declare and name your object


// Servo
#include <Servo.h>
#define MAXSERVOS 12

typedef struct _servoWrapper {
  int pin = -1;
  Servo servo;
  int pos;
  int getPin() {
    if (servo.attached()) return pin;
    else return false;
  }
  bool attached() {
    return servo.attached();
  }
  void disable() {
    pin = -1;
    if (attached()) servo.detach();
  }
  void enable() {
    servo.attach(pin);
  }
  void update() {
    if (attached()) servo.write(pos);
  }
} ServoWrapper;

static ServoWrapper Servos[MAXSERVOS];



void setup()
{
  Serial.begin(9600);
  uduino.addCommand("setMode", SetMode);
  uduino.addCommand("writePin", WritePin);
  uduino.addCommand("readPin", ReadPin);
}


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
    if(type != 4) getServoAttachedTo(pinToMap)->disable();
    switch (type) {
      case 0: // Output
        pinMode(pinToMap, OUTPUT);
        break;
      case 1: // PWM
        pinMode(pinToMap, OUTPUT);
        break;
      case 2: // Analog
        pinMode(pinToMap, INPUT);
        break;
      case 3: // Input_Pullup
        pinMode(pinToMap, INPUT_PULLUP);
        break;
      case 4: // Servo
       startServo(getServoAttachedTo(-1),pinToMap);
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

  int value;
  arg = uduino.next();
  if (arg != NULL)
  {
    value = atoi(arg);
    if(getServoAttachedTo(pinToMap)->pin != -1) { //it's a servo
      getServoAttachedTo(pinToMap)->pos = value;
    } else {
      analogWrite(pinToMap, value);
    }
  }

}

void ReadPin() {
  Serial.println(analogRead(A0));
}

void loop()
{
  if (Serial.available() > 0)
    uduino.readSerial();

  for (int i = 0; i < MAXSERVOS; i++) {
    updateServo(&Servos[i]);
  }
  delay(15);
}


ServoWrapper *getServoAttachedTo(int pin) {
  for (int i = 0; i < MAXSERVOS; i++) {
    if (Servos[i].getPin() == pin) return &Servos[i];
  }
  return &Servos[0];
}

void startServo(struct _servoWrapper* servo, int pin) {
  servo->pin = pin;
  servo->enable();
}

void setServoPosition(struct _servoWrapper* servo, int pos) {
  servo->pos = pos;
}
void updateServo(struct _servoWrapper* servo) {
  servo->update();
}

