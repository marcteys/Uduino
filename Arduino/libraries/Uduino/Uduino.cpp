#if defined(ARDUINO) && ARDUINO >= 100
#include "Arduino.h"
#else
#include "WProgram.h"
#endif

#include "Uduino.h"


#include <string.h>
#ifndef UDUINO_HARDWAREONLY
#include <SoftwareSerial.h>
#endif

char * Uduino::_identity = (char*)"none";
int Uduino::init = 0;


// Constructor makes sure some things are set. 
Uduino::Uduino(const char* identity)
{
  usingSoftwareSerial=0;
  strncpy(delim," ",MAXDELIMETER);  // strtok_r needs a null-terminated string
  strncpy(delimBundle," ,",MAXDELIMETER);  // strtok_r needs a null-terminated string
  term='\r';   // return character, default terminator for commands
  numCommand=0;    // Number of callback handlers installed
  clearBuffer(); 

  Uduino::_identity = (char*)identity;

  int val = 2;
  void* p = &val;

  this->addCommand("identity",Uduino::printIdentity);
  this->addCommand("connected",Uduino::arduinoFound);
  this->addCommand("disconnected",Uduino::arduinoDisconnected);
}

#ifndef UDUINO_HARDWAREONLY
// Constructor to use a SoftwareSerial object
Uduino::Uduino(SoftwareSerial &_SoftSer,char* identity)
{
  usingSoftwareSerial=1; 
  SoftSerial = &_SoftSer;
  strncpy(delim," ",MAXDELIMETER);  // strtok_r needs a null-terminated string
  strncpy(delimBundle," ,",MAXDELIMETER);  // strtok_r needs a null-terminated string
  term='\r';   // return character, default terminator for commands
  numCommand=0;    // Number of callback handlers installed
  clearBuffer(); 
}
#endif


void Uduino::printIdentity() { // TODO : refactor that
  char* additionnal = (char*)"uduinoIdentity ";
  char* full_text;
  full_text = (char*)malloc(strlen(additionnal)+strlen(Uduino::_identity)+1); 
  strcpy(full_text, additionnal ); 
  strcat(full_text, Uduino::_identity);
  Uduino::init = 1;
  init = 1;
  Serial.println (full_text);
}


void Uduino::arduinoDisconnected() { 
  Uduino::init = 0;
  init = 0;
}

void Uduino::arduinoFound() { 
//  init = 1;
   // Serial.println(init);
}

int Uduino::isInit()
{
  return init;
}

//
// Initialize the command buffer being processed to all null characters
//
void Uduino::clearBuffer()
{
  for (int i=0; i<UDUINOBUFFER; i++) 
  {
    buffer[i]='\0';
  }
  bufPos=0; 
}

// Retrieve the next token ("word" or "argument") from the Command buffer.  
// returns a NULL if no more tokens exist.   
char *Uduino::next() 
{
  char *nextToken;
 // nextToken = strtok_r(NULL, delim, &last); 
  nextToken = strtok_r(NULL, delimBundle, &last); 
  return nextToken; 
}


// Launch a command
void Uduino::launchCommand(char * command) {
  char * t;

  t = strtok_r(command,delimBundle,&last);

  if (t == NULL) return; 
    for (int i=0; i<numCommand; i++) {
        #ifdef UDUINODEBUG
        Serial.print("Comparing ["); 
        Serial.print(token); 
        Serial.print("] to [");
        Serial.print(CommandList[i].command);
        Serial.println("]");
        #endif

      if (strncmp(t,CommandList[i].command,UDUINOBUFFER) == 0) 
      {
        #ifdef UDUINODEBUG
        Serial.print("Matched Command: "); 
        Serial.println(t);
        #endif
        // Execute the stored handler function for the command
        (*CommandList[i].function)(); 
        break; 
      }
    }
}

// This checks the Serial stream for characters, and assembles them into a buffer.  
// When the terminator character (default '\r') is seen, it starts parsing the 
// buffer for a prefix command, and calls handlers setup by addCommand() member
void Uduino::readSerial() 
{
  // If we're using the Hardware port, check it.   Otherwise check the user-created SoftwareSerial Port
  #ifdef UDUINO_HARDWAREONLY
  while (Serial.available() > 0) 
  #else
  while ((usingSoftwareSerial==0 && Serial.available() > 0) || (usingSoftwareSerial==1 && SoftSerial->available() > 0) )
  #endif
  {
    int i; 
    boolean matched; 
    if (usingSoftwareSerial==0) {
      // Hardware serial port
      inChar=Serial.read();   // Read single available character, there may be more waiting
    } else {
      #ifndef UDUINO_HARDWAREONLY
      // SoftwareSerial port
      inChar = SoftSerial->read();   // Read single available character, there may be more waiting
      #endif
    }
    #ifdef UDUINODEBUG
    Serial.print(inChar);   // Echo back to serial stream
    #endif
    if (inChar==term) {     // Check for the terminator (default '\r') meaning end of command
      #ifdef UDUINODEBUG
      Serial.print("Received: "); 
      Serial.println(buffer);
        #endif
      bufPos=0;           // Reset to start of buffer
      token = strtok_r(buffer,delimBundle,&last);   // Search for command at start of buffer
      if (token == NULL) return; 
      matched=false; 
      for (i=0; i<numCommand; i++) {
        #ifdef UDUINODEBUG
        Serial.print("Comparing ["); 
        Serial.print(token); 
        Serial.print("] to [");
        Serial.print(CommandList[i].command);
        Serial.println("]");
        #endif
        // Compare the found command against the list of known commands for a match
        if (strncmp(token,CommandList[i].command,UDUINOBUFFER) == 0) 
        {
          #ifdef UDUINODEBUG
          Serial.print("Matched Command: "); 
          Serial.println(token);
          #endif
          // Execute the stored handler function for the command
          (*CommandList[i].function)(); 
          clearBuffer(); 
          matched=true; 
          break; 
        }
      }
      if (matched==false) {
        (*defaultHandler)(); 
        clearBuffer(); 
      }

    }
    if (isprint(inChar))   // Only printable characters into the buffer
    {
      buffer[bufPos++]=inChar;   // Put character into buffer
      buffer[bufPos]='\0';  // Null terminate
      if (bufPos > UDUINOBUFFER-1) bufPos=0; // wrap buffer around if full  
    }
  }
}

// Adds a "command" and a handler function to the list of available commands.  
// This is used for matching a found token in the buffer, and gives the pointer
// to the handler function to deal with it. 
void Uduino::addCommand(const char *command, void (*function)())
{
  if (numCommand < MAXCOMMANDS) {
    #ifdef UDUINODEBUG
    Serial.print(numCommand); 
    Serial.print("-"); 
    Serial.print("Adding command for "); 
    Serial.println(command); 
    #endif
    
    strncpy(CommandList[numCommand].command,command,UDUINOBUFFER); 
    CommandList[numCommand].function = function; 
    numCommand++; 
  } else {
    // In this case, you tried to push more commands into the buffer than it is compiled to hold.  
    // Not much we can do since there is no real visible error assertion, we just ignore adding
    // the command
    #ifdef UDUINODEBUG
    Serial.println("Too many handlers - recompile changing MAXCOMMANDS"); 
    #endif 
  }
}

// This sets up a handler to be called in the event that the receveived command string
// isn't in the list of things with handlers.
void Uduino::addDefaultHandler(void (*function)())
{
  defaultHandler = function;
}


//Converts a char * to int
int Uduino::charToInt(char* arg) {
  String msgString = String(arg);
  return msgString.toInt();
}
