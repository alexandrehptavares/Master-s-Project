/*
 * Absolute Positioning System
 * January 2018
 *
 * Ronaldo Sena
 * ronaldo.sena@outlook.com
 * 
 * Description:
 * Pins used in microcontroller
 */


#ifndef PortManipulation_h
#define PortManipulation_h


// Pin-out
#define PIN_INDICATOR_LED  PB7 // 13 (OC0A/OC1C/PCINT7)
// Position switches must be on interruptible pins. On ATMega 2560 there are several interrupts pins, some shared
// with other functionalities, like SPI, UART pins. In the arduino mega board the pins available are:
// [2 (PE4), 3(PE5), 18(PD3), 19(PD2), 20(PD1), 21(PD0)]
// since there are 5 motors (at least three of them need two end-course switches) the program needs to track,
// one possible solution will be to use a OR-chip. If either one goes high, then the output will trigger the interrupt. 
// To check end/begin, just see the direction
#define PIN_X_SWITCH       PD0 // 21 
#define PORT_PIN_X_SWITCH  PORTD
#define DDR_PIN_X_SWITCH   DDRD

#define PIN_Y_SWITCH       PE4 // 2
#define PORT_PIN_Y_SWITCH  PORTE
#define DDR_PIN_Y_SWITCH   DDRE

#define PIN_Z_SWITCH       PD1 // 20 
#define PORT_PIN_Z_SWITCH  PORTD
#define DDR_PIN_Z_SWITCH   DDRD

// The Interruption Routines are defined in the MotorHandler.c file
#define INT_PIN_X_SWITCH  INT0
#define INT_PIN_Y_SWITCH  INT1
#define INT_PIN_Z_SWITCH  INT4


// For all @_AXIS bellow, it is as follows:
// @_ENA -> This normally low input signal will disable all outputs when pulLED high.
// @_DIR -> The level if this signal is sampLED on each rising edge of STEP to determine which direction to take the step.
// @_SET -> Each rising edge of this signal will cause one step (or microstep) to be taken.
// Following pin number on Arduino board, is the respective ATMega2560 pin register
// X axis
#define PORT_PIN_X  PORTC
#define DDR_PIN_X   DDRC
#define PIN_X_ENA   PC2 // 35
#define PIN_X_DIR   PC4 // 33
#define PIN_X_SETP  PC6 // 31

// Y axis
#define PORT_PIN_Y  PORTC
#define DDR_PIN_Y   DDRC
#define PIN_Y_ENA   PC3 // 34
#define PIN_Y_DIR   PC5 // 32
#define PIN_Y_SETP  PC7 // 30

// Z axis
#define PORT_PIN_Z  PORTA
#define DDR_PIN_Z   DDRA
#define PIN_Z_ENA   PA0 // 22
#define PIN_Z_DIR   PA2 // 24
#define PIN_Z_SETP  PA4 // 26

// PHI angle
#define PORT_PIN_P  PORTA
#define DDR_PIN_P   DDRA
#define PIN_P_ENA   PA1 // 23
#define PIN_P_DIR   PA3 // 25
#define PIN_P_SETP  PA5 // 27

// THETA angle
#define PORT_PIN_T  PORTL
#define DDR_PIN_T   DDRL
#define PIN_T_ENA   PL6 // 43
#define PIN_T_DIR   PL4 // 45
#define PIN_T_SETP  PL2 // 47

// Disclaimer: this will only work for TB6XXX chips
#define PIN_SPINDLE       PL0 // 49


/*  PORT MANIPULATIONS  */

// Output compare interrupt enable/disable bit 
#define TIMER3_INTERRUPTS_ON    TIMSK3 |=  (1 << OCIE3A);
#define TIMER3_INTERRUPTS_OFF   TIMSK3 &= ~(1 << OCIE3A);

#define LED_ON         PORTB |= (1<<PIN_INDICATOR_LED);
#define LED_OFF        PORTB &=~(1<<PIN_INDICATOR_LED);

