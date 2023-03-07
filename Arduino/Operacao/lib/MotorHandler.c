/*
 * Absolute Positioning System
 * January 2018
 *
 * Ronaldo Sena
 * ronaldo.sena@outlook.com
 * 
 * Description:
 *
 * TODO: Optimize flash memory use, i.e., control variable size (might use stdint.h)
 * TODO: Prevent step lost by making the step process an atomic operation (might use util/atomic.h)
 */  

#include "MotorHandler.h"
#include "PortManipulation.h"

// TODO: make a pointer to the struct and make functions to control each individual motor
volatile MotorStruct motorArray[NUM_STEPPERS];
volatile bool drillMode = false;
// Variables to control drilling process
volatile bool calculateCoordinates = true;
volatile bool setLow = false;
const float PI2 = 6.28318; // 2 * pi
float angleStep = PI2/1600.0; // 2*pi divide by stepsPerRevolution
float angle = 0.0;
const int radius = 100; // in steps
int x = 100; // must be the same as radius
int y = 0;
int xx = 0;
int yy = 0;

void SetStepperMotors()
{
  for (int i = 0; i < NUM_STEPPERS; i++)
  {
    ResetStepper(i);
    motorArray[i].DirectionFunction = SetDirection;
    motorArray[i].StepFunction = TakeStep;
  }
}

void ResetStepper(int whichMotor)
{
  motorArray[whichMotor].currentPosition = 0;
  motorArray[whichMotor].direction = forward;
  motorArray[whichMotor].stepsRequested = 0;
  motorArray[whichMotor].stepsTaken = 0;
  motorArray[whichMotor].movementDone = false;
  motorArray[whichMotor].shouldRun = false;
  motorArray[whichMotor].stepsPerRevolution = 1600;
}

void PrepareMovement(int whichMotor, bool dir)
{
  motorArray[whichMotor].DirectionFunction(whichMotor, dir);
  motorArray[whichMotor].direction = dir;
  motorArray[whichMotor].stepsRequested = 0;
  motorArray[whichMotor].stepsTaken = 0;
  motorArray[whichMotor].movementDone = false;
  motorArray[whichMotor].shouldRun = false;
}

void StopMotor(int whichMotor)
{
  motorArray[whichMotor].movementDone = true;
  motorArray[whichMotor].shouldRun = false;
  motorArray[whichMotor].currentPosition %= motorArray[whichMotor].stepsPerRevolution;
}

void MoveNSteps(int whichMotor, int32_t N)
{
  if (N >= 0)
    SetDirection(whichMotor, forward);
  else
    SetDirection(whichMotor, backward);

  motorArray[whichMotor].stepsRequested = abs(N);
  motorArray[whichMotor].stepsTaken = 0;
  motorArray[whichMotor].movementDone = false;
  motorArray[whichMotor].shouldRun = true;
}

int MoveToAbsolutePositon(int whichMotor, long thisPosition)
{
  #ifdef DEBUGGING 
    Debug("Going to")
    Debug(thisPosition)
    Debug("Current direction")
    Debug(motorArray[whichMotor].direction)
  #endif
  
  long steps2take = thisPosition - motorArray[whichMotor].currentPosition;
  MoveNSteps(whichMotor, steps2take);
  return 0;
}

// TODO: make case for the rest of the motors
void SetDirection(int whichMotor, bool dir)
{
  switch (whichMotor)
  {
    case X_MOTOR:      
      if (dir == forward)
      {
        motorArray[whichMotor].direction = forward;
        X_DIR_FORWARD
        #ifdef DEBUGGING
          Debug("FORWARD")
        #endif 
      }
      else
      {
        motorArray[whichMotor].direction = backward;
        X_DIR_BACKWARD
        #ifdef DEBUGGING
          Debug("BACKWARD")
        #endif 
      }
    break;
    
    case Y_MOTOR:      
      if (dir == forward)
      {
        motorArray[whichMotor].direction = forward;
        Y_DIR_FORWARD
        #ifdef DEBUGGING
          Debug("FORWARD")
        #endif 
      }
      else
      {
        motorArray[whichMotor].direction = backward;
        Y_DIR_BACKWARD
        #ifdef DEBUGGING
          Debug("BACKWARD")
        #endif 
      }
    break;

    case Z_MOTOR:      
      if (dir == forward)
      {
        motorArray[whichMotor].direction = forward;
        Z_DIR_FORWARD
        #ifdef DEBUGGING
          Debug("FORWARD")
        #endif 
      }
      else
      {
        motorArray[whichMotor].direction = backward;
        Z_DIR_BACKWARD
        #ifdef DEBUGGING
          Debug("BACKWARD")
        #endif 
      }
    break;

    case T_MOTOR:      
      if (dir == forward)
      {
        motorArray[whichMotor].direction = forward;
        T_DIR_FORWARD
        #ifdef DEBUGGING
          Debug("T_FORWARD")
        #endif 
      }
      else
      {
        motorArray[whichMotor].direction = backward;
        T_DIR_BACKWARD
        #ifdef DEBUGGING
          Debug("T_BACKWARD")
        #endif 
      }
    break;

    case P_MOTOR:      
      if (dir == forward)
      {
        motorArray[whichMotor].direction = forward;
        P_DIR_FORWARD
        #ifdef DEBUGGING
          Debug("P_FORWARD")
        #endif 
      }
      else
      {
        motorArray[whichMotor].direction = backward;
        P_DIR_BACKWARD
        #ifdef DEBUGGING
          Debug("P_BACKWARD")
        #endif 
      }
    break;
  }
}

