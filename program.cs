using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Win32;

namespace Romulator {
	
	
	//-------------------------------------------------------------------------
	//	Class:			Program
	//	By:				Team Romulator:
	//					David Landry, chief software engineer
	//					Biniyam Yemane-Berhane, Nema Karimi, Thien Nguyen
	//	For:			CE / BEE Capstone; University of Washington, Bothell
	//					Developed for the School of STEM
	//	Advisor:		Dr. Arnold S. Berger
	//	Date:			August 9, 2019	
	//	Description:	The main Romulator program.
	//	Properties:	romulatorPort:		A SerialCommunications object.
	//										This is a wrapper class for the
	//										SerialPort class.
	//				romulatorCommand:	A string.
	//										Defines which command to direct
	//										to within Romulator.
	//				romulatorFile:		A string.
	//										The file name to work with, whether
	//										the source code for the romulator
	//										device or the name of the .csv for
	//										the	memory dump.
	//
	//	Methods:	Main:				The main method.
	//				PrintIntro:			Prints the intro lines.
	//				AssignArguments:	Assigns command line arguments
	//										to the Program properties.
	//				ParseCommands:		Parses romulatorCommand for a correct
	//										command.
	//				Help:				Displays a help screen.
	//				MainMenu:			Calls the main menu to guide the user
	//										through the romulator.
	//				StartSerial:		Establishes the serial port and opens
	//										it up.
	//				ReadData:			Read data from the ROM emulator's
	//										memory.
	//				WriteData:			Write data to the ROM emulator's
	//										memory.
	//				WriteToEmulator:	Part 2 of WriteData. Sends data over
	//										to the ROM emulator.
	//				Emulate:			Set the ROM emulator to emulation mode.
	//-------------------------------------------------------------------------
	class Program {
		
		//---------------------------------------------------------------------
		//	Class properties
		//---------------------------------------------------------------------
		static SerialCommunications romulatorPort;
		static string romulatorCommand;
		static string romulatorFile;
		static readonly string[] recordString = {	"undefined", "Motorola S-Record",
											"Intel Hex", "file is missing",
											"invalid record", "invalid record",
											"invalid record"	};

		
		//---------------------------------------------------------------------
		//	Class Methods
		//---------------------------------------------------------------------
		
