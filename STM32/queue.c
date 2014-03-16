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

#include "main.h"

// 4K sample queue
#define QSIZE 4096

// The sample queue is fed by the timer interrupt and read from the main sample
// loop. Note that the 'qCount' variable is used by both of these 'threads',
// so it must be protected by a simple semaphore (enqueueBusy and dequeueBusy).
static volatile uint8_t queue[QSIZE];
static volatile uint32_t qHead, qTail, qCount;
static volatile uint8_t enqueueBusy, dequeueBusy;
static volatile uint8_t firstSample;
static volatile uint8_t prevSample;
volatile uint8_t Overflow;

// If our SamplingChannels setting is such that we
// can send more than one sample per byte (SamplingChannels <= 4)
// then we 'stack' samples before enqueuing them.
static uint8_t stackedSample;
static uint8_t stackShift;
static uint8_t stackMask;

/**
 * @brief  Clear the sample queue
 * @param  none
 * @retval none
 */
void ClearSampleQueue() {
	qHead = qTail = qCount = 0;
	enqueueBusy = dequeueBusy = 0;

	switch (SamplingChannels) {
	case 1:
		stackMask = 0x01;
		break;
	case 2:
		stackMask = 0x03;
		break;
	case 3:
		stackMask = 0x07;
		break;
	case 4:
		stackMask = 0x0f;
		break;
	default:
		stackMask = 0xff;
		break;
	}
	stackShift = 0;
	stackedSample = 0;
	Overflow = 0;
	firstSample = 1;
}

/**
 * @brief  Check if the sample queue is empty.
 * @param  none
 * @retval non-zero if the sample queue is empty, otherwise 0
 */
int16_t SampleQueueIsEmpty() {
	return qCount == 0;
}

/**
 * @brief  Check if the sample queue is full.
 * @param  none
 * @retval non-zero if the sample queue is full, otherwise 0
 */
int16_t SampleQueueIsFull() {
	return qCount >= QSIZE;
}

/**
 * @brief  Add a byte to the queue
 * @param  byte: the byte to add to the queue
 * @retval 0 if successful, -1 if the queue is full
 */
static uint16_t enqueueByte(uint8_t byte) {
	// Check if the queue is full.
	if (qCount >= QSIZE) {
		Overflow = 1;
		return -1;
	}

	queue[qTail++] = byte;
	if (qTail == QSIZE)
		qTail = 0;

	// We need to protect 'qCount' with a simple semaphore. If the main
	// 'thread' is dequeueing, we need to wait before incrementing the count.
	while (dequeueBusy)
		;
	enqueueBusy = 1;
	qCount++;
	enqueueBusy = 0;
	return 0;
}

/**
 * @brief  Add a sample to the queue. A sample may be less than 8 bits,
 *         so if the sampling channels is 4 or less, we can stack multiple
 *         samples per byte.
 * @param  sample: the sample to add to the queue
 * @retval 0 if successful, -1 if the queue is full
 */
int16_t EnqueueSample(uint8_t sample) {
	if (SamplingMode == SAMPLING_MODE_TRANSITIONONLY) {
		// When in transition mode, we need to send a
		// rollover marker every time we roll over 16 bits.
		if ((Irqs & 0xffff) == 0) {
			uint16_t rolloverCnt = (Irqs >> 16);

			// Queue the marker.
			enqueueByte(ROLLOVER_MARKER);

			// Queue the timestamp -- low byte first.
			enqueueByte(rolloverCnt & 0xff);
			enqueueByte((rolloverCnt >> 8) & 0xff);
			enqueueByte(0);
		}
	} else {
		// Check if we can stack more samples per byte.
		// Note that transition-only mode does not stack samples.
		if (SamplingChannels <= 4) {
			stackedSample |= (sample & stackMask) << stackShift;
			stackShift += SamplingChannels;

			// If we don't yet have a full byte, wait for the next sample
			// before queuing it.
			if (stackShift < 8)
				return 0;

			sample = stackedSample;
			stackShift = 0;
			stackedSample = 0;
		}
	}

	if (SamplingMode == SAMPLING_MODE_TRANSITIONONLY) {
		// If we're in transition mode, we need to add a timestamp as well.
		// This comes in the form of the lower 16 bits of 'Irqs'.

		// Skip samples that are the same.
		if (!firstSample && (prevSample == sample))
			return 0;

		firstSample = 0;
		prevSample = sample;

		// Queue the marker.
		enqueueByte(SAMPLE_MARKER);

		// Queue the timestamp -- low byte first.
		enqueueByte(Irqs & 0xff);
		enqueueByte((Irqs >> 8) & 0xff);
	}

	// Finally, enqueue the sample byte (possibly multiple samples).
	return enqueueByte(sample);
}

/**
 * @brief  Add a final sample to the queue. This is used in transition-only
 *         mode. This is done because the last sample we sent may have been a
 *         while ago and we need to have a 'duration' of the last signal state.
 * @param  none
 * @retval none
 */
void EnqueueFinalSample() {
	EnqueueSample(prevSample == 0 ? 0xff : 0);
}

/**
 * @brief  Remove a sample (or multiple samples if they are 'stacked' in a
 *         single byte) from the queue. A call to SampleQueueIsEmpty() should
 *         be made prior to calling this function.
 * @param  none
 * @retval the sample byte, or zero if the queue was empty
 */
uint8_t DequeueSample() {
	uint8_t sample;

	// Check if the queue is empty -- return zero if it is.
	if (qCount == 0)
		return 0;

	sample = queue[qHead++];
	if (qHead == QSIZE)
		qHead = 0;

	// We need to protect 'qCount' with a simple semaphore. If the interrupt
	// 'thread' is enqueueing, we need to wait before decrementing the count.
	while (enqueueBusy)
		;
	dequeueBusy = 1;
	qCount--;
	dequeueBusy = 0;
	return sample;
}
