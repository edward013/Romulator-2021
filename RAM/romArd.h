          #ifndef ROMULATOR_ARDUINO
          #define ROMULATOR_ARDUINO
          //------------------------------------------------------------------------------
          //  File:     romArd.h
          //  By:       Team Romulator:
          //          David Landry, chief software engineer
          //          Biniyam Yemane-Berhane, Nema Karimi, Thien Nguyen
          //  For:      CE / BEE Capstone; University of Washington, Bothell
          //          Developed for the School of STEM
          //  Advisor:    Dr. Arnold S. Berger
          //  Date:     August 9, 2019
          //  Description:  This is the header file for all Arduino sketch files
          //            related to the ROM Emulator. It defines the ports used
          //            by the emulator as constants.
          //------------------------------------------------------------------------------

          //------------------------------------------------------------------------------
          // Control Pins
          //------------------------------------------------------------------------------
          const char EN0 = 13;
          const char EN1 = 12;
          const char EN3 = 11;
          const char EN4 = 10;

          const char DIR = 9;

          const char RD = 8;

          const char WRITE = 7;

          const char SEL = 6;

          // Address bits
          const char Addr[17] = {22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38};
          // Data bits
//          const char Data[8] = {39, 40, 41, 42, 43, 44, 45, 46};
          const char Data[8] = {46, 45, 44, 43, 42, 41, 40, 39};

          //------------------------------------------------------------------------------
          //  More constants
          //------------------------------------------------------------------------------

          // Buffer
          const int MAX_DATA_BUFFER = 100;

          // bit count
          const char ADDRESS_BITS = 17;
          const char DATA_BITS = 8;

          // Record type
          const char S_RECORD = 1;
          const char HEX_RECORD = 2;

          // Global Variables
          String incomingMessage = "";


          //------------------------------------------------------------------------------
          // Function prototypes
          //------------------------------------------------------------------------------

          // Write mode:
          void writeMode();
          bool parseSRecord(  unsigned long& address, 
                              int& addressBytes, 
                              byte data[], 
                              int& dataBytes);
          bool parseHexRecord(unsigned long& address, 
                              int& addressBytes, 
                              byte data[], 
                              int& dataBytes);
          bool writeBits( unsigned long& address, byte data[], int& dataBytes );
          bool verifyWrite( unsigned long address, byte data[], int dataBytes );

          // Read mode:
          void readMode();

          // Emulate mode:
          void emulateMode();

          // Helper methods:
          String waitForMessage();
          bool verifyMessage(String test, String want);

          #endif
