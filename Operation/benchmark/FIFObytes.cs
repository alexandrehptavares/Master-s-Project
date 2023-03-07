using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace benchmark
{
    internal class FIFObytes
    {
        byte[] fifo;
        readonly int fifoSize;
        int iWr;    // Index for WRITE in buffer
        int iRd;    // Index for READ in buffer
        private int sampleCountFifo;
        int contOverFlowFifo;

        // Constructor
        public FIFObytes(int size)
        {
            fifoSize = size;
            fifo = new byte[size];
            iWr = 0;
            iRd = 0;
            sampleCountFifo = 0;
            contOverFlowFifo = 0;
        }

        public bool Push(int val)
        {
            bool keep = true; 
            fifo[iWr] = (byte)val;
            if (sampleCountFifo >= fifoSize)  // The buffer is full, check if it will be overwritten
            {
                keep = false; // Buffer keeps the same size, but had overflow (stop)
                contOverFlowFifo++;
            }                
            else
                sampleCountFifo++;
            iWr++;  // increase the pointer to the next position
            if (iWr >= fifoSize) // if the index hits the last position, it is redirected to the first one (circular buffer) 
                iWr = 0;
            return keep;
        }

        public bool Pop_One(ref byte data)
        {
            if (sampleCountFifo < MainForm.packageSize) // Not enough Bytes to fill an entire package 
                return false;
            else
            {
                data = fifo[iRd];
                iRd++;
                sampleCountFifo--;
                if (iRd >= fifoSize)
                    iRd = 0;
                return true;    
            }
        }

        public bool Pop_Package(ref byte[] data)
        {
            if (sampleCountFifo < MainForm.packageSize) // Not enough Bytes to fill an entire package 
                return false;
            else
            {
                for (int ii = 0; ii < MainForm.packageSize-1; ii++)
                {
                    data[ii] = fifo[iRd];
                    iRd++;
                    sampleCountFifo--;
                    if (iRd >= fifoSize)
                        iRd = 0;
                }
                return true;
            }
        }

        public void Push_Back_to_the_End() // Push back to the end of the FIFO/line
        {            
            iRd -= (MainForm.packageSize - 1);
            sampleCountFifo += (MainForm.packageSize - 1);
            if (iRd < 0)
                iRd += fifoSize;

        }
    }
}
