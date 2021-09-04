using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Win32;

namespace Romulator {
	//-------------------------------------------------------------------------
	//	Class:			Verify
	//	By:				Team Romulator:
	//					David Landry, chief software engineer
	//					Biniyam Yemane-Berhane, Nema Karimi, Thien Nguyen
	//	For:			CE / BEE Capstone; University of Washington, Bothell
	//					Developed for the School of STEM
	//	Advisor:		Dr. Arnold S. Berger
	//	Date:			August 9, 2019	
	//	Description:	Verifies that a file contains valid S-Record or Intel
	//						Hex records. It performs 4 tests:
	//					 		Test 1: Does the file exist?
	//					 		Test 2: Does each record start with the 
	//								correct symbol?
	//					 		Test 3: Is each record the correct length?
	// 							Test 4: Is the checksum correct?
	//	Needs:			File name, received from Program.
	//-------------------------------------------------------------------------
	static public class Verify {
		
		//static FileStream verifier;
		//static string fileContents;
		
		//---------------------------------------------------------------------
		//	Method:			verifyRecord
		//	Description:	Main method & public facing of this class.
		//					Used to verify that a .S68 or .hex file contains
		//						valid records.
		//---------------------------------------------------------------------
		public static void verifyRecord(string fileName, ref int[] returnStatus) {
			//int charCount = 0;
			//int lineCount = 0;
			int recordType = (int)RecordType.NOT_YET_DEFINED;
			char testChar = ' ';
			
			//------------------------------
			//	Test 1: Does the file exist?
			//------------------------------
			if (File.Exists(fileName)) {
				using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read)) {
					using (StreamReader verifier = new StreamReader(fs)) {					
						while (!verifier.EndOfStream) {
							testChar = (char)verifier.Read();
							returnStatus[(int)FileStatus.CHAR_COUNT]++;
							
							//------------------------------------------------
							//	Test2S: Does each S-Record start with S?
							//------------------------------------------------
							if (testChar == 's' || testChar == 'S') {
								if (recordType == (int)RecordType.NOT_YET_DEFINED)
									recordType = (int)RecordType.S_RECORD;
								if (recordType == (int)RecordType.S_RECORD) {
									int checksumNum = 0;	// A record's checksum: the last 2 hex 
															//	digits.
									int byteCount = 0;		// Number of bytes that the count digits
															//	of a record corresponds to.
									int recordCharacters = 0;	// Number of remaining characters
																//	in current record.
									int sum = 0;
									// Valid SRecord: Stnnaaaadddd...ddcc\n
									//		S:	Establishes record as an SRecord.
									//		t:	Record type. 0-9.
									//		n:	Number of bytes that follow (2 hex digits per byte) (count).
									//		a:	Starting address for the record (address)
									//		d:	The data (data).
									//		c:	Checksum (1s complement of the sum of bytes of address, count,
									//				and data.	
									
									// Get the SRecord type.
									char testRecordType = (char)verifier.Read();
									returnStatus[(int)FileStatus.CHAR_COUNT]++;
									// Get the count byte.
									string countByte = "";
									countByte += (char)verifier.Read();
									returnStatus[(int)FileStatus.CHAR_COUNT]++;
									countByte += (char)verifier.Read();
									returnStatus[(int)FileStatus.CHAR_COUNT]++;
									
									try{
										byteCount = int.Parse(countByte, NumberStyles.AllowHexSpecifier);
									}
									catch(Exception){
										returnStatus[(int)FileStatus.CHAR_COUNT] = 0;
										returnStatus[(int)FileStatus.LINE_COUNT] = 0;
										returnStatus[(int)FileStatus.RECORD_TYPE] = (int)RecordType.BAD_FORMAT;
										return;
									}
									// recordCharacters is the number of
									//	remaining characters in the current
									//	record.
									recordCharacters = byteCount * 2;
									
									//---------------------------------------------
									//	Test 3S: Is each record the correct length?
									//---------------------------------------------
									string remainingContents = "";
									for (int i = 0; i < recordCharacters; i++) {
										testChar = (char)verifier.Read();
										// If there is a newline here, the
										//	record is shorter than the
										//	indicated byte count, so it's not
										//	a valid record.
										if (testChar.Equals('\n')) {
											returnStatus[(int)FileStatus.CHAR_COUNT] = 0;
											returnStatus[(int)FileStatus.LINE_COUNT] = 0;
											returnStatus[(int)FileStatus.RECORD_TYPE] = (int)RecordType.BAD_SIZE;
											return;
										}
										remainingContents += testChar;
										returnStatus[(int)FileStatus.CHAR_COUNT]++;
									}
									// If the next character after obtaining
									//	the rest of the characters isn't a
									//	newline or CR/NL, then the record is 
									//	longer than the indicated byte count, 
									//	so it's not a valid record.
									testChar = (char)verifier.Read();
									if (!(testChar.Equals('\r') || testChar.Equals('\n'))) {
										returnStatus[(int)FileStatus.CHAR_COUNT] = 0;
										returnStatus[(int)FileStatus.LINE_COUNT] = 0;
										returnStatus[(int)FileStatus.RECORD_TYPE] = (int)RecordType.BAD_SIZE;
										Console.WriteLine("!(testChar.Equals('1') || testChar.Equals('2'))");
										return;
									}
									if (testChar.Equals('\r')) {
										testChar = (char)verifier.Read();
										if(!(testChar.Equals('\n'))) {
											returnStatus[(int)FileStatus.CHAR_COUNT] = 0;
											returnStatus[(int)FileStatus.LINE_COUNT] = 0;
											returnStatus[(int)FileStatus.RECORD_TYPE] = (int)RecordType.BAD_SIZE;
											Console.WriteLine("testChar.Equals('3') && !(testChar.Equals('4')");
											return;

										}
									}
									returnStatus[(int)FileStatus.CHAR_COUNT]++;
									returnStatus[(int)FileStatus.LINE_COUNT]++;

									//-----------------------------------
									//	Test 4S: Is the checksum correct?
									//-----------------------------------
									// Sum up all bytes prior to the checksum.
									for (int i = 0; i < recordCharacters - 2; i += 2) {
										string currentNumber = remainingContents.Substring(i, 2);
										try {
											sum += int.Parse(currentNumber, NumberStyles.AllowHexSpecifier);
										}
										catch(Exception) {
											returnStatus[(int)FileStatus.CHAR_COUNT] = 0;
											returnStatus[(int)FileStatus.LINE_COUNT] = 0;
											returnStatus[(int)FileStatus.RECORD_TYPE] = (int)RecordType.BAD_FORMAT;
											return;
										}
									}
									// Add the count bytes to the sum.
									sum += byteCount;
									
									// Not the sum.
									sum = ~sum;
									
									// Get a shifted version of the sum to zero
									//	out the LSB
									int shifted = sum >> 8;
									shifted <<= 8;
									// Isolate sum's LSB.
									sum -= shifted;
									// Get the checksum.
									string checksum = remainingContents.Substring(remainingContents.Length - 2);
									// Convert the checksum.
									
									try {
										checksumNum = int.Parse(checksum, NumberStyles.AllowHexSpecifier);
									}
									catch(Exception) {
										returnStatus[(int)FileStatus.CHAR_COUNT] = 0;
										returnStatus[(int)FileStatus.LINE_COUNT] = 0;
										returnStatus[(int)FileStatus.RECORD_TYPE] = (int)RecordType.BAD_FORMAT;
										return;
									}
									
									if (sum != checksumNum) {
										returnStatus[(int)FileStatus.CHAR_COUNT] = 0;
										returnStatus[(int)FileStatus.LINE_COUNT] = 0;
										returnStatus[(int)FileStatus.RECORD_TYPE] = (int)RecordType.BAD_CHECKSUM;
										return;
									}
									
								} // end if (recordType == (int)RecordType.S_RECORD)
								else {
									returnStatus[(int)FileStatus.CHAR_COUNT] = 0;
									returnStatus[(int)FileStatus.LINE_COUNT] = 0;
									returnStatus[(int)FileStatus.RECORD_TYPE] = (int)RecordType.BAD_FORMAT;
									return;
								}
							} // end if (testChar == 'S') (S-Record tests)
								
							
							//------------------------------------------------
							//	Test2H: Does each hex Record start with :?
							//------------------------------------------------
							else if (testChar == ':') {
								if (recordType == (int)RecordType.NOT_YET_DEFINED)
									recordType = (int)RecordType.INTEL_HEX;
								if (recordType == (int)RecordType.INTEL_HEX) {
									int checksumNum = 0;	// A record's checksum: the last 2 hex 
															//	digits.
									int byteCount = 0;		// Number of bytes that the count digits
															//	of a record corresponds to.
									int recordCharacters = 0;	// Number of remaining characters
																//	in current record.
									int sum = 0;
									// Valid Intel Hex: :nnaaaattdd...ddcc\n
									//		n:	Number of bytes that follow (2 hex digits per byte) (count).
									//		a:	Starting address for the record (address)
									//		t:	The hex record type. (type)
									//		d:	The data (data).
									//		c:	Checksum (2s complement of the sum of bytes of non-checksum
									//				values).
									string count = "";
									count += (char)verifier.Read();
									count += (char)verifier.Read();
									returnStatus[(int)FileStatus.CHAR_COUNT] += 2;
									try {
										byteCount = int.Parse(count, NumberStyles.AllowHexSpecifier);
									}
									catch (Exception) {
										returnStatus[(int)FileStatus.CHAR_COUNT] = 0;
										returnStatus[(int)FileStatus.LINE_COUNT] = 0;
										returnStatus[(int)FileStatus.RECORD_TYPE] = (int)RecordType.BAD_FORMAT;
										return;
									}
									recordCharacters = (byteCount * 2)/*data*/ + 4/*address*/ + 2/*type*/ + 2/*checksum*/;
									
									//--------------------------------------------
									// Test 3H: Is each record the correct length?
									//--------------------------------------------
									string contents = "";
									for (int i = 0; i < recordCharacters; i++) {
										testChar = (char)verifier.Read();
										if (testChar.Equals('\n') || testChar.Equals('\r')) {
											returnStatus[(int)FileStatus.CHAR_COUNT] = 0;
											returnStatus[(int)FileStatus.LINE_COUNT] = 0;
											returnStatus[(int)FileStatus.RECORD_TYPE] = (int)RecordType.BAD_SIZE;
											Console.WriteLine("testChar.Equals('5') || testChar.Equals('6')");
											return;
										}
										contents += testChar;
										returnStatus[(int)FileStatus.CHAR_COUNT]++;
									}
									testChar = (char)verifier.Read();
									if (testChar.Equals('\r')) {
										returnStatus[(int)FileStatus.CHAR_COUNT]++;
										testChar = (char)verifier.Read();
										if (!testChar.Equals('\n')) {
											returnStatus[(int)FileStatus.CHAR_COUNT] = 0;
											returnStatus[(int)FileStatus.LINE_COUNT] = 0;
											returnStatus[(int)FileStatus.RECORD_TYPE] = (int)RecordType.BAD_SIZE;
											Console.WriteLine("testChar.Equals('7') && !(testChar.Equals('8') x2");
											return;
										}
									}
									else if (!testChar.Equals('\n')) {
										returnStatus[(int)FileStatus.CHAR_COUNT] = 0;
										returnStatus[(int)FileStatus.LINE_COUNT] = 0;
										returnStatus[(int)FileStatus.RECORD_TYPE] = (int)RecordType.BAD_SIZE;
										Console.WriteLine("!testChar.Equals('9')");
										return;
									}
									returnStatus[(int)FileStatus.CHAR_COUNT]++;
									returnStatus[(int)FileStatus.LINE_COUNT]++;

									//---------------------------------
									// Test 4H: Is the checksum correct?
									//---------------------------------
									for (int i = 0; i < recordCharacters-2; i += 2) {
										string numbers = contents.Substring(i,2);
										try {
											sum += int.Parse(numbers,NumberStyles.AllowHexSpecifier);
										}
										catch (Exception) {
											returnStatus[(int)FileStatus.CHAR_COUNT] = 0;
											returnStatus[(int)FileStatus.LINE_COUNT] = 0;
											returnStatus[(int)FileStatus.RECORD_TYPE] = (int)RecordType.BAD_CHECKSUM;
											return;
										}
									}
									sum += byteCount;
									// 2's complement of sum:
									sum = ~sum + 1;
									// Shift to isolate the LSB:
									int shifted = sum >> 8;
									shifted <<= 8;
									// shifted should equal xx...x00.
									sum -= shifted;
									string checksum = contents.Substring(contents.Length - 2, 2);
									try {
										checksumNum = int.Parse(checksum, NumberStyles.AllowHexSpecifier);
									}
									catch (Exception) {
										returnStatus[(int)FileStatus.CHAR_COUNT] = 0;
										returnStatus[(int)FileStatus.LINE_COUNT] = 0;
										returnStatus[(int)FileStatus.RECORD_TYPE] = (int)RecordType.BAD_FORMAT;
										return;
									}
									if (sum != checksumNum) {
										returnStatus[(int)FileStatus.CHAR_COUNT] = 0;
										returnStatus[(int)FileStatus.LINE_COUNT] = 0;
										returnStatus[(int)FileStatus.RECORD_TYPE] = (int)RecordType.BAD_CHECKSUM;
										return;

									}									
								} // end if (recordType == (int)RecordType.INTEL_HEX)
								else {
									returnStatus[(int)FileStatus.CHAR_COUNT] = 0;
									returnStatus[(int)FileStatus.LINE_COUNT] = 0;
									returnStatus[(int)FileStatus.RECORD_TYPE] = (int)RecordType.BAD_FORMAT;
									return;
								}
							} // end if (testChar == ':') (Intel Hex tests)
							else {
								returnStatus[(int)FileStatus.CHAR_COUNT] = 0;
								returnStatus[(int)FileStatus.LINE_COUNT] = 0;
								returnStatus[(int)FileStatus.RECORD_TYPE] = (int)RecordType.BAD_FORMAT;
								return;
							}
						} // end while (!verifier.EndOfStream)
					} // end using (StreamReader verifier)
				} // end using (FileStream fs)
				returnStatus[(int)FileStatus.RECORD_TYPE] = recordType;
			} // end if (File.Exists(fileName))
			else {
				returnStatus[(int)FileStatus.CHAR_COUNT] = 0;
				returnStatus[(int)FileStatus.LINE_COUNT] = 0;
				returnStatus[(int)FileStatus.RECORD_TYPE] = (int)RecordType.BAD_FILE;
			}
				
		}
		/*
		//---------------------------------------------------------------------
		//	Method:			verifySRecord
		//	Description:	Verifies an S Record
		//---------------------------------------------------------------------
		int verifySRecord {
			
		}
		*/
		
		//---------------------------------------------------------------------
		//	Method:			verifyHex
		//	Description:	Verifies a Hex record
		//---------------------------------------------------------------------
		
	}
}