#define FIRMWARE_VERSION "1.0.0"

#include "lib/PortManipulation.c"
#include "lib/FSM.c"

#include <Timer.h>

/// DECLARAÇÃO DOS PINOS
// Sensores
const int eletreto_Sensor = A0;
const int pression_Sensor_1 = A2;
const int pression_Sensor_2 = A3;
const int photo_int_sensor = A5;

/// VARIÁVEIS
bool flag_motor = false;
bool flag_reverse = false;

byte motor_control = 0;

int eletreto_signal;
int pression_1_signal;
int pression_2_signal;
int photo_interruptor_signal;

byte LSB_el;
byte MSB_el;
byte LSB_p_1;
byte MSB_p_1;
byte LSB_p_2;
byte MSB_p_2;
byte LSB_pI;
byte MSB_pI;
byte CheckSum;

Timer Timer_send_data;
Timer Timer_move_motor;

void setup()
{
  Serial.begin(115200);

  SetPins();
  SetInterrupts();
  SetStepperMotors();
  SetTimeInterval(100);
  TIMER3_INTERRUPTS_ON;

  // chama o Timer com tempo de 1 ms cada
  Timer_send_data.every(1, Get_dados);
  Timer_move_motor.every(15, Move_Motor);
}

void Get_dados()
{
  eletreto_signal = analogRead(eletreto_Sensor);
  pression_1_signal = analogRead(pression_Sensor_1);
  pression_2_signal = analogRead(pression_Sensor_2);
  photo_interruptor_signal = analogRead(photo_int_sensor);

  LSB_el = (byte)(eletreto_signal & 0xFF);
  MSB_el = eletreto_signal >> 8;
  LSB_p_1 = (byte)(pression_1_signal & 0xFF);
  MSB_p_1 = pression_1_signal >> 8;
  LSB_p_2 = (byte)(pression_2_signal & 0xFF);
  MSB_p_2 = pression_2_signal >> 8;
  LSB_pI = (byte)(photo_interruptor_signal & 0xFF);
  MSB_pI = photo_interruptor_signal >> 8;

  CheckSum = MSB_el + MSB_p_1 + MSB_p_2 + MSB_pI;
  //CheckSum += LSB_el + LSB_p_1 + LSB_p_2 + LSB_pI;
  CheckSum = (byte)(CheckSum & 0xFF);

  Serial.write(0x7E); // Decimal = 126  // 1º Byte de verificação: START BYTE
  // Inicio dos dados
  Serial.write(LSB_el);
  Serial.write(MSB_el);
  Serial.write(LSB_p_1);
  Serial.write(MSB_p_1);
  Serial.write(LSB_p_2);
  Serial.write(MSB_p_2);
  Serial.write(LSB_pI);
  Serial.write(MSB_pI);
  // Fim dos dados
  Serial.write(CheckSum); // 2º Byte de verificação: CHECK SUM
  Serial.write(0x81);              // Decimal = 129  // 3º Byte de verificação: STOP BYTE

}

void Move_Motor()
{
  MoveNSteps(0, -1);
}

void Move_Back_Motor()
{
  MoveNSteps(0, 1);
}

void Plot_All_sensors()
{
  // Abrir o Plotter Serial (Ctrl+Shift+L)
  Serial.print(analogRead(A1));
  Serial.print(",");
  Serial.print(analogRead(A2));
  Serial.print(",");
  Serial.print(analogRead(A3));
  Serial.print(",");
  Serial.println(analogRead(A5));
  delay(1);
}

void loop()
{
  // RunFSM();


  switch (motor_control)
  {
    case 1:
      Timer_send_data.update();
      Timer_move_motor.update();
      break;
    case 2:
      StopMotor(0);
      break;
    case 3:
      Move_Back_Motor();
      delay(5);
      break;
  }

  /*
    if (motor_control == 1)
    {
    Timer_send_data.update();
    Timer_move_motor.update();
    }
  */

  if (Serial.available())
  {
    motor_control = Serial.read();
  }

}

