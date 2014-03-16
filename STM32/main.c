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
#include "main.h"
#include "evalboard.h"
#include "compress.h"

uint32_t ClockRate;
volatile uint32_t Ticks = 0;
uint32_t TimerBaseClockRate;

static void SendCompressedByte(uint8_t b);
static void SampleLoop(void);

/**
 * @brief  Program entry point.
 * @param  none
 * @retval 0
 */
int main(void) {
	RCC_ClocksTypeDef RCC_Clocks;

	SystemInit();

	// Get the clock rate so we can set the sampling rate correctly.
	RCC_GetClocksFreq(&RCC_Clocks);
	ClockRate = RCC_Clocks.HCLK_Frequency;

	// NOTE: STM generic

	// Our timer (TIM2) uses the APB1 (PCLK1) clock.
	TimerBaseClockRate = RCC_Clocks.PCLK1_Frequency;

	// Set the SysTick interrupt to fire once every millisecond.
	SysTick_Config(RCC_Clocks.SYSCLK_Frequency / 1000);

	LedInit();

	// Turn the Orange LED on, signifying WAIT...
	LedSet(LED_ORANGE, LED_MODE_ON);

	UsartInit();
	Copyright();

	uint32_t commandTicks = Ticks;

	LedSet(LED_ORANGE, LED_MODE_OFF);

	// Loop forever...
	while (1) {
		if (SamplingActive) {
			LedSet(LED_RED, LED_MODE_OFF);

			// Turn the Blue LED on when sampling.
			LedSet(LED_BLUE, LED_MODE_ON);
			SampleLoop();
			LedSet(LED_BLUE, LED_MODE_OFF);
		}

		// Check for commands every 1/10 second.
		if ((Ticks - commandTicks) >= 100) {
			ProcessCommands();
			commandTicks = Ticks;
		}
	}
	return 0;
}

/**
 * @brief  Get samples from the input pins and queue them for output via USART.
 *         The loop continues even after the sampling time is over in order to
 *         clear the queue.
 * @param  none
 * @retval none
 */
static void SampleLoop() {
	uint32_t startTicks;

	// In compression mode, initialize and send a "start compression" marker.
	if (SamplingCompression) {
		// Initialize compression, sending a pointer to the callback
		// function below that will receive the compressed data.
		if (CompressInit(&SendCompressedByte) < 0) {
			LedSet(LED_RED, LED_MODE_ON);
			return;
		}
		UsartSendString("<cmp>");
	}

	// CLear the output queue and set the timer to send an interrupt at our
	// current sampling rate.
	ClearSampleQueue();
	TimerInit(TimerBaseClockRate, SamplingRate);

	// Get our start time.
	startTicks = Ticks;

	// Loop until our time is up.
	while (1) {
		if (!SampleQueueIsEmpty()) {
			// Send the next available sample to the output.
			if (SamplingCompression) {
				CompressByte(DequeueSample());
			} else
				UsartSendChar(DequeueSample());
			LedSet(LED_ORANGE, LED_MODE_OFF);
		} else {
			// If the queue is empty and sampling is not active, we're done.
			if (!SamplingActive)
				break;
			LedSet(LED_ORANGE, LED_MODE_ON);
		}

		// Turn on the RED LED if the queue is full.
		if (SampleQueueIsFull()) {
			LedSet(LED_RED, LED_MODE_ON);
		} else
			LedSet(LED_RED, LED_MODE_OFF);

		// 'startTicks' is the millisecond count of when we started sampling.
		// 'Ticks' is the millisecond count now.
		if (SamplingActive && ((Ticks - startTicks) > SamplingTime)) {
			// Turn sampling off when time expires.
			// But continue in the loop until the queue is empty.
			TimerDenit();

			// If we're in transition-only mode, we need to send a final sample
			// to expand to the full sample time.
			if (SamplingMode == SAMPLING_MODE_TRANSITIONONLY)
				EnqueueFinalSample();

			SamplingActive = 0;
		}
	}

	// If compression is active, de-intialize and send and "stop compression" marker.
	if (SamplingCompression) {
		CompressFlush();
		CompressDenit();
		UsartSendString("</cmp>");
	}

	// If we had an overflow (i.e. we are sending data to the queue faster than it can be
	// sent out,) turn on the RED LED and send an overflow error.
	if (Overflow) {
		LedSet(LED_RED, LED_MODE_ON);
		UsartSendString("<err>Overflow</err>");
	}
}

/**
 * @brief  Callback function used to receive data output from the compression
 *         routines. This data simply gets sent out via USART.
 * @param  b: a byte output from the compression stream
 * @retval none
 */
static void SendCompressedByte(uint8_t b) {
	UsartSendChar(b);
}

/**
 * @brief  Systick interrupt handler. The interrupt is configured above to fire
 *         every millisecond. This is just used for counting the passage of
 *         time.
 * @param  none
 * @retval none
 */
void SysTick_Handler() {
	Ticks++;
}

/**
 * @brief  Send a firmware revision/copyright message
 * @param  none
 * @retval none
 */
void Copyright() {
	UsartSendString("Logic Analyzer by Bob Foley\r\n");
	UsartSendString("version 0.50  (rev. 15-Mar-2014 8:00 a.m.)\r\n\r\n");
}

/**
 * @brief  Send a response to a PING command
 * @param  none
 * @retval none
 */
void PingResponse() {
	UsartSendString("pOnG\r\n");
}
