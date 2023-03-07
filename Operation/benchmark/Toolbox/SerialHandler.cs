using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Diagnostics;
using System.Threading;

namespace benchmark.Toolbox
{
    partial class SerialHandler
    {
        public SerialPort Board;
        public bool LedState { get; set; }
        public bool Handshake { get; private set; }

        byte[] sendThis = new byte[3];

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="portName"> Serial port name</param>
        /// <param name="baudRate">Baud rate used</param>
        public SerialHandler(string portName = "COM4", int baudRate = 115200) // The portName property cannot be empty
        {
            Board = new SerialPort
            {
                PortName = portName,
                BaudRate = 230400, 
                ReadBufferSize = 1000                
            };
            Board.ReadTimeout = 3000;
            Board.WriteTimeout = 3000;
        }

        /// <summary>
        /// Toggle acquisition board connection state
        /// </summary>
        public bool OpenConnection()
        {
            byte[] confirm = new byte[1];
            if (!Board.IsOpen)
            {
                Board.Open();
                Board.DiscardOutBuffer();
                Board.DiscardInBuffer();
                Handshake = DoHandshake();                
            }
            else
            {
                SendThis(SerialCommands.CMD_RESET);
                try
                {
                    Board.Read(confirm, 0, 1);
                }
                catch (TimeoutException)
                {
                    Debug.Print("Timeout exception!");
                    Debug.Print(Board.PortName);
                    return false;
                }

                if (confirm[0] == SerialCommands.CO_RESET)
                {
                    Handshake = false;
                    LedState = false;
                    SpindleState = false; // MotionControl.cs
                    DrillState = false; // MotionControl.cs
                    Board.DiscardOutBuffer();
                    Board.DiscardInBuffer();
                    Board.Close();
                }                
            }
            return true;
        }

        public bool DoHandshake()
        {
            byte[] ConfirmFirmware = new byte[1];
            string firmwareVersion;

            SendThis(SerialCommands.CMD_FMR_VER);
            try
            {
                firmwareVersion = Board.ReadLine();
                firmwareVersion = firmwareVersion.TrimEnd('\r');
            }
            catch (TimeoutException)
            {
                Debug.Print("Timeout exception!");
                return false;
            }

            if (firmwareVersion == SerialCommands.FIRMWARE_VERSION)
            {
                SendThis(SerialCommands.CMD_FMR_VER_OK);
                Debug.Print("Successful connection");
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SendThis(byte thisByte)
        {
            byte[] sendThis = new byte[2]; // Used to send individual commands since .NET doesn't support 
            sendThis[0] = thisByte;
            sendThis[1] = SerialCommands.END_OF_MSG;
            Board.Write(sendThis, 0, sendThis.Length);
        }

        public void SendThis(byte[] thisByte)
        {
            Board.Write(thisByte, 0, thisByte.Length);
        }

        /// <summary>
        /// This is supposed to be used only during tests
        /// </summary>
        /// <returns> True if successful.</returns>
        public bool ToggleLED()
        {
            byte[] ConfirmLED = new byte[1];

            if (Board.IsOpen)
            {
                if (!LedState)
                {
                    SendThis(SerialCommands.CMD_LED_ON);
                }
                else
                {
                    SendThis(SerialCommands.CMD_LED_OFF);
                }
                                
                try
                {
                    Board.Read(ConfirmLED, 0, 1);
                }
                catch (TimeoutException)
                {
                    Debug.Print("Timeout exception!");
                    return false;
                }

                if (ConfirmLED[0] == SerialCommands.CO_LED_OFF)
                {
                    LedState = false;
                    return true;
                }   

                if (ConfirmLED[0] == SerialCommands.CO_LED_ON)
                {
                    LedState = true;
                    return true;
                }
            }
            return false;
        }
    }
}
