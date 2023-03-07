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

#include "FSM.h"
#include "SerialCommands.h"
#include "MotorHandler.c"
#include "PortManipulation.h"

bool ESTADO = false;
const int32_t maxSteps = 100000;

FiniteStateMachine FSM;

// Main routine of the machine
void RunFSM()
{
  switch (FSM.state)
  {
  case waitHandShake:
    DoHandShake();
    break;

  case waitHostCommand:
    DoHostCommand();
    break;

  default:
    break;
  }
}

// DoHandShake sends a string with current firmware version burnt into chip
void DoHandShake()
{
  uint8_t cmd;
  // Host requested firmware version
  if (Serial.available() && !FSM.handshake)
  {
    cmd = Serial.read();
    if (cmd == CMD_FMR_VER)
    {
      Serial.println(FIRMWARE_VERSION);
    }
    else if (cmd == CMD_FMR_VER_OK)
    {
      FSM.handshake = true;
      FSM.state = waitHostCommand;
    }
  }
}

void DoHostCommand()
{
  int32_t NSteps = 0;
  int16_t speed = 0;
  uint8_t cmd;
  uint8_t bufferLength = 6;
  uint8_t incomingBuffer[bufferLength];
  memset(incomingBuffer, 0, sizeof(incomingBuffer));

  if (Serial.available() > 0)
  {
    cmd = (uint8_t)Serial.read();
    // If the incoming command tells me to move some amount of steps, I need to read 5 more bytes
    // Here's what you gonna do: read only one byte, if that byte is to do something that requires some number
    // read more bytes.
    // Maybe worry about timeout in the future, but YAGNI

    switch (cmd)
    {
    case CMD_RESET:
      Serial.readBytes((uint8_t *)incomingBuffer, 1);
      if (incomingBuffer[0] == END_OF_MSG)
      {
        LED_OFF
        FSM.handshake = false;
        FSM.state = waitHandShake;
        Serial.write(CO_RESET);
      }
      break;

    case CMD_SPEED:
      Serial.readBytes((uint8_t *)incomingBuffer, 3);
      if (incomingBuffer[2] == END_OF_MSG)
      {
        for (int i = 0; i < 2; i++)
          speed |= ((int16_t)incomingBuffer[i]) << (8 * i);
        SetTimeInterval(speed);
      }
      break;

    case CMD_LED_ON:
      Serial.readBytes((uint8_t *)incomingBuffer, 1);
      if (incomingBuffer[0] == END_OF_MSG)
      {
        LED_ON
        Serial.write(CO_LED_ON);
      }
      break;

    case CMD_LED_OFF:
      Serial.readBytes((uint8_t *)incomingBuffer, 1);
      if (incomingBuffer[0] == END_OF_MSG)
      {
        LED_OFF
        Serial.write(CO_LED_OFF);
      }
      break;

    // NOTE: for this two commands, I'm kinda improvising
    // The idea is to make the motors go forwards or backwards for as long as the
    // button is being pressed in host program. The motors will stop when the button is released
    // i.e. when the key is down, host will send either CMD_X_FWD or CMD_X_BCK. When the key is released
    // host send CMD_X_STOP
    case CMD_X_FWD:
      Serial.readBytes((uint8_t *)incomingBuffer, 1);
      if (incomingBuffer[0] == END_OF_MSG)
      {
        MoveNSteps(X_MOTOR, maxSteps);
        Serial.write(CO_X_STEPS);
      }
      break;

    case CMD_X_BCK:
      Serial.readBytes((uint8_t *)incomingBuffer, 1);
      if (incomingBuffer[0] == END_OF_MSG)
      {
        MoveNSteps(X_MOTOR, -maxSteps);
        Serial.write(CO_X_STEPS);
      }
      break;

    case CMD_X_STOP:
      Serial.readBytes((uint8_t *)incomingBuffer, 1);
      if (incomingBuffer[0] == END_OF_MSG)
      {
        StopMotor(X_MOTOR);
        Serial.write(CO_X_STOP);
      }
      break;

    case CMD_X_STEPS:
      Serial.readBytes((uint8_t *)incomingBuffer, 5);
      if (incomingBuffer[4] == END_OF_MSG)
      {
        for (int i = 0; i < 4; i++)
          NSteps |= ((int32_t)incomingBuffer[i]) << (8 * i);
        MoveNSteps(X_MOTOR, NSteps);
        Serial.write(CO_X_STEPS);
      }
      break;

    case CMD_X_MOVETO:
      // Ignoring this part for now
      break;

    /****************************************/
    /********* Y-axis related stuff *********/
    /****************************************/
    case CMD_Y_FWD:
      Serial.readBytes((uint8_t *)incomingBuffer, 1);
      if (incomingBuffer[0] == END_OF_MSG)
      {
        MoveNSteps(Y_MOTOR, maxSteps);
        Serial.write(CO_Y_STEPS);
      }
      break;

    case CMD_Y_BCK:
      Serial.readBytes((uint8_t *)incomingBuffer, 1);
      if (incomingBuffer[0] == END_OF_MSG)
      {
        MoveNSteps(Y_MOTOR, -maxSteps);
        Serial.write(CO_Y_STEPS);
      }
      break;

    case CMD_Y_STOP:
      Serial.readBytes((uint8_t *)incomingBuffer, 1);
      if (incomingBuffer[0] == END_OF_MSG)
      {
        StopMotor(Y_MOTOR);
        Serial.write(CO_Y_STOP);
      }
      break;

    case CMD_Y_STEPS:
      Serial.readBytes((uint8_t *)incomingBuffer, 5);
      if (incomingBuffer[4] == END_OF_MSG)
      {
        for (int i = 0; i < 4; i++)
          NSteps |= ((int32_t)incomingBuffer[i]) << (8 * i);
        MoveNSteps(Y_MOTOR, NSteps);
        Serial.write(CO_Y_STEPS);
      }
      break;

    case CMD_Y_MOVETO:
      // Ignoring this part for now
      break;

    /****************************************/
    /********* Z-axis related stuff *********/
    /****************************************/
    case CMD_Z_FWD:
      Serial.readBytes((uint8_t *)incomingBuffer, 1);
      if (incomingBuffer[0] == END_OF_MSG)
      {
        MoveNSteps(Z_MOTOR, maxSteps);
        Serial.write(CO_Z_STEPS);
      }
      break;

    case CMD_Z_BCK:
      Serial.readBytes((uint8_t *)incomingBuffer, 1);
      if (incomingBuffer[0] == END_OF_MSG)
      {
        MoveNSteps(Z_MOTOR, -maxSteps);
        Serial.write(CO_Z_STEPS);
      }
      break;

    case CMD_Z_STOP:
      Serial.readBytes((uint8_t *)incomingBuffer, 1);
      if (incomingBuffer[0] == END_OF_MSG)
      {
        StopMotor(Z_MOTOR);
        Serial.write(CO_Z_STOP);
      }
      break;

    case CMD_Z_STEPS:
      Serial.readBytes((uint8_t *)incomingBuffer, 5);
      if (incomingBuffer[4] == END_OF_MSG)
      {
        for (int i = 0; i < 4; i++)
          NSteps |= ((int32_t)incomingBuffer[i]) << (8 * i);
        MoveNSteps(Z_MOTOR, NSteps);
        Serial.write(CO_Z_STEPS);
      }
      break;

    case CMD_Z_MOVETO:
      // Ignoring this part for now
      break;

    /****************************************/
    /********* P-axis related stuff *********/
    /****************************************/
    case CMD_P_FWD:
      Serial.readBytes((uint8_t *)incomingBuffer, 1);
      if (incomingBuffer[0] == END_OF_MSG)
      {
        MoveNSteps(P_MOTOR, maxSteps);
        Serial.write(CO_P_FWD);
      }
      break;

    case CMD_P_BCK:
      Serial.readBytes((uint8_t *)incomingBuffer, 1);
      if (incomingBuffer[0] == END_OF_MSG)
      {
        MoveNSteps(P_MOTOR, -maxSteps);
        Serial.write(CO_P_BCK);
      }
      break;

    case CMD_P_STOP:
      Serial.readBytes((uint8_t *)incomingBuffer, 1);
      if (incomingBuffer[0] == END_OF_MSG)
      {
        StopMotor(P_MOTOR);
        Serial.write(CO_P_STOP);
      }
      break;

    case CMD_P_STEPS:
      Serial.readBytes((uint8_t *)incomingBuffer, 5);
      if (incomingBuffer[4] == END_OF_MSG)
      {
        for (int i = 0; i < 4; i++)
          NSteps |= ((int32_t)incomingBuffer[i]) << (8 * i);
        MoveNSteps(P_MOTOR, NSteps);
        Serial.write(CO_P_STEPS);
      }
      break;

    case CMD_P_MOVETO:
      // Ignoring this part for now
      break;

    /****************************************/
    /********* T-axis related stuff *********/
    /****************************************/
    case CMD_T_FWD:
      Serial.readBytes((uint8_t *)incomingBuffer, 1);
      if (incomingBuffer[0] == END_OF_MSG)
      {
        MoveNSteps(T_MOTOR, maxSteps);
        Serial.write(CO_T_FWD);
      }
      break;

    case CMD_T_BCK:
      Serial.readBytes((uint8_t *)incomingBuffer, 1);
      if (incomingBuffer[0] == END_OF_MSG)
      {
        MoveNSteps(T_MOTOR, -maxSteps);
        Serial.write(CO_T_BCK);
      }
      break;

    case CMD_T_STOP:
      Serial.readBytes((uint8_t *)incomingBuffer, 1);
      if (incomingBuffer[0] == END_OF_MSG)
      {
        StopMotor(T_MOTOR);
        Serial.write(CO_T_STOP);
      }
      break;

    case CMD_T_STEPS:
      Serial.readBytes((uint8_t *)incomingBuffer, 5);
      if (incomingBuffer[4] == END_OF_MSG)
      {
        for (int i = 0; i < 4; i++)
          NSteps |= ((int32_t)incomingBuffer[i]) << (8 * i);
        MoveNSteps(T_MOTOR, NSteps);
        Serial.write(CO_T_STEPS);
      }
      break;

    case CMD_T_MOVETO:
      // Ignoring this part for now
      break;

    case CMD_CIRCLE_ON:
      Drill(true);
      Serial.write(CO_CIRCLE_ON);
      break;

    case CMD_CIRCLE_OFF:
      Drill(false);
      Serial.write(CO_CIRCLE_OFF);
      break;

    /****************************************/
    /********* Spindle related stuff ********/
    /****************************************/
    case CMD_SPINDLE_ON:
      Serial.readBytes((uint8_t *)incomingBuffer, 1);
      if (incomingBuffer[0] == END_OF_MSG)
      {
        SPINDLE_ON;
        Serial.write(CO_SPINDLE_ON);
      }
      break;

    case CMD_SPINDLE_OFF:
      Serial.readBytes((uint8_t *)incomingBuffer, 1);
      if (incomingBuffer[0] == END_OF_MSG)
      {
        SPINDLE_OFF;
        Serial.write(CO_SPINDLE_OFF);
      }
      break;

    default:
      // Stay where you are
      break;
    }
  }
}