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
#include "main.h"
#include "evalboard.h"

/**
 * @brief  Initialize the USART serial I/O
 * @param  none
 * @retval none
 */
void UsartInit(void) {
	GPIO_InitTypeDef GPIO_InitStructure;
	USART_InitTypeDef USART_InitStructure;

	RCC_APB1PeriphClockCmd(USART_AP_PERIPH, ENABLE);

	/* GPIOx clock enable */
	RCC_AHB1PeriphClockCmd(USART_AH_PERIPH, ENABLE);

	/* GPIOx Configuration -- TX */
	GPIO_InitStructure.GPIO_Pin = USART_TX_PIN;
	GPIO_InitStructure.GPIO_Mode = GPIO_Mode_AF;
	GPIO_InitStructure.GPIO_Speed = GPIO_Speed_50MHz;
	GPIO_InitStructure.GPIO_OType = GPIO_OType_PP;
	GPIO_InitStructure.GPIO_PuPd = GPIO_PuPd_UP;
	GPIO_Init(USART_GPIO, &GPIO_InitStructure);

	// IUSART input -- RX
	GPIO_InitStructure.GPIO_Pin = USART_RX_PIN;
	GPIO_InitStructure.GPIO_Mode = GPIO_Mode_AF;
	GPIO_InitStructure.GPIO_Speed = GPIO_Speed_50MHz;
	GPIO_Init(USART_GPIO, &GPIO_InitStructure);

	/* Connect USART pins */
	GPIO_PinAFConfig(USART_GPIO, USART_TX_SOURCE, USART_GPIO_AF );
	GPIO_PinAFConfig(USART_GPIO, USART_RX_SOURCE, USART_GPIO_AF );

	// Receiver interrupt config.
	NVIC_InitTypeDef NVIC_InitStructure;

	/* Enable the USARTz Interrupt */
	NVIC_InitStructure.NVIC_IRQChannel = USART_IRQ;
	NVIC_InitStructure.NVIC_IRQChannelPreemptionPriority = 0;
	NVIC_InitStructure.NVIC_IRQChannelSubPriority = 0;
	NVIC_InitStructure.NVIC_IRQChannelCmd = ENABLE;
	NVIC_Init(&NVIC_InitStructure);

	// The baud rate of 921600 was chosen experimentally as a rate that can
	// transmit without errors. 1036800 did not work.
	// At this rate, I can get approx. 89000 samples per second to the computer.
	USART_InitStructure.USART_BaudRate = 921600; //460800; // 115200;
	USART_InitStructure.USART_WordLength = USART_WordLength_8b;
	USART_InitStructure.USART_StopBits = USART_StopBits_1;
	USART_InitStructure.USART_Parity = USART_Parity_No;
	USART_InitStructure.USART_HardwareFlowControl = USART_HardwareFlowControl_None;

	USART_InitStructure.USART_Mode = USART_Mode_Tx | USART_Mode_Rx;

	USART_Init(USART_NO, &USART_InitStructure);

	// Interrupt config.
	USART_ITConfig(USART_NO, USART_IT_RXNE, ENABLE);

	USART_Cmd(USART_NO, ENABLE); // enable USARTx
}

/**
 * @brief  Send a byte to the USART
 * @param  c: the byte to send
 * @retval none
 */
void UsartSendChar(char c) {
	USART_SendData(USART_NO, c);

	// Wait for the character to be sent.
	while ((USART_NO ->SR & (1 << 7)) == 0)
		;
}

/**
 * @brief  Send a string of bytes to the USART
 * @param  s: the string to send
 * @retval none
 */
void UsartSendString(char *s) {
	while (*s)
		UsartSendChar(*(s++));
}

// Queue to hold characters as they come in. Note that no attention is
// paid to thread-safety or queue overflows here. There should be very
// little data and it is not considered critical.
#define MAXQ 256

volatile char inbuf[MAXQ];
volatile uint8_t inHead = 0;
volatile uint8_t inTail = 0;

/**
 * @brief  ISR to handle incoming bytes from the USART
 * @param  none
 * @retval none
 */
void USART_IRQHANDLER() {
	if (USART_GetITStatus(USART_NO, USART_IT_RXNE ) != RESET) {
		// Add incoming bytes to the queue.
		inbuf[inHead++] = USART_ReceiveData(USART_NO );
		if (inHead == MAXQ)
			inHead = 0;
		USART_ClearITPendingBit(USART_NO, USART_IT_RXNE );
	}
}

/**
 * @brief  Get a character from the input queue, if available.
 * @param  none
 * @retval the next character from the queue, or zero if none available.
 */
char UsartGetchar() {
	char c;

	if (inHead == inTail)
		return 0;
	c = inbuf[inTail++];
	if (inTail == MAXQ)
		inTail = 0;
	return c;
}

static char getsBuf[200];

/**
 * @brief  Get a string of characters from the input queue, if available.
 * @note   A string is only returned if it is terminated by a linefeed (\n)
 *         character.
 * @param  none
 * @retval a pointer to a string of received data, or NULL if none available.
 */
char *UsartGets() {
	int i;
	char c, found = 0;

	// Make sure that there is a line-feed in the input buffer before reading characters...
	for (i = inTail; i != inHead; i++) {
		if (inbuf[i] == '\n') {
			found = 1;
			break;
		}
	}

	if (!found)
		return NULL ;

	i = 0;
	while ((c = UsartGetchar()) != '\n') {
		if (c != '\r')
			getsBuf[i++] = c;
	}
	getsBuf[i] = '\0';
	return getsBuf;
}
