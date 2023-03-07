#define FIRMWARE_VERSION "1.0.0"

#include "lib/PortManipulation.c"
#include "lib/FSM.c"

/// ==================================== REGISTRADORES ====================================
/*
  // X axis
  #define PORT_PIN_X  PORTC
  #define DDR_PIN_X   DDRC
  #define PIN_X_ENA   PC2 // 35
  #define PIN_X_DIR   PC4 // 33
  #define PIN_X_SETP  PC6 // 31

  #define X_DIR_FORWARD  PORT_PIN_X |= (1<<PIN_X_DIR);
  #define X_DIR_BACKWARD PORT_PIN_X &=~(1<<PIN_X_DIR);
*/


/// ================================= DECLARAÇÃO DOS PINOS =================================
// Sensores
const int eletreto_Sensor = A0;
const int pression_Sensor_1 = A2;
const int pression_Sensor_2 = A3;
const int photo_int_sensor = A5;

/// ===================================== VARIÁVEIS =======================================
// Controle do Motor
bool flag_motor = false;
int motor_control = 0;

// Sensores
int eletreto_signal;
int pression_1_signal;
int pression_2_signal;
int photo_interruptor_signal;

// Pacote de Bytes no protocolo UART
#define StartByte 0x7E
byte LSB_el;
byte MSB_el;
byte LSB_p_1;
byte MSB_p_1;
byte LSB_p_2;
byte MSB_p_2;
byte LSB_pI;
byte MSB_pI;
byte CheckSum;
#define StopByte 0x81

// Para evitar debouncing das interrupções vindas da Serial
int maxReedCounter = 10; //min time (in ms) of one rotation (for debouncing)
int reedCounter;

/// ================================= DECLARAÇÃO DE FUNÇÕES =================================
void get_data();
void move_motor_foward();
void move_motor_bacward();
void plot_all_sensors();

double count_serial;

void setup()
{
  Serial.begin(115200);

  SetPins();
  SetInterrupts();
  SetStepperMotors();
  SetTimeInterval(100);
  TIMER3_INTERRUPTS_ON;

  // Timer for sensors data send
  count_serial = 0;
  //set_timer_sensors();  

  //Timer1.attachInterrupt(get_data).start(1000);
}


void loop()
{

  motor_handler();

  /*
  switch (motor_control)
  {
    case 1: // Move foward
      MoveNSteps(0, 1);
      break;
    case 2: // Move Backward
      MoveNSteps(0, -1);
      break;
    case 3: // Stop Motor
      StopMotor(0);
      break;
    case 4: // Move foward faster (before starting control)
      MoveNSteps(0, 2);
      break;
  }

  if (Serial.available())
  {
    motor_control = Serial.read();
  }
  */
  
}


// ===========================================================================================
// ----------------------------- Funções do Timer e Interrupções -----------------------------
// ===========================================================================================


ISR(TIMER1_COMPA_vect)
{
  //Interrupt at freq of 1kHz

  eletreto_signal = analogRead(eletreto_Sensor);
  pression_1_signal = analogRead(pression_Sensor_1);
  pression_2_signal = analogRead(pression_Sensor_2);
  photo_interruptor_signal = analogRead(photo_int_sensor);

  CheckSum = MSB_el + MSB_p_1 + MSB_p_2 + MSB_pI;
  //CheckSum = (byte)(CheckSum & 0xFF);

  Serial.write(StartByte); // Decimal = 126  // 1º Byte de verificação: START BYTE
  // Inicio dos dados
  Serial.write((byte)(eletreto_signal & 0xFF));
  
  Serial.write(eletreto_signal >> 8);
  Serial.write((byte)(pression_1_signal & 0xFF));
  Serial.write(pression_1_signal >> 8);
  Serial.write((byte)(pression_2_signal & 0xFF));
  Serial.write(pression_2_signal >> 8);
  Serial.write((byte)(photo_interruptor_signal & 0xFF));
  Serial.write(photo_interruptor_signal >> 8);
  // Fim dos dados
  Serial.write(CheckSum); // 2º Byte de verificação: CHECK SUM
  Serial.write(StopByte); // Decimal = 129  // 3º Byte de verificação: STOP BYTE

  Serial.println(count_serial);
  count_serial += 0.001;
}

void get_data()
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

  Serial.write(StartByte); // Decimal = 126  // 1º Byte de verificação: START BYTE
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
  Serial.write(StopByte);              // Decimal = 129  // 3º Byte de verificação: STOP BYTE
}

/*
void set_timer_sensors()
{
  // TIMER SETUP- the timer interrupt allows preceise timed measurements of the reed switch
  //for mor info about configuration of arduino timers see http://arduino.cc/playground/Code/Timer1
  cli();//stop interrupts

  //set timer1 interrupt at 1kHz
  TCCR1A = 0;// set entire TCCR1A register to 0
  TCCR1B = 0;// same for TCCR1B
  TCNT1  = 0;//initialize counter value to 0;
  // set timer count for 1khz increments
  OCR1A = 1999;// = (16*10^6) / (1000*8) - 1
  // turn on CTC mode
  TCCR1B |= (1 << WGM12);
  // Set CS11 bit for 8 prescaler
  TCCR1B |= (1 << CS11);
  // enable timer compare interrupt
  TIMSK1 |= (1 << OCIE1A);

  //sei();//allow interrupts
  //END TIMER SETUP
}
*/

void motor_handler()
{

/*
  // ---------------------------------------
  //          To avoid debouncing
  // ---------------------------------------
  if (reedCounter == 0){//min time between pulses has passed
      mph = (56.8*float(circumference))/float(time);//calculate miles per hour
      time = 0;//reset timer
      reedCounter = maxReedCounter;//reset reedCounter
    }
    else{
      if (reedCounter > 0){//don't let reedCounter go negative
        reedCounter -= 1;//decrement reedCounter
      }
    }
  }
  else{//if reed switch is open
    if (reedCounter > 0){//don't let reedCounter go negative
      reedCounter -= 1;//decrement reedCounter
    }
  }
  */
  
  //   
  if(Serial.available())
  {
    noInterrupts();
    X_HIGH
    interrupts();
  }  
}

void move_motor_foward()
{
  MoveNSteps(0, 1);
}

void move_motor_bacward()
{
  MoveNSteps(0, -1);
}

void plot_all_sensors()
{
  // Abrir o Plotter Serial (Ctrl+Shift+L)
  Serial.print(analogRead(A0));
  Serial.print(",");
  Serial.print(analogRead(A2));
  Serial.print(",");
  Serial.println(analogRead(A3));
  Serial.print(",");
  Serial.print(analogRead(A5));
  Serial.print(",");
  //delay(1);
}