		//---------------------------------------------------------------------
		//	Method:			Main
		//	Description:	The main method that starts when Romulator is
		//						invoked.
		//	Parameters:		The function (write, -w, --write, read, -r, --read,
		//						emulate, -e, --emulate, help, -h, --help).
		//					File name if read or write were indicated.
		//	Returns:
		//---------------------------------------------------------------------
		public static void Main(string[] args) {
			PrintIntro();
			if (!AssignArguments(args.Length, args)) {
				Console.WriteLine(	"Invalid command line arguments. Type" +
									" \"romulator help\" for a list of " +
									"commands.");
				return;
			}
			if (!romulatorCommand.Equals("m")) {
				if (!ParseCommands()) {
					Console.WriteLine(	"Invalid command. Type \"romulator " +
										"help\" for a list of commands.");
					return;
				}
			}
			else
				MainMenu();
			if (romulatorCommand.Equals("h"))
				Help();
			else if (romulatorCommand.Equals("r"))
				ReadData();
			else if (romulatorCommand.Equals("w"))
				WriteData();
			else if (romulatorCommand.Equals("e"))
				Emulate();
			else if (!romulatorCommand.Equals("q"))
				Console.WriteLine(	"An invalid command was passed to the " +
									"Romulator. Type \"romulator help\" for " +
									"a list of commands.");
		}
		
		
		//---------------------------------------------------------------------
		//	Method:			printIntro
		//	Description:	Prints the intro.
		//	Parameters:		None
		//	Returns:		Void
		//---------------------------------------------------------------------
		static void PrintIntro() {
			Console.WriteLine("=======================================");
			Console.WriteLine("Romulator");
			Console.WriteLine("Copyright 2021 by Team Romulator");
			Console.WriteLine("Department of Science and Engineering");
			Console.WriteLine("University of Washington, Bothell");
			Console.WriteLine("=======================================");
		}
		
		
		//---------------------------------------------------------------------
		//	Method:			AssignArguments
		//	Description:	Assigns string values to command and file.
		//	Parameters:		int argNumber:	number of arguments.
		//					string[] args:	the command line arguments.
		//	Returns:		bool:			false if there are more than 2
		//										command line arguments.
		//									true otherwise.
		//---------------------------------------------------------------------
		static bool AssignArguments(int argNumber, string[] args) {
			if (argNumber == 0)
				romulatorCommand = "m";
			if (argNumber == 1 || argNumber == 2)
				romulatorCommand = args[0];
			if (argNumber == 2)
				romulatorFile = args[1];
			if (argNumber > 2)
				return false;
			romulatorCommand = romulatorCommand.ToLower();
			return true;
		}
		
		
		//---------------------------------------------------------------------
		//	Method:			ParseCommands
		//	Description:	Parses the command for correctness.
		//	Parameters:		None
		//	Returns:		bool:	True if it's a valid command.
		//							False otherwise.
		//---------------------------------------------------------------------
		static bool ParseCommands() {
			if (	romulatorCommand.Equals("help") ||
					romulatorCommand.Equals("-h") ||
					romulatorCommand.Equals("--help"))
				romulatorCommand = "h";
			else if (	romulatorCommand.Equals("write") ||
						romulatorCommand.Equals("-w") ||
						romulatorCommand.Equals("--write"))
				romulatorCommand = "w";
			else if (	romulatorCommand.Equals("read") ||
						romulatorCommand.Equals("-r") ||
						romulatorCommand.Equals("--read"))
				romulatorCommand = "r";
			else if (	romulatorCommand.Equals("emulate") ||
						romulatorCommand.Equals("-e") ||
						romulatorCommand.Equals("--emulate"))
				romulatorCommand = "e";
			else
				return false;
			return true;
		}
		
		
		//---------------------------------------------------------------------
		//	Method:			Help
		//	Description:	Displays the help screen.
		//	Parameters:		None
		//	Returns:		Void
		//---------------------------------------------------------------------
		static void Help() {
			Console.WriteLine(	"To use Romulator, make sure the device is " +
								"first attached to the computer via USB.");
			Console.WriteLine(	"Type \".\\romulator\" to run Romulator from" +
								" a menu.");
			Console.WriteLine(	"Otherwise, you may pass in command line" +
								" arguments:");
			Console.WriteLine(	"\t\"help\" or \"-h\":");
			Console.WriteLine(	"\t\tBrings up this screen.");
			Console.WriteLine(	"\t\"read <filename>\" or \"-r <filename>\":");
			Console.WriteLine(	"\t\tObtain a snapshot of the Romulator's " +
								"memory, saving it to a .csv file on the " +
								"computer.");
			Console.WriteLine(	"\t\"write <filename>\" or \"-w " +
								"<filename>\":");
			Console.WriteLine(	"\t\tWrite an S-Record or Intel Hex record " +
								"to the Romulator, where it will be parsed" +
								" and passed on.");
			Console.WriteLine(	"\t\t\tto the appropriate memory addresses.");
			Console.WriteLine(	"\t\"emulate\" or \"-e\":");
			Console.WriteLine(	"\t\tRun the Romulator in emulation mode, " +
								"where it will behave like a ROM to the " +
								"target system.");
			Console.WriteLine(	"\t\tBe sure the Romulator is attached to" +
								" the target system via the ribbon cable " +
								"interface before using"); 
			Console.WriteLine(	"\t\t\tthis mode.");
		}
		
		
		//---------------------------------------------------------------------
		//	Method:			MainMenu
		//	Description:	The Romulator main menu, entered when no command
		//						line arguments have been entered.
		//	Parameters:		None
		//	Returns:		Void
		//---------------------------------------------------------------------
		static void MainMenu() {
			bool quit = false;
			do {
				Console.WriteLine(	"Main menu:");
				Console.WriteLine(	"\t1.\tWrite an S-Record or Intel Hex" +
									" to the Romulator.");

				Console.WriteLine(	"\t2.\tEmulate ROM on a target system.");
				Console.WriteLine(	"\t3.\tHelp using the Romulator.");
				Console.WriteLine(	"\t4.\tRead data from RAM.");
				Console.WriteLine(	"\tEnter Q to quit.\n");
				Console.Write(">");
				string selection = Console.ReadLine();
				if (selection.Equals("1")) {
					romulatorCommand = "w";
					Console.WriteLine("Enter a file name or path to open.");
					Console.Write(">");
					romulatorFile = Console.ReadLine();
					quit = true;
				}
				else if (selection.Equals("2")) {
					romulatorCommand = "e";
					quit = true;
				}
				else if (selection.Equals("3")) {
					romulatorCommand = "h";
					quit = true;
				}
				else if (selection.Equals("4")) {
					romulatorCommand = "r";
					quit = true;
				}
				else if (selection.ToLower().Equals("q")) {
					romulatorCommand = "q";
					quit = true;
				}
			} while (!quit);
		}
		
		
		
