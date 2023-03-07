/*
 * Absolute Positioning System
 * February 2018
 *
 * Ronaldo Sena
 * ronaldo.sena@outlook.com
 * 
 * Description:
 * Finite State Machine
 */

#ifndef FSM_h
#define FSM_h

typedef enum {waitHandShake,waitHostCommand} State;
typedef struct
{
  State state = waitHandShake;
  bool handshake = false;
}FiniteStateMachine;

// Initial FSM's state, performs handshake with host
void DoHandShake();

// Second and final FSM's state, waits for incoming commands
void DoHostCommand();

// Run the FSM, called every main loop
void RunFSM();

#endif