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


#ifndef MAIN_H_
#define MAIN_H_

#define MHZ_168 1

#include "stm32f4xx.h"

extern volatile uint32_t Ticks;
extern volatile uint8_t Overflow;
extern volatile uint32_t Irqs;

#define SAMPLING_MODE_CONTINUOUS     0
#define SAMPLING_MODE_TRANSITIONONLY 1

// Markers for data transmitted for transition-only mode.
#define SAMPLE_MARKER 0xbf
#define PERIOD_MARKER 0xbd
#define ROLLOVER_MARKER 0xbe

// Settings
extern uint8_t SamplingActive;
extern uint16_t SamplingTime;
extern uint8_t SamplingChannels;
extern uint32_t SamplingRate;
extern uint8_t SamplingCompression;
extern uint8_t SamplingMode;

#ifdef __cplusplus
 extern "C" {
#endif

extern void TimerInit(uint32_t TimerBaseClockRate, uint32_t DesiredFequency);
extern void TimerDenit(void);

extern void Copyright(void);
extern char *itoa(signed long);
extern void PingResponse(void);
extern void UsartInit(void);
extern void UsartSendString(char *);
extern void UsartSendChar(char);
extern char UsartGetchar(void);
extern char *UsartGets(void);

extern void ClearSampleQueue(void);
extern int16_t SampleQueueIsEmpty(void);
extern int16_t SampleQueueIsFull(void);
extern int16_t EnqueueSample(uint8_t sample);
extern void EnqueueFinalSample(void);
extern uint8_t DequeueSample(void);
extern void ProcessCommands(void);

#ifdef __cplusplus
}
#endif
#endif /* MAIN_H_ */
