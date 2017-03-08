#include<Uduino.h>
Uduino uduino("uduinoButton");

const int buttonPin = 12;
int buttonState = 0;
int prevButtonState = 0;

void setup()
{
  Serial.begin(9600);
  pinMode(buttonPin, INPUT_PULLUP);
}

void loop()
{
  uduino.readSerial();

  buttonState = digitalRead(buttonPin);

  if (buttonState != prevButtonState) {
    Serial.println(buttonState);
    prevButtonState = buttonState;
  }
  delay(15);
  
}
