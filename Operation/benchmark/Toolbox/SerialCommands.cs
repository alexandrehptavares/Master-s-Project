namespace benchmark.Toolbox
{
    public static class SerialCommands
    {
        public static string FIRMWARE_VERSION = "1.0.0";

        public static byte CMD_RESET = 0x20;
        public static byte CMD_FMR_VER = 0x21;
        public static byte CMD_FMR_VER_OK = 0x22;
        public static byte CMD_BEGIN = 0x23;
        public static byte CMD_END = 0x24;
        public static byte CMD_LED_OFF = 0x25;
        public static byte CMD_LED_ON = 0x26;
        public static byte CMD_SPEED = 0x27;
        public static byte CMD_X_FWD = 0x28;
        public static byte CMD_X_BCK = 0x29;
        public static byte CMD_X_STOP = 0x2A;
        public static byte CMD_X_STEPS = 0x2B;
        public static byte CMD_X_MOVETO = 0x2C;
        public static byte CMD_Y_FWD = 0x2D;
        public static byte CMD_Y_BCK = 0x2E;
        public static byte CMD_Y_STOP = 0x2F;
        public static byte CMD_Y_STEPS = 0x30;
        public static byte CMD_Y_MOVETO = 0x31;
        public static byte CMD_Z_FWD = 0x32;
        public static byte CMD_Z_BCK = 0x33;
        public static byte CMD_Z_STOP = 0x34;
        public static byte CMD_Z_STEPS = 0x35;
        public static byte CMD_Z_MOVETO = 0x36;
        public static byte CMD_P_FWD = 0x37;
        public static byte CMD_P_BCK = 0x38;
        public static byte CMD_P_STOP = 0x39;
        public static byte CMD_P_STEPS = 0x3A;
        public static byte CMD_P_MOVETO = 0x3B;
        public static byte CMD_T_FWD = 0x3C;
        public static byte CMD_T_BCK = 0x3D;
        public static byte CMD_T_STOP = 0x3E;
        public static byte CMD_T_STEPS = 0x3F;
        public static byte CMD_T_MOVETO = 0x40;
        public static byte CMD_SPINDLE_ON = 0x41;
        public static byte CMD_SPINDLE_OFF = 0x42;
        public static byte CMD_CIRCLE_ON = 0x43;
        public static byte CMD_CIRCLE_OFF = 0x44;
        
        

        public static byte CONFIRM_ORDER = 0x50; // Confirming order indicator
        public static byte CO_LED_ON = 0x51; // Confirm Turn on indication led
        public static byte CO_LED_OFF = 0x52; // Confirm Turn off indication led
        public static byte CO_SPINDLE_ON = 0x53;
        public static byte CO_SPINDLE_OFF = 0x54;
        public static byte CO_CIRCLE_ON = 0x55;
        public static byte CO_CIRCLE_OFF = 0x56;
        public static byte CO_SPEED = 0x57;
        public static byte CO_X_FWD = 0x58; // Confirm X axis go forward
        public static byte CO_X_BCK = 0x59; // Confirm X axis go backward
        public static byte CO_X_STOP = 0x5A; // Confirm X axis stop
        public static byte CO_X_STEPS = 0x5B;
        public static byte CO_X_MOVETO = 0x5C;
        public static byte CO_Y_FWD = 0x5D; // Confirm Y axis go forward
        public static byte CO_Y_BCK = 0x5E; // Confirm Y axis go backward
        public static byte CO_Y_STOP = 0x5F; // Confirm Y axis stop
        public static byte CO_Y_STEPS = 0x60;
        public static byte CO_Y_MOVETO = 0x61;
        public static byte CO_Z_FWD = 0x62; // Confirm Z axis go forward
        public static byte CO_Z_BCK = 0x63; // Confirm Z axis go backward
        public static byte CO_Z_STOP = 0x64; // Confirm Z axis stop
        public static byte CO_Z_STEPS = 0x65;
        public static byte CO_Z_MOVETO = 0x66;
        public static byte CO_P_FWD = 0x67; // Confirm P axis go forward
        public static byte CO_P_BCK = 0x68; // Confirm P axis go backward
        public static byte CO_P_STOP = 0x69; // Confirm P axis stop
        public static byte CO_P_STEPS = 0x6A;
        public static byte CO_P_MOVETO = 0x6B;
        public static byte CO_T_FWD = 0x6C; // Confirm T axis go forward
        public static byte CO_T_BCK = 0x6D; // Confirm T axis go backward
        public static byte CO_T_STOP = 0x6E; // Confirm T axis stop
        public static byte CO_T_STEPS = 0x6F;
        public static byte CO_T_MOVETO = 0x70;
        public static byte CO_FMR_VER = 0x71; // Confirm firmware version
        public static byte CO_RESET = 0x72; // Confirm firmware version
        public static byte END_OF_MSG = 0x73; // End of confirmation message

    }
}
