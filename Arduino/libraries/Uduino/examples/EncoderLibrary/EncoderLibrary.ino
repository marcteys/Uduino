#include<Uduino.h>
Uduino uduino("myEncoder"); // Declare and name your object

#include <Encoder.h>
Encoder myEnc(5, 6);
long oldPosition  = -999;

void setup()
{
  Serial.begin(9600);
}

void loop() {
  uduino.readSerial();  
  long newPosition = myEnc.read();
  if (newPosition != oldPosition) {
    oldPosition = newPosition;
    Serial.println(newPosition);
  }
}
