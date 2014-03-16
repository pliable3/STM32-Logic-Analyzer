#ifndef __EVALLEDS_H
#define __EVALLEDS_H

#include "stdint.h"
#include "stm32f4xx.h"

// NOTE: STM generic

// Board-specific LED mappings
#define LED_RED          GPIO_Pin_14
#define LED_GREEN        GPIO_Pin_12
#define LED_ORANGE       GPIO_Pin_13
#define LED_BLUE         GPIO_Pin_15
#define LED_AH_PERIPH    RCC_AHB1Periph_GPIOD
#define LED_GPIO         GPIOD

// Board-specific USART mappings
#define USART_NO         USART2
#define USART_AP_PERIPH  RCC_APB1Periph_USART2
#define USART_AH_PERIPH  RCC_AHB1Periph_GPIOA
#define USART_GPIO_AF    GPIO_AF_USART2
#define USART_GPIO       GPIOA
#define USART_TX_PIN     GPIO_Pin_2
#define USART_RX_PIN     GPIO_Pin_3
#define USART_TX_SOURCE  GPIO_PinSource2
#define USART_RX_SOURCE  GPIO_PinSource3
#define USART_IRQ        USART2_IRQn
#define USART_IRQHANDLER USART2_IRQHandler

// Board-specific Timer mappings
#define TIMER_AH_PERIPH  RCC_AHB1Periph_GPIOE
#define TIMER_GPIO       GPIOE
#define TIMER_PINS       GPIO_Pin_8 | GPIO_Pin_9 | GPIO_Pin_10 | GPIO_Pin_11 | GPIO_Pin_12 | GPIO_Pin_13 | GPIO_Pin_14 | GPIO_Pin_15
#define TIMER_HI_PINS    1

#define LED_MODE_EXCL_ON 0 // Turn on the specified LED ONLY.
#define LED_MODE_ON      1 // Turn on the specified LED and keep others on too.
#define LED_MODE_OFF     2

#ifdef __cplusplus
extern "C" {
#endif

void LedInit(void);
void LedSet(uint16_t led, uint8_t mode);

#ifdef __cplusplus
}
#endif
#endif //__EVALLEDS_H
