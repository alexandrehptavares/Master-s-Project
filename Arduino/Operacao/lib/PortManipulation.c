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


//#include "Arduino.h"
#include "PortManipulation.h"

void SetPins()
{
  // Not so pretty, but effective when doing firmware maintenance
  DDRB |= 1 << PIN_INDICATOR_LED;

  DDR_PIN_X |= 1 << PIN_X_ENA;
  DDR_PIN_X |= 1 << PIN_X_DIR;
  DDR_PIN_X |= 1 << PIN_X_SETP;
  PORT_PIN_X &=~(1<<PIN_X_ENA);

  DDR_PIN_Y |= 1 << PIN_Y_ENA;
  DDR_PIN_Y |= 1 << PIN_Y_DIR;
  DDR_PIN_Y |= 1 << PIN_Y_SETP;
  PORT_PIN_Y &=~(1<<PIN_Y_ENA);

  DDR_PIN_Z |= 1 << PIN_Z_ENA;
  DDR_PIN_Z |= 1 << PIN_Z_DIR;
  DDR_PIN_Z |= 1 << PIN_Z_SETP;
  PORT_PIN_Z &=~(1<<PIN_Z_ENA);

  DDR_PIN_P |= 1 << PIN_P_ENA;
  DDR_PIN_P |= 1 << PIN_P_DIR;
  DDR_PIN_P |= 1 << PIN_P_SETP;
  PORT_PIN_P &=~(1<<PIN_P_ENA);

  DDR_PIN_T |= 1 << PIN_T_ENA;
  DDR_PIN_T |= 1 << PIN_T_DIR;
  DDR_PIN_T |= 1 << PIN_T_SETP;
  PORT_PIN_T &=~(1<<PIN_T_ENA);

  // SWITCHERS
  DDR_PIN_X_SWITCH &= ~1<<PIN_X_SWITCH;
  PORT_PIN_X_SWITCH |= 1<<PIN_X_SWITCH;
  DDR_PIN_Y_SWITCH &= ~1<<PIN_Y_SWITCH;
  PORT_PIN_Y_SWITCH |= 1<<PIN_Y_SWITCH;
  DDR_PIN_Z_SWITCH &= ~1<<PIN_Z_SWITCH;
  PORT_PIN_Z_SWITCH |= 1<<PIN_Z_SWITCH;

  // Spindle
  DDRL |= 1 << PIN_SPINDLE;
  SPINDLE_OFF;
}

void SetInterrupts()
{
  noInterrupts();

  /* JUST TEST, AND THE EXTERNAL INTERRUPTS NEED A DEBOUNCING CIRCUIT */
  // EXTERNAL INTERRUPT
  // INT0 -> X axis
  EICRA |= (1 << ISC00); // Trigger on any edge
  EIMSK |= (1 << INT_PIN_X_SWITCH); // enable interrupts
  // // INT1 -> Y axis
  // EICRA |= (1 << ISC00); // Trigger on any edge
  // EIMSK |= (1 << INT_PIN_Y_SWITCH); // enable interrupts
  // // INT2 -> Z axis
  // EICRA |= (1 << ISC00); // Trigger on any edge
  // EIMSK |= (1 << INT_PIN_Z_SWITCH); // enable interrupts

  // INTERNAL TIMER INTERRUPT
  // TCCR3X – Timer/Counter 3 Control Register X
  TCCR3A = 0; // Reset do default values
  TCCR3B = 0; // Reset do default values
  TCNT3  = 0; // Set counter to 0x00
  OCR3A = 10; // Set comparer to 1000 counter ticks
  // Code never lies, comments sometimes do
  TCCR3B |= (1 << WGM32); //  Clear Timer on Compare
  TCCR3B |= ((1 << CS31) | (1 << CS30)); // Set prescaler to 64
  // Since clock is 16MHz, a clock pulse is about 1/16M (62.5 nanoseconds)
  // The prescaler is set to 64, therefore the counter will increment every 62.5*64 nanoseconds.
  // That gives a count every 4 microseconds. As the counter compare (OCR3A) is set to 1000,
  // the ISR will trigger every 4 ms (4 us * 1000).
  // To change trigger time, change the value on OCR3A (16 bit) register

  interrupts();
}

// Set time interval between interrupts in microseconds
void SetTimeInterval(long interval)
{
  // Assuming a prescaler of 64, 16MHzclock frequency 
  int increment =  1.0 * interval;
  OCR3A = increment;
}