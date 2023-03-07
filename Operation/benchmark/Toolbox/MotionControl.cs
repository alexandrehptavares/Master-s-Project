using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using benchmark.Toolbox;

namespace benchmark.Toolbox
{
    partial class  SerialHandler
    {
        public bool SpindleState { get; set; }
        public bool DrillState { get; set; }
    
        public bool MoveCommand(byte CMD, byte CO)
        {
            byte[] byte2send = new byte[6];
            byte[] ConfirmOrder = new byte[1];

            byte2send[0] = CMD;
            byte2send[1] = SerialCommands.END_OF_MSG;
            SendThis(byte2send);

            if (Board.IsOpen)
            {
                SendThis(byte2send);
                try
                {
                    Board.Read(ConfirmOrder, 0, 1);
                }
                catch (TimeoutException)
                {
                    Debug.Print("Timeout exception!");
                    return false;
                }

                if (ConfirmOrder[0] == CO)
                {
                    return true;
                }
            }
            return false;
        }

        public bool MoveNSteps(Int32 N, byte CMD, byte CO)
        {
            byte[] ConfirmOrder = new byte[8];
            byte[] byte2send = new byte[6];

            if (Board.IsOpen)
            {
                byte2send[0] = CMD;
                BitConverter.GetBytes(N).CopyTo(byte2send, 1);
                byte2send[5] = SerialCommands.END_OF_MSG;
                SendThis(byte2send);
                System.Threading.Thread.Sleep(100);
                try
                {
                    Board.Read(ConfirmOrder, 0, 1);
                }
                catch (TimeoutException)
                {
                    Debug.Print("Timeout exception!");
                    return false;
                }

                if (ConfirmOrder[0] == CO)
                {
                    return true;
                }
            }
            return false;
        }

        public bool SetSpeed(Int16 interval)
        {
            byte[] ConfirmOrder = new byte[2];
            byte[] byte2send = new byte[4];

            if (Board.IsOpen)
            {
                byte2send[0] = SerialCommands.CMD_SPEED;
                BitConverter.GetBytes(interval).CopyTo(byte2send, 1);
                byte2send[3] = SerialCommands.END_OF_MSG;
                SendThis(byte2send);
                System.Threading.Thread.Sleep(100);
                try
                {
                    Board.Read(ConfirmOrder, 0, 1);
                }
                catch (TimeoutException)
                {
                    Debug.Print("Timeout exception!");
                    return false;
                }

                if (ConfirmOrder[0] == SerialCommands.CO_SPEED)
                {
                    return true;
                }
            }
            return false;
        }

        public bool Spindle()
        {
            byte[] ConfirmOrder = new byte[1];

            if (Board.IsOpen)
            {
                if (!SpindleState)
                {
                    SendThis(SerialCommands.CMD_SPINDLE_ON);
                }
                else
                {
                    SendThis(SerialCommands.CMD_SPINDLE_OFF);
                }

                try
                {
                    Board.Read(ConfirmOrder, 0, 1);
                }
                catch (TimeoutException)
                {
                    Debug.Print("Timeout exception!");
                    return false;
                }

                if (ConfirmOrder[0] == SerialCommands.CO_SPINDLE_OFF)
                {
                    SpindleState = false;
                    return true;
                }

                if (ConfirmOrder[0] == SerialCommands.CO_SPINDLE_ON)
                {
                    SpindleState = true;
                    return true;
                }
            }
            return false;
        }

        public bool Circle()
        {
            byte[] ConfirmOrder = new byte[1];

            if (Board.IsOpen)
            {
                if (!DrillState)
                {
                    SendThis(SerialCommands.CMD_CIRCLE_ON);
                }
                else
                {
                    SendThis(SerialCommands.CMD_CIRCLE_OFF);
                }

                try
                {
                    Board.Read(ConfirmOrder, 0, 1);
                }
                catch (TimeoutException)
                {
                    Debug.Print("Timeout exception!");
                    return false;
                }

                if (ConfirmOrder[0] == SerialCommands.CO_CIRCLE_OFF)
                {
                    DrillState = false;
                    return true;
                }

                if (ConfirmOrder[0] == SerialCommands.CO_CIRCLE_ON)
                {
                    DrillState = true;
                    return true;
                }
            }
            return false;
        }
    }
}