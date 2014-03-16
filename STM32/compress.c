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
#include "compress.h"
#include "main.h"

#define USE_MALLOC 0 // set to 1 if using malloc()
#define MINBITS 9
#define MAXBITS 15
#define CLEAR_CODE 256
#define FIRST_CODE 257

#define NBITS 13

#define MAX_CODE ((1 << NBITS) - 1)

static uint16_t mask[] = { 0x00, 0x01, 0x03, 0x07, 0x0f, 0x1f, 0x3f, 0x7f, 0xff, 0x1ff, 0x3ff, 0x7ff, 0xfff, 0x1fff,
		0x3fff, 0x7fff, 0xffff };
static uint16_t primes[] = { 601, 1501, 2801, 5003, 9001, 18013, 35023 };

static uint16_t hashSize;
static int16_t shift;
static uint16_t freeEntry;

#if USE_MALLOC
static int32_t *hashTable; // 32-bit hash table.
static uint16_t *codeTable;// 16-bit code table.
#else
// If no malloc, just allocate directly. Hard-coded for 13-bits.
int32_t hashTable[9001]; // 32-bit hash table.
uint16_t codeTable[9001]; // 16-bit code table.
#endif

static uint32_t crc;
static uint16_t ent;
static uint8_t firstByte;

static uint16_t outBits;
static uint8_t outByte;

void (*CompressCallback)(uint8_t OutChar);

static void clearHash(void);
static void sendOutputCode(uint16_t);

/**
 * @brief  Initialize compression
 * @param  Callback: a pointer to a function that will receive output bytes
 *         from the compression process
 * @retval 0 is successful, otherwise -1
 */
int CompressInit(void (*Callback)(uint8_t)) {
	CompressCallback = Callback;

	hashSize = primes[NBITS - MINBITS];

#if USE_MALLOC
	hashTable = calloc(hashSize, sizeof(int32_t));
	if (hashTable == NULL )
	return -1;
	codeTable = calloc(hashSize, sizeof(uint16_t));
	if (codeTable == NULL )
	return -1;
#endif

	int16_t j = 0;
	int32_t fc;
	for (fc = hashSize; fc < 65536L; fc *= 2)
		j++;
	shift = (int16_t) (8 - j);

	clearHash();

	outBits = 0;
	outByte = 0;
	ent = 0;
	crc = 0;
	firstByte = 1;
	return 0;
}

/**
 * @brief  De-initialize compression
 * @param  none
 * @retval none
 */
void CompressDenit() {
#if USE_MALOC
	if (codeTable != NULL ) {
		free(codeTable);
		codeTable = NULL;
	}
	if (hashTable != NULL ) {
		free(hashTable);
		hashTable = NULL;
	}
#endif
	CompressCallback = NULL;
}

/**
 * @brief  Clear the hash and code tables
 * @param  none
 * @retval none
 */
static void clearHash() {
	// memset() is faster.
	memset(hashTable, 0xff, hashSize * sizeof(int32_t));
	memset(codeTable, 0x00, hashSize * sizeof(int16_t));
//	int i;
//	for (i = 0; i < hashSize; i++)
//		hashTable[i] = -1;

	freeEntry = FIRST_CODE;
}

/**
 * @brief  Compress a string
 * @param  str: a pointer to a string to compress
 * @retval None
 */
void CompressString(char *str) {
	uint8_t b;

	while ((b = *str++)) {
		CompressByte(b);
	}
}

/**
 * @brief  Add a byte to the compression stream
 * @param  b: a byte to compress
 * @retval none
 */
void CompressByte(uint8_t b) {
	crc += b;

	if (firstByte) {
		firstByte = 0;
		ent = b;
	} else {
		int16_t hashIndex = ((b << shift) ^ ent); // WAS32
		int32_t hashCode = (ent << 16 | b); // 32-bit hash code.

		if (hashTable[hashIndex] == hashCode) {
			// Found the current code in the hash table, save the result and return.
			ent = codeTable[hashIndex];
			return;
		} else if (hashTable[hashIndex] >= 0) {
			// A code was found in the hash table, but it is not the one we were looking for.
			// Probe until the code is found.
			int16_t hashDisplacement = (hashIndex == 0 ? 1 : hashSize - hashIndex); // WAS32

			if (hashDisplacement < 0)
				hashDisplacement = -hashDisplacement;

			while (1) {
				hashIndex -= hashDisplacement;

				if (hashIndex < 0)
					hashIndex += hashSize;

				if (hashTable[hashIndex] == hashCode) {
					ent = codeTable[hashIndex];
					return;
				}

				if (hashTable[hashIndex] <= 0)
					break;
			}
		}

		sendOutputCode(ent);

		ent = b;
		if (freeEntry < MAX_CODE) {
			codeTable[hashIndex] = freeEntry++;
			hashTable[hashIndex] = hashCode;
		} else {
			sendOutputCode(CLEAR_CODE);
			clearHash();
		}
	}
}

/**
 * @brief  Check if 'outBits' is full. If it is, send
 *         the byte to the output.
 * @param  none
 * @retval none
 */
static void checkIfOutbitsFull() {
	if (outBits == 8) {
		CompressCallback(outByte);
		outBits = 0;
		outByte = 0;
	}
}

/**
 * @brief  Send an output code. This may result in one or more bytes being sent
 *         to the output. Left-over bits are retained for the next call to this
 *         function.
 * @param  ch: 16-bit output code
 * @retval none
 */
static void sendOutputCode(uint16_t ch) {
	int16_t b = NBITS;

	while (1) {
		if (b <= (8 - outBits)) {
			outByte = (uint8_t) ((outByte << b) | ch);
			outBits += (uint16_t) b;
			checkIfOutbitsFull();
			break;
		} else {
			outByte = (uint8_t) (outByte << (8 - outBits));
			b -= (uint8_t) ((8 - outBits));
			outByte |= (uint8_t) (ch >> b);

			/* Mask off the used bits. */
			ch &= mask[b];

			outBits = 8;
			checkIfOutbitsFull();
		}
	}
}

/**
 * @brief  Flushes any remaining data and adds a terminator.
 * @param  none
 * @retval none
 */
void CompressFlush() {
	sendOutputCode(ent);
	sendOutputCode((uint16_t) (0xffff & mask[NBITS]));
}
