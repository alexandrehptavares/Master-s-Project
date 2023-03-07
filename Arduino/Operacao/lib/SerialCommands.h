/*
 * Absolute Positioning System
 * January 2018
 *
 * Ronaldo Sena
 * ronaldo.sena@outlook.com
 * 
 * Description:
 * Commands used in serial communication
 */

#ifndef SerialCommands_h
#define SerialCommands_h

// Incoming commands
//#define terminatorChar 0x23 // ASCII #
#define CMD_RESET       0x20
#define CMD_FMR_VER     0x21
#define CMD_FMR_VER_OK  0x22
#define CMD_BEGIN       0x23
#define CMD_END         0x24
#define CMD_LED_OFF     0x25
#define CMD_LED_ON      0x26
#define CMD_SPEED       0x27
#define CMD_X_FWD       0x28
#define CMD_X_BCK       0x29
#define CMD_X_STOP      0x2A
#define CMD_X_STEPS     0x2B
#define CMD_X_MOVETO    0x2C
#define CMD_Y_FWD       0x2D
#define CMD_Y_BCK       0x2E
#define CMD_Y_STOP      0x2F
#define CMD_Y_STEPS     0x30
#define CMD_Y_MOVETO    0x31
#define CMD_Z_FWD       0x32
#define CMD_Z_BCK       0x33
#define CMD_Z_STOP      0x34
#define CMD_Z_STEPS     0x35
#define CMD_Z_MOVETO    0x36
#define CMD_P_FWD       0x37
#define CMD_P_BCK       0x38
#define CMD_P_STOP      0x39
#define CMD_P_STEPS     0x3A
#define CMD_P_MOVETO    0x3B
#define CMD_T_FWD       0x3C
#define CMD_T_BCK       0x3D
#define CMD_T_STOP      0x3E
#define CMD_T_STEPS     0x3F
#define CMD_T_MOVETO    0x40
#define CMD_SPINDLE_ON  0x41
#define CMD_SPINDLE_OFF 0x42
#define CMD_CIRCLE_ON   0x43
#define CMD_CIRCLE_OFF  0x44


// Forming Data Packets
// There will be two types of Data Packets:
//      1) Confirming an order
//      2) Giving feedback of the process
// The first type will be a 3 byte packet as follows:
// | HEADER | ORDER TO CONFIRM | TAIL
// (CONFIRM_ORDER)(CMD_@)(END_OF_MSG)
#define CONFIRM_ORDER  0x50
#define CO_LED_ON      0x51
#define CO_LED_OFF     0x52
#define CO_SPINDLE_ON  0x53
#define CO_SPINDLE_OFF 0x54
#define CO_CIRCLE_ON   0x55
#define CO_CIRCLE_OFF  0x56
#define CO_SPEED       0x57
#define CO_X_FWD      0x58
#define CO_X_BCK      0x59
#define CO_X_STOP     0x5A
#define CO_X_STEPS    0x5B
#define CO_X_MOVETO   0x5C
#define CO_Y_FWD      0x5D
#define CO_Y_BCK      0x5E
#define CO_Y_STOP     0x5F
#define CO_Y_STEPS    0x60
#define CO_Y_MOVETO   0x61
#define CO_Z_FWD      0x62
#define CO_Z_BCK      0x63
#define CO_Z_STOP     0x64
#define CO_Z_STEPS    0x65
#define CO_Z_MOVETO   0x66
#define CO_P_FWD      0x67
#define CO_P_BCK      0x68
#define CO_P_STOP     0x69
#define CO_P_STEPS    0x6A
#define CO_P_MOVETO   0x6B
#define CO_T_FWD      0x6C
#define CO_T_BCK      0x6D
#define CO_T_STOP     0x6E
#define CO_T_STEPS    0x6F
#define CO_T_MOVETO   0x70
#define CO_FMR_VER    0x71
#define CO_RESET      0x72
#define END_OF_MSG    0x73

// The second type will be a 4 byte packet, giving numerical information about what is going on 
// | HEADER | DATA_MSB | DATA_LSB | TAIL
#define DATA_START    0xA0
#define DATA_END      0xA0

#endif