// TODO: make case for the rest of the motors
void TakeStep(int whichMotor)
{ 
  switch (whichMotor)
  {
    case X_MOTOR:
      X_HIGH  
    break;

    case Y_MOTOR:
      Y_HIGH
    break;

    case Z_MOTOR:
      Z_HIGH
    break;

    case T_MOTOR:
      T_HIGH
    break;

    case P_MOTOR:
      P_HIGH
    break;
  }
}

void Drill(bool mode)
{
  drillMode = mode;
}

// Mainly for debugging purposes  
void TellMeCurrentPosition()
{
  for (int i = 0; i < NUM_STEPPERS; i++)
  {
    #ifdef DEBUGGING
      Debug(i)
      Debug(motorArray[i].currentPosition)
    #endif
  } 
}

// The real hero! All brute force work happens here
// There is a small setback. The TB6600 drivers vary slightly, some of them have a clock period that can identify this code:
//   X_HIGH
//   X_LOW
// However, for others, you must provide a 5 us pulse length.
// That been said, I'm  going to assume that all of them need a 5 us pulse length. With that approach, every other ISR call
// all pins will be set to 0. 
ISR(TIMER3_COMPA_vect)
{
  if (setLow)
  {
    X_LOW
    Y_LOW
    Z_LOW
    P_LOW
    T_LOW
    setLow = false;
  }
  else
  {
    if (drillMode)
    {
      if (calculateCoordinates)
      {
        xx = radius * cos(angle);
        yy = radius * sin(angle);
        
        calculateCoordinates = false;
      }
      else
      {
        if (xx != x || yy != y)
        {
          if (xx > x)
          {
            x = x + 1 ;
            if (motorArray[X_MOTOR].direction != forward)
            {
              motorArray[X_MOTOR].direction = forward;
              motorArray[X_MOTOR].DirectionFunction(X_MOTOR, forward);
            }
            TakeStep(X_MOTOR);
          }
          if (xx < x)
          {
            x = x - 1 ;
            if (motorArray[X_MOTOR].direction != backward)
            {
              motorArray[X_MOTOR].direction = backward;
              motorArray[X_MOTOR].DirectionFunction(X_MOTOR, backward);
            }
            TakeStep(X_MOTOR);
          } 

          if (yy > y)
          {
            y = y + 1 ;
            if (motorArray[Y_MOTOR].direction != forward)
            {
              motorArray[Y_MOTOR].direction = forward;
              motorArray[Y_MOTOR].DirectionFunction(Y_MOTOR, forward);
            }
            TakeStep(Y_MOTOR);
          }
          if (yy < y)
          {
            y = y - 1 ;
            if (motorArray[Y_MOTOR].direction != backward)
            {
              motorArray[Y_MOTOR].direction = backward;
              motorArray[Y_MOTOR].DirectionFunction(Y_MOTOR, backward);
            }
            TakeStep(Y_MOTOR);
          }
        }
        else
        {
          angle += angleStep;
          calculateCoordinates = true;
        }
      }        
    }
    else
    {
      // Pseudo-code
      // For every timer interrupt:
      // 1) run throughout stepper array
      // 2) check if the motor is supposed to take a step
      //      2.1) take a step
      //      2.2) increment step counter
      // 3) if the movement is completed
      //      3.1) the movement is done
      //      3.2) update current absolute position
      for (int32_t i = 0; i < NUM_STEPPERS; i++)
      {
        if ( motorArray[i].shouldRun )
        {
          if ( motorArray[i].stepsTaken < motorArray[i].stepsRequested )
          {
            // The current strategy toggle the pin state every clock counter.
            // The TB chip requires a full pulse in order to take a step.
            // So, the motor will actually take half of the requested steps 
            // NOTE: TB chip requires a minimum pulse length of 5 us
            motorArray[i].StepFunction(i);
            if (motorArray[i].direction == forward)
              motorArray[i].currentPosition++;
            else
              motorArray[i].currentPosition--;
            motorArray[i].stepsTaken++;
            #ifdef DEBUGGING
              Debug("Steps taken")
              Debug(motorArray[i].stepsTaken)
              Debug("Current position")
              Debug(motorArray[i].currentPosition)
            #endif
          }
          else
          {
            ResetStepper(i);
            StopMotor(i);
          }
        }
      }
    }
    setLow = true;
  }
}

/* JUST TEST, AND THE EXTERNAL INTERRUPTS NEED A DEBOUNCING CIRCUIT */

// ISR(INT4_vect)
// {
//   StopMotor(X_MOTOR);
// }

// ISR(INT5_vect)
// {
//   StopMotor(Y_MOTOR);
// }

// ISR(INT6_vect)
// {
//   StopMotor(Z_MOTOR);
// }