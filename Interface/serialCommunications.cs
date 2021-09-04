using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Win32;

namespace Romulator {
	//-------------------------------------------------------------------------
	//	Class:			SerialCommunications
	//	By:				Team Romulator:
	//					David Landry, chief software engineer
	//					Biniyam Yemane-Berhane, Nema Karimi, Thien Nguyen
	//	For:			CE / BEE Capstone; University of Washington, Bothell
	//					Developed for the School of STEM
	//	Advisor:		Dr. Arnold S. Berger
	//	Date:			August 9, 2019
	//	Description:	This class handles all serial communications, including
	//						port opening, sending, receiving, etc.
	//	Acknowledgements:
	//					Gytautas Jankauskas, a UW alumni, helped debug the code.
	//-------------------------------------------------------------------------
	public class SerialCommunications
    {
		
		static SerialPort serialPort;
		const int bufferMax = 1024;
        static byte[] buffer = new Byte[bufferMax];
        static int bufferLength = 0;
		//string incomingMessage;
		
		
		
		//---------------------------------------------------------------------
		//	Default constructor
		//	Assigns properties to a new SC object.
		//---------------------------------------------------------------------
		public SerialCommunications(	int baudRate = 9600, 
										Parity parity = Parity.None, 
										int dataBits = 8, 
										StopBits stopBits = StopBits.One) {
			string romCom = findPortName();
			if (romCom == "")
				throw new IOException
					("The Romulator is not attached to the PC.");
            serialPort = new SerialPort(	romCom, 
											baudRate, 
											parity, 
											dataBits, 
											stopBits);
            serialPort.ReadTimeout = 100; // Set to 100ms. Default is -1.
			serialPort.WriteTimeout = 100;
            serialPort.DataReceived += new 
				SerialDataReceivedEventHandler(serialPort_DataReceived);
            serialPort.Open();
        }
		
		
	
		
		//---------------------------------------------------------------------
		//	Method			Available
		//	Description:	Data available in the buffer.
		//---------------------------------------------------------------------
		public int Available() { return bufferLength; }
		public int BitsPerCharacter() { return	serialPort.DataBits + 
												(int)serialPort.StopBits + 1; }
		public int Baud() { return serialPort.BaudRate; }
		
		
		
		
		//---------------------------------------------------------------------
		//	Method:			findPortName
		//	Description:	Finds the name of the attached port.
		//	Returns:		A string consisting of the port name.
		//	Issues:			What if there are more than one COM port in use?
		//						Can we scan friendly names and select
		//						Arduino Mega 2560 from the array of friendly
		//						names generated?
		//---------------------------------------------------------------------
		static string findPortName()
        {
            // Get a list of serial port names.
            string[] ports = SerialPort.GetPortNames();

            // Display each port name to the console.
			int portCounter = 0;
			string portName = "";
            foreach(string port in ports)
            {
				portCounter++;
				if (portCounter == 1)
					portName = String.Copy(port);
				else
					Console.WriteLine("There are more than one attached ports.");
				portCounter++;
            }
			return portName;
        }
		
		
		//---------------------------------------------------------------------
		//	Method:			serialPort_DataReceived
		//	Description:	Event handler for incoming data.
		//---------------------------------------------------------------------
		private void serialPort_DataReceived(	object sender, 
												SerialDataReceivedEventArgs e) {
			//Console.WriteLine("Data has come in to the serial buffer.");
			lock (buffer)
            {
				//Console.WriteLine("I locked the buffer. No other threads can access it.");
                int bytesReceived = serialPort.Read(buffer, bufferLength, bufferMax - bufferLength);
				//Console.WriteLine("I read data from the serial port into buffer[].");
                if (bytesReceived > 0)
                {
                    bufferLength += bytesReceived;
					//Console.WriteLine("I adjusted bufferLength.");
                    if (bufferLength >= bufferMax)
                        throw new ApplicationException("Buffer Overflow.  Send shorter lines, or increase lineBufferMax.");
					//Console.WriteLine(serialPort.ReadExisting());
					//Console.WriteLine("Hi.");
                }
            }
		}
		

		
		//---------------------------------------------------------------------
		//	Method:			ReadLine
		//	Description:	Read a line from the buffer.
		//---------------------------------------------------------------------
        public string ReadLine() {
            string line = "";
            lock (buffer) {
                //-- Look for Return char in buffer --
                for (int i = 0; i < bufferLength; i++) {
                    //-- Consider EITHER CR or LF as end of line, so if both were received it would register as an extra blank line. --
                    if (buffer[i] == '\r' || buffer[i] == '\n') {
                        buffer[i] = 0; // Turn NewLine into string terminator
						for (int j = 0; j < i; j++) {
							line = line + ((char)buffer[j]);
						}
						if (buffer[i+1] == '\n') {
							bufferLength = bufferLength - i - 2;
							Array.Copy(buffer, i + 2, buffer, 0, bufferLength); // Shift everything past NewLine to beginning of buffer

						}
						else {	
							bufferLength = bufferLength - i - 1;
							Array.Copy(buffer, i + 1, buffer, 0, bufferLength); // Shift everything past NewLine to beginning of buffer
						}
                        break;
                    }
                }
            }
            return line;
        }

		//---------------------------------------------------------------------
		//	Method:			Print
		//	Description:	Send a string via serial.
		//---------------------------------------------------------------------
        public void Print( string line )
        {
            System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
            byte[] bytesToSend = encoder.GetBytes(line);
            serialPort.Write(bytesToSend, 0, bytesToSend.Length);
        }



		//---------------------------------------------------------------------
		//	Method:			PrintLine
		//	Description:	Send a string via serial, appending a carriage
		//						return at the end.
		//---------------------------------------------------------------------
        public void PrintLine(string line)
        {
            Print(line + "\r");
        }

        public void PrintClear()
        {
            byte[] bytesToSend = new byte[2];
            bytesToSend[0] = 254;
            bytesToSend[1] = 1;
            serialPort.Write(bytesToSend, 0, 2);
            Thread.Sleep(500); // LCD is slow, pause for 500ms before sending more chars
        }
		
		
		//---------------------------------------------------------------------
		//	Method:			WaitForData
		//	Description:	Wait for data to come in from the ROM emulator.
		//---------------------------------------------------------------------
		public string WaitForData()
		{
			while (bufferLength == 0);
			Thread.Sleep(100);
			return ReadLine();
		}
    }
}