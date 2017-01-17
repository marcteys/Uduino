// Uduino Test board

#include<Uduino.h>
Uduino uduino("uduinoBoard"); // Declare and name your object

// Servo
#include <Servo.h>
#define MAXSERVOS 12

#define UDUINODEBUG 1

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
  uduino.addCommand("s", SetMode);
  uduino.addCommand("d", WritePinDigital);
  uduino.addCommand("a", WritePinAnalog);
  uduino.addCommand("r", ReadPin);
  uduino.addCommand("b", ReadBundle);
}



void ReadBundle() {
  char *arg;
  char *number;
  number = uduino.next();
  int len ;
  if (number != NULL)
    len = atoi(number);

  for (int i = 0; i < len; i++) {
    uduino.launchCommand(arg);
  }
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
    PinSetMode(pinToMap, type);
  }
}

void PinSetMode(int pin, int type) {
  if (type != 4) getServoAttachedTo(pin)->disable();
  switch (type) {
    case 0: // Output
      pinMode(pin, OUTPUT);
      break;
    case 1: // PWM
      pinMode(pin, OUTPUT);
      break;
    case 2: // Analog
      pinMode(pin, INPUT);
      break;
    case 3: // Input_Pullup
      pinMode(pin, INPUT_PULLUP);
      break;
    case 4: // Servo
      startServo(getServoAttachedTo(-1), pin);
      break;
  }
}

void WritePinAnalog() {
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
     if (getServoAttachedTo(pinToMap)->pin != -1) { //it's a servo
      getServoAttachedTo(pinToMap)->pos = value;
    } else {
      analogWrite(pinToMap, value);
    }
  }
}


void WritePinDigital() {
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
      if (getServoAttachedTo(pinToMap)->pin == -1) //it's not a servo
      digitalWrite(pinToMap, value);
  }
  
}

void ReadPin() {
  int pinToRead;
  char *arg;
  arg = uduino.next();
  if (arg != NULL)
  {
    pinToRead = atoi(arg);
  }
  Serial.print(pinToRead);
  Serial.print(" ");
  Serial.println(analogRead(pinToRead));
}

void loop()
{
  if (Serial.available() > 0)
    uduino.readSerial();

  for (int i = 0; i < MAXSERVOS; i++) {
    updateServo(&Servos[i]);
  }
  delayMicroseconds(50);
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