#define X_TOGGLE       PORT_PIN_X ^= (1<<PIN_X_SETP);
#define X_HIGH         PORT_PIN_X |= (1<<PIN_X_SETP);
#define X_LOW          PORT_PIN_X &=~(1<<PIN_X_SETP);
#define X_DIR_FORWARD  PORT_PIN_X |= (1<<PIN_X_DIR);
#define X_DIR_BACKWARD PORT_PIN_X &=~(1<<PIN_X_DIR);
#define X_ENA_ON       PORT_PIN_X |= (1<<PIN_X_ENA);
#define X_ENA_OFF      PORT_PIN_X &=~(1<<PIN_X_ENA);

#define Y_TOGGLE       PORT_PIN_Y ^= (1<<PIN_Y_SETP);
#define Y_HIGH         PORT_PIN_Y |= (1<<PIN_Y_SETP);
#define Y_LOW          PORT_PIN_Y &=~(1<<PIN_Y_SETP);
#define Y_DIR_FORWARD  PORT_PIN_Y |= (1<<PIN_Y_DIR);
#define Y_DIR_BACKWARD PORT_PIN_Y &=~(1<<PIN_Y_DIR);
#define Y_ENA_ON       PORT_PIN_Y |= (1<<PIN_Y_ENA);
#define Y_ENA_OFF      PORT_PIN_Y &=~(1<<PIN_Y_ENA);

#define Z_TOGGLE       PORT_PIN_Z ^= (1<<PIN_Z_SETP);
#define Z_HIGH         PORT_PIN_Z |= (1<<PIN_Z_SETP);
#define Z_LOW          PORT_PIN_Z &=~(1<<PIN_Z_SETP);
#define Z_DIR_FORWARD  PORT_PIN_Z |= (1<<PIN_Z_DIR);
#define Z_DIR_BACKWARD PORT_PIN_Z &=~(1<<PIN_Z_DIR);
#define Z_ENA_ON       PORT_PIN_Z |= (1<<PIN_Z_ENA);
#define Z_ENA_OFF      PORT_PIN_Z &=~(1<<PIN_Z_ENA);

#define P_TOGGLE       PORT_PIN_P ^= (1<<PIN_P_SETP);
#define P_HIGH         PORT_PIN_P |= (1<<PIN_P_SETP);
#define P_LOW          PORT_PIN_P &=~(1<<PIN_P_SETP);
#define P_DIR_FORWARD  PORT_PIN_P |= (1<<PIN_P_DIR);
#define P_DIR_BACKWARD PORT_PIN_P &=~(1<<PIN_P_DIR);
#define P_ENA_ON       PORT_PIN_P |= (1<<PIN_P_ENA);
#define P_ENA_OFF      PORT_PIN_P &=~(1<<PIN_P_ENA);

// THETA port for step is port L. Watch out!
#define T_TOGGLE       PORT_PIN_T ^= (1<<PIN_T_SETP);
#define T_HIGH         PORT_PIN_T |= (1<<PIN_T_SETP);
#define T_LOW          PORT_PIN_T &=~(1<<PIN_T_SETP);
#define T_DIR_FORWARD  PORT_PIN_T |= (1<<PIN_T_DIR);
#define T_DIR_BACKWARD PORT_PIN_T &=~(1<<PIN_T_DIR);
#define T_ENA_ON       PORT_PIN_T |= (1<<PIN_T_ENA);
#define T_ENA_OFF      PORT_PIN_T &=~(1<<PIN_T_ENA);

#define SPINDLE_ON     PORTL |= (1<<PIN_SPINDLE);
#define SPINDLE_OFF    PORTL &=~(1<<PIN_SPINDLE);
#define SPINDLE_TOGGLE PORTL ^= (1<<PIN_SPINDLE);


void SetPins();
void SetInterrupts();
void SetTimeInterval(long interval);

#endif