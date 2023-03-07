/*
 * Absolute Positioning System
 * January 2018
 *
 * Ronaldo Sena
 * ronaldo.sena@outlook.com
 * 
 * Description:
 *  Timer3 will take care of all time-based tasks required by the motors.
 * 
 * TODO: check if there are any interdependency involving timer3 and find best fix for it.
 */


#ifndef MotorHandler_h
#define MotorHandler_h

#define forward true
#define backward false

#define NUM_STEPPERS 6

#define X_MOTOR 0
#define Y_MOTOR 1
#define Z_MOTOR 2
#define T_MOTOR 3
#define P_MOTOR 4
#define D_MOTOR 5

typedef struct
{
  void (*DirectionFunction)(int, bool);
  void (*StepFunction)(int);

  int32_t currentPosition; // Keep track of where the motor is
  volatile bool direction;
  volatile int32_t stepsRequested; // Steps requested for current movement
  volatile int32_t stepsTaken;// Steps taken in current movement
  volatile bool movementDone;
  volatile bool shouldRun;
  volatile int stepsPerRevolution; // Remember to double the amount set in the driver
}MotorStruct;


// Set everything up
void SetStepperMotors();

// Reset the motor to its default configurations
void ResetStepper(int whichMotor);

// Set configurations according to incoming parameters.
// Must be called before every movement
void PrepareMovement(int whichMotor, bool dir);

// Make this motor stop immediately
void StopMotor(int whichMotor);

// Request the motor to move N stepsq
// Can be interrupted by end course switches and incoming serial command
void MoveNSteps(int whichMotor, int32_t N);

// Request the motor to go to a specific position
// Can be interrupted by end course switches and incoming serial command
int MoveToAbsolutePositon(int whichMotor, int32_t pos);

// Set the direction of this motor (true = forward)
void SetDirection(int whichMotor, bool dir);

// Make this motor take a step
void TakeStep(int whichMotor);

// Plane XY to circular motion, Z-axis go up
// Circle' radius is constant
void Drill(bool mode);

// Just for tests
void TellMeCurrentPosition();
#endif