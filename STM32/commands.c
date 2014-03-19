//
//    8-Channel Logic Analyzer
//    Copyright (C) 2014  Bob Foley
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include "main.h"

uint8_t SamplingActive = 0;
uint16_t SamplingTime = 1000;
uint8_t SamplingChannels = 4;
uint32_t SamplingRate = 1000;
uint8_t SamplingCompression = 0;
uint8_t SamplingMode = SAMPLING_MODE_CONTINUOUS;

/**
 * @brief  Process commands and settings sent to us over the serial port.
 * @param  none
 * @retval none
 *
 *   Settings
 *   ========
 *   Settings are sent via USART in a simple format: a 4 character setting name
 *   followed by an equal sign with a value.
 *
 *   CHAN=<# channels>
 *   RATE=<sampling rate in Hz>
 *   TIME=<total sample time in ms>
 *   COMP=<Y/N compression>
 *   MODE=<T/C compression>
 *
 *   Commands
 *   ========
 *   START
 *   STOP
 *   COPY
 *   PING
 */
void ProcessCommands() {
	char *p = UsartGets();
	uint32_t v;

	if (p == NULL )
		return;

	if (strcmp(p, "START") == 0)
		SamplingActive = 1;
	else if (strcmp(p, "STOP") == 0)
		SamplingActive = 0;
	else if (strcmp(p, "COPY") == 0)
		Copyright();
	else if (strcmp(p, "PING") == 0)
		PingResponse();
	else if (strncmp(p, "CHAN=", 5) == 0) {
		v = atoi(p + 5);

		// Must be 1-8 */
		if (v >= 1 && v <= 8)
			SamplingChannels = v;
	} else if (strncmp(p, "RATE=", 5) == 0) {
		v = atoi(p + 5);

		// Minimum of 10 Hz, maximum of 10 MHz
		if (v > 10 && v < 10000000)
			SamplingRate = v;
	} else if (strncmp(p, "TIME=", 5) == 0) {
		v = atoi(p + 5);

		// Minimum of 10 ms, maximum of 100 s
		if(v > 10 && v < 100000)
			SamplingTime = v;
	}
	else if (strncmp(p, "COMP=", 5) == 0)
		SamplingCompression = (*(p + 5) == 'Y');
	else if (strncmp(p, "MODE=", 5) == 0)
		SamplingMode = (*(p + 5) == 'T') ? SAMPLING_MODE_TRANSITIONONLY : SAMPLING_MODE_CONTINUOUS;
}
