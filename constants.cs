using System;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Win32;

namespace Romulator {
	//-------------------------------------------------------------------------
	//	File:			Constants
	//	By:				Team Romulator:
	//					David Landry, chief software engineer
	//					Biniyam Yemane-Berhane, Nema Karimi, Thien Nguyen
	//	For:			CE / BEE Capstone; University of Washington, Bothell
	//					Developed for the School of STEM
	//	Advisor:		Dr. Arnold S. Berger
	//	Date:			August 9, 2019
	//	Description:	This header file declares some constants related to
	//						record types when writing to the ROM Emulator.
	//-------------------------------------------------------------------------
	enum RecordType {
		NOT_YET_DEFINED = 0,
		S_RECORD = 1,
		INTEL_HEX = 2,
		BAD_FILE = -1,
		BAD_FORMAT = -2,
		BAD_SIZE = -3,
		BAD_CHECKSUM = -4
	}

	enum FileStatus {
		CHAR_COUNT = 0,
		LINE_COUNT = 1,
		RECORD_TYPE = 2
	}
	
	enum WriteResult {
		RECORD_MISMATCH = -1,
		WRITE_FAIL = -2,
		SUCCESSFUL_WRITE = 1
	}
}