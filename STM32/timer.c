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

#include <string.h>
#include <math.h>
#include "main.h"
#include "stm32f4xx_tim.h"
#include "evalboard.h"

// Interrupt counter.
volatile uint32_t Irqs;

/**
 * @brief  Configure the input pins that will be used for sampling.
 * @param  none
 * @retval none
 */
static void ConfigInputPins(void) {
	GPIO_InitTypeDef GPIO_InitStructure;

	RCC_AHB1PeriphClockCmd(TIMER_AH_PERIPH, ENABLE);
	GPIO_InitStructure.GPIO_Pin = TIMER_PINS;
	GPIO_InitStructure.GPIO_Mode = GPIO_Mode_IN;
	GPIO_InitStructure.GPIO_Speed = GPIO_Speed_100MHz;
	GPIO_InitStructure.GPIO_OType = GPIO_OType_PP;
	GPIO_InitStructure.GPIO_PuPd = GPIO_PuPd_DOWN;
	GPIO_Init(TIMER_GPIO, &GPIO_InitStructure);
}

/**
 * @brief  Configure the timer for the desired frequency.
 * @param  TimerBaseClockRate: the rate of the timer base clock rate (not
 *         necessarily the system processor clock rate).
 * @param  DesiredFequency: the frequency of interrupts desired.
 * @retval none
 */
static void ConfigTimer(uint32_t TimerBaseClockRate, uint32_t DesiredFequency) {
	TIM_TimeBaseInitTypeDef timerInitStructure;

	RCC_APB1PeriphClockCmd(RCC_APB1Periph_TIM2, ENABLE);
	// From the docs...
	// The timer clock frequencies are automatically fixed by hardware. There are two cases:
	// 1. if the APB prescaler is 1, the timer clock frequencies are set to the same frequency as
	// that of the APB domain to which the timers are connected.
	// 2. otherwise, they are set to twice (*2) the frequency of the APB domain to which the
	// timers are connected.
	//
	timerInitStructure.TIM_Period = TimerBaseClockRate / DesiredFequency;
	timerInitStructure.TIM_Prescaler = 1; // See above
	timerInitStructure.TIM_CounterMode = TIM_CounterMode_Up;
	timerInitStructure.TIM_ClockDivision = TIM_CKD_DIV1;
	timerInitStructure.TIM_RepetitionCounter = 0;
	TIM_TimeBaseInit(TIM2, &timerInitStructure);
	TIM_Cmd(TIM2, ENABLE);
	TIM_ITConfig(TIM2, TIM_IT_Update, ENABLE);

	NVIC_InitTypeDef nvicStructure;
	nvicStructure.NVIC_IRQChannel = TIM2_IRQn;
	nvicStructure.NVIC_IRQChannelPreemptionPriority = 0;
	nvicStructure.NVIC_IRQChannelSubPriority = 1;
	nvicStructure.NVIC_IRQChannelCmd = ENABLE;
	NVIC_Init(&nvicStructure);
}

/**
 * @brief  ISR to latch input samples
 * @param  none
 * @retval none
 */
void TIM2_IRQHandler() {
	if (TIM_GetITStatus(TIM2, TIM_IT_Update ) != RESET) {
		uint8_t sample;

		Irqs++;

		TIM_ClearITPendingBit(TIM2, TIM_IT_Update );

		// Sample all 8 inputs simultaneously. Pins should have been chosen so
		// that they are contiguous in the sample GPIO bank, for speed.
#if TIMER_HI_PINS
		sample = (GPIO_ReadInputData(TIMER_GPIO ) >> 8) & 0xff;
#else
		sample = GPIO_ReadInputData(TIMER_GPIO ) & 0xff;
#endif

		// Send the samples to the output queue.
		EnqueueSample(sample);
	}
}

/**
 * @brief  Initialize the sample timer
 * @param  TimerBaseClockRate: the rate of the timer base clock rate (not
 *         necessarily the system processor clock rate).
 * @param  DesiredFequency: the frequency of interrupts desired.
 * @retval none
 */
void TimerInit(uint32_t TimerBaseClockRate, uint32_t DesiredFequency) {
	Irqs = 0;
	ConfigInputPins();
	ConfigTimer(TimerBaseClockRate, DesiredFequency);
}

/**
 * @brief  De-initialize the sample timer
 * @param  none
 * @retval none
 */
void TimerDenit() {
	NVIC_InitTypeDef nvicStructure;

	RCC_APB1PeriphClockCmd(RCC_APB1Periph_TIM2, DISABLE);

	TIM_Cmd(TIM2, DISABLE);
	TIM_ITConfig(TIM2, TIM_IT_Update, DISABLE);
	nvicStructure.NVIC_IRQChannel = TIM2_IRQn;
	nvicStructure.NVIC_IRQChannelPreemptionPriority = 0;
	nvicStructure.NVIC_IRQChannelSubPriority = 1;
	nvicStructure.NVIC_IRQChannelCmd = DISABLE;
	NVIC_Init(&nvicStructure);
}
