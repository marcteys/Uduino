/******************************************************************************* 
Uduino - Uduino — Simple and robust Arduino-Unity communication
Uduino - Copyright (C) 2016-2017 Marc Teyssier  <marc.teys@gmail.com>
http://marcteyssier.com

ArduinoSerialCommand - An Arduino library to tokenize and parse commands received over
a serial port. 
Copyright (C) 2011-2013 Steven Cogswell  <steven.cogswell@gmail.com>
http://awtfy.com


This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
***********************************************************************************/
#ifndef Uduino_h
#define Uduino_h

#if defined(ARDUINO) && ARDUINO >= 100
#include "Arduino.h"
#else
#include "WProgram.h"
#endif

// If you want to use UDUINO with the hardware serial port only, and want to disable
// SoftwareSerial support, and thus don't have to use "#include <SoftwareSerial.h>" in your
// sketches, then uncomment this define for UDUINO_HARDWAREONLY, and comment out the 
// corresponding #undef line.  
//
// You don't have to use SoftwareSerial features if this is not defined, you can still only use 
// the Hardware serial port, just that this way lets you get out of having to include 
// the SoftwareSerial.h header. 
//#define UDUINO_HARDWAREONLY 1
#undef UDUINO_HARDWAREONLY

#ifdef UDUINO_HARDWAREONLY
#warning "Warning: Building UDUINO without SoftwareSerial Support"
#endif

#ifndef UDUINO_HARDWAREONLY 
#include <SoftwareSerial.h>  
#endif

#include <string.h>

#define UDUINOBUFFER 32 // Max length of bundle

#define MAXCOMMANDS 8
#define MAXDELIMETER 2

#define UDUINODEBUG 1
//#undef UDUINODEBUG      // Comment for Debug Mode

class Uduino
{
  public:
    static char *_identity; 

    Uduino(char* identity);      // Constructor
    #ifndef UDUINO_HARDWAREONLY
    Uduino(SoftwareSerial &SoftSer,char* identity);  // Constructor for using SoftwareSerial objects
    #endif

    void clearBuffer();   // Sets the command buffer to all '\0' (nulls)
    char *next();         // returns pointer to next token found in command buffer (for getting arguments to commands)
    void readSerial();    // Main entry point.  
    void addCommand(const char *, void(*)());   // Add commands to processing dictionary
    void addDefaultHandler(void (*function)());    // A handler to call when no valid command received. 
    void launchCommand(char * command);
    int charToInt(char* arg); //COnverts char to int

    // Uduino specific commands
    char *getIdentity();
    static void printIdentity();   // Sets the command buffer to all '\0' (nulls)

  private:
    char inChar;          // A character read from the serial stream 
    char buffer[UDUINOBUFFER];   // Buffer of stored characters while waiting for terminator character
    int  bufPos;                        // Current position in the buffer
    char delim[MAXDELIMETER];           // null-terminated list of character to be used as delimeters for tokenizing (default " ")
    char delimBundle[MAXDELIMETER];           // null-terminated list of character to be used as delimeters for tokenizing (default " ")
    char term;                          // Character that signals end of command (default '\r')
    char *token;                        // Returned token from the command buffer as returned by strtok_r
    char *last;                         // State variable used by strtok_r during processing
    typedef struct _callback {
      char command[UDUINOBUFFER];
      void (*function)();
    } UduinoCallback;            // Data structure to hold Command/Handler function key-value pairs
    int numCommand;
    UduinoCallback CommandList[MAXCOMMANDS];   // Actual definition for command/handler array
    void (*defaultHandler)();           // Pointer to the default handler function 
    int usingSoftwareSerial;            // Used as boolean to see if we're using SoftwareSerial object or not
    #ifndef UDUINO_HARDWAREONLY 
    SoftwareSerial *SoftSerial;         // Pointer to a user-created SoftwareSerial object
    #endif
};

#endif //Uduino_h