		//---------------------------------------------------------------------
		//	Method:			StartSerial
		//	Description:	Start the serial connection.
		//	Parameters:		None
		//	Returns:		bool:	True if a connection has been established.
		//							False otherwise.
		//---------------------------------------------------------------------
		static bool StartSerial() {
			// Establish our serial port.
			try {
				romulatorPort = new SerialCommunications();
			}
			catch (IOException) {
				return false;
			}
			return true;
		}
		
		
		
		//---------------------------------------------------------------------
		//	Method:			WriteData
		//	Description:	Write S-Records or Intel Hex records to the
		//						ROM emulator, one line at a time.
		//	Parameters:		None
		//	Returns:		bool:	True if the write was a success.
		//							False otherwise.
		//---------------------------------------------------------------------
		static bool WriteData() {
			if (!StartSerial()) {
					Console.Write("The Romulator is not attached to the ");
					Console.WriteLine("computer. Type \"romulator help\" for");
					Console.WriteLine(" more information.");
					return false;
			}
			int[] test = new int[3];
			Verify.verifyRecord(romulatorFile, ref test);
			switch(test[(int)FileStatus.RECORD_TYPE]) {
				case (int)RecordType.BAD_FILE:
					Console.Write("The file you entered does not exist. ");
					Console.WriteLine("Try again.");
					return false;
				case (int)RecordType.BAD_FORMAT:
					Console.Write("The file is not a valid S-Record or ");
					Console.WriteLine("Intel Hex.");
					return false;
				case (int)RecordType.BAD_SIZE:
					Console.Write("At least one record is an invalid ");
					Console.WriteLine("size.");
					return false;
				case (int)RecordType.BAD_CHECKSUM:
					Console.Write("At least one record failed the checksum ");
					return false;
				case (int)RecordType.S_RECORD:
					Console.WriteLine("Valid Motorola S Record file.");
					switch (WriteToEmulator(test)) {
						case (int)WriteResult.RECORD_MISMATCH:
							Console.Write("There was a problem communicating");
							Console.WriteLine(" the record to the emulator.");
							return false;
						case (int)WriteResult.WRITE_FAIL:
							Console.Write("The emulator failed to correctly ");
							Console.WriteLine("write data to memory.");
							return false;
						case (int)WriteResult.SUCCESSFUL_WRITE:
							Console.WriteLine("Successful write!");
							return true;
						default:
							Console.WriteLine("An unknown error occured.");
							return false;
					}  
				case (int)RecordType.INTEL_HEX:
					Console.WriteLine("Valid Intel Hex record.");
					switch (WriteToEmulator(test)) {
						case (int)WriteResult.RECORD_MISMATCH:
							Console.Write("There was a problem communicating");
							Console.WriteLine(" the record to the emulator.");
							return false;
						case (int)WriteResult.WRITE_FAIL:
							Console.Write("The emulator failed to correctly ");
							Console.WriteLine("write data to memory.");
							return false;
						case (int)WriteResult.SUCCESSFUL_WRITE:
							Console.WriteLine("Successful write!");
							return true;
					}
					break;
				default:
					Console.WriteLine("An unknown error occured.");
					return false;
			}
			Console.WriteLine("An unknown error occured.");
			return false;
		}
		
		
		
