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

#include "stm32f4xx.h"
#include "EvalBoard.h"

/**
 * @brief  Initialize LEDS.
 * @param  none
 * @retval none
 */
void LedInit() {
	GPIO_InitTypeDef GPIO_InitStructure;

	// NOTE: STM generic

	/* GPIOD Periph clock enable */
	RCC_AHB1PeriphClockCmd(LED_AH_PERIPH, ENABLE);
	GPIO_InitStructure.GPIO_Pin = LED_RED | LED_GREEN | LED_ORANGE | LED_BLUE;
	GPIO_InitStructure.GPIO_OType = GPIO_OType_PP;
	GPIO_InitStructure.GPIO_Speed = GPIO_Speed_25MHz;
	GPIO_InitStructure.GPIO_PuPd = GPIO_PuPd_NOPULL;

	/* standard output pin */
	GPIO_InitStructure.GPIO_Mode = GPIO_Mode_OUT;
	GPIO_Init(LED_GPIO, &GPIO_InitStructure);
	GPIO_Write(LED_GPIO, 0); //initial state (all LEDs OFF)
}

/**
 * @brief  Set/Reset an LED.
 * @param  led: the LED to set/reset
 * @param  mode: the mode (one of LED_MODE_ON, LED_MODE_OFF, LED_MODE_EXCL_ON)
 * @retval none
 */
void LedSet(uint16_t led, uint8_t mode) {
	switch (mode) {
	case LED_MODE_ON:
		GPIO_SetBits(LED_GPIO, led);
		break;
	case LED_MODE_OFF:
		GPIO_ResetBits(LED_GPIO, led);
		break;
	case LED_MODE_EXCL_ON:
		GPIO_ResetBits(LED_GPIO, LED_GREEN | LED_ORANGE | LED_BLUE | LED_RED );
		GPIO_SetBits(LED_GPIO, led);
		break;
	}
}

/**
 * @brief  Turn off all LEDs.
 * @param  none
 * @retval none
 */
void LedNone() {
	GPIO_ResetBits(LED_GPIO, LED_RED | LED_GREEN | LED_ORANGE | LED_BLUE );
}