		//---------------------------------------------------------------------
		//	Method:			WriteToEmulator
		//	Description:	Write S-Records or Intel Hex records to the
		//						ROM emulator, one line at a time.
		//	Parameters:		None
		//	Returns:		int:	True if the write was a success.
		//							False otherwise.

		//---------------------------------------------------------------------
		static int WriteToEmulator(int[] test) {
			using (FileStream fs = new FileStream(	romulatorFile, 
													FileMode.Open, 
													FileAccess.Read)) {
				using (StreamReader recordStream = new StreamReader(fs)) {
					for (int i = 0; i < test[(int)FileStatus.LINE_COUNT]; i++) {
						string line = "";
						string verify = "";
						string[] lines = new string[2];
						int stringTest = 0;
						line = recordStream.ReadLine();
						// lines[0] & line[1] are the first and second half
						//		of a record. The record is cut in half because
						//		longer records might not get processed
						//		correctly.
						lines[0] = line.Substring(0, line.Length / 2);
						lines[1] = line.Substring(	line.Length / 2, 
													(line.Length % 2 == 1) ?
													(line.Length/2)+1 :
													(line.Length/2));
						Console.WriteLine("Sending record " + (i+1) + " (" + line + ")" + ":");
						
						
						// Send first half
						romulatorPort.Print(lines[0]);
						romulatorPort.WaitForData();
						
						// Send second half
						romulatorPort.Print(lines[1]);
						verify = romulatorPort.WaitForData();
						
						// Verify that the record was correctly delivered.
						stringTest = string.Compare(line, verify);
						if (stringTest == 0) {
							Console.WriteLine("Record delivered successfully.\n");
							romulatorPort.Print("yes");
						}
						else {
							Console.WriteLine("line: " + line + ".  verify: " + verify + ".");
							romulatorPort.Print("no");
							return (int)WriteResult.RECORD_MISMATCH;
						}
						
						// At this point, the Romulator is toggling bits to set data to the
						// memory locations.
						// Wait for acknowledgement that the Romulator's ready for the next record.
						verify = romulatorPort.WaitForData();
						Console.WriteLine(verify);
						if (verify == "error")
							return (int)WriteResult.WRITE_FAIL;
					}
				}
			}
			return (int)WriteResult.SUCCESSFUL_WRITE;
		}
		
		
		
		//---------------------------------------------------------------------
		//	Method:			ReadData
		//	Description:	Read the data stored in the Romulator's RAM.
		//	Parameters:		string:		the file name to save it to.
		//	Returns:		Bool:		true if successfully
		//								false if not.
		//---------------------------------------------------------------------
		static void ReadData() {
			if (!StartSerial()) {
				Console.Write("The Romulator is not attached to the ");
				Console.WriteLine("computer. Type \"romulator help\" for");
				Console.WriteLine(" more information.");
				return;
			}
			string data = "";
			string Add = "";
			string Byte = "";
			string z = "";
			romulatorPort.Print("read");
			Console.Write("Please enter the address start point(use 0x#### format): ");
			Add = Console.ReadLine();
			Console.Write("Please enter the Byte count number: ");
			Byte = Console.ReadLine();
			romulatorPort.Print(Add);
			z = romulatorPort.WaitForData();
			if(z == "acknowledged"){
				romulatorPort.Print(Byte);
			}else if(z != "acknowledged"){
				Console.Write("Something happend");
			}
			while (data != "finished") {
				data = romulatorPort.WaitForData();
				Console.WriteLine(data);
				romulatorPort.Print("next");
			}
		}

		
		//---------------------------------------------------------------------
		//	Method:			Emulate
		//	Description:	Set the ROM emulator into Emulate mode.
		//---------------------------------------------------------------------
		static void Emulate() {
			if (!StartSerial()) {
				Console.Write("The Romulator is not attached to the ");
				Console.WriteLine("computer. Type \"romulator help\" for");
				Console.WriteLine(" more information.");
				return;
			}
			string quit = "";
			romulatorPort.Print("emulate");
			Console.WriteLine("Enter \"quit\" to stop emulating.");
			while (!quit.Equals("quit")) {
				quit = Console.ReadLine();
				quit = quit.ToLower();
			}
			romulatorPort.Print("quit");
		}
	}
}