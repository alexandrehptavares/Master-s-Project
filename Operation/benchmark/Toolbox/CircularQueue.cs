/*
 * Absolute Positioning System - Circular Queue class
 * 
 * Author: Ronaldo Sena
 * Contact: ronaldo.sena@outlook.com
 * Github: @ronaldosena
 * Date: January 2018
 * 
 * Description:
 * Thread-safe circular queue class
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace benchmark.Toolbox
{
    public class CircularQueue<ADT>
    {
        object lockProcess = new object();

        /// <summary>
        /// Storage array
        /// </summary>
        ADT[] circularQueue;

        /// <summary>
        /// Queue's current size
        /// </summary>
        public int currentSize { get; private set; }

        /// <summary>
        /// Queue's maximum size
        /// </summary>
        public int maxSize { get; set; }

        /// <summary>
        /// Last element in the queue
        /// </summary>
        private int head;

        /// <summary>
        /// First element in the queue
        /// </summary>
        private int tail;

        /// <summary>
        /// Construct. Default queue size is 1024
        /// </summary>
        public CircularQueue()
        {
            maxSize = 1024; //By default
            currentSize = 0;
            head = 0;
            tail = 0;
            circularQueue = new ADT[maxSize];
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxSize">Maximum queue size</param>
        public CircularQueue(int MaxSize)
        {
            maxSize = MaxSize;
            currentSize = 0;
            head = 0;
            tail = 0;
            circularQueue = new ADT[maxSize];
        }

        /// <summary>
        /// Is the queue empty?
        /// </summary>
        public bool IsEmpty
        {
            get { return currentSize == 0; }
        }

        /// <summary>
        /// Is the queue full?
        /// </summary>
        public bool IsFull
        {
            get { return currentSize == maxSize; }
        }

        /// <summary>
        /// Enqueue some data
        /// </summary>
        /// <param name="data">Data to insert</param>
        /// <returns>True if successful</returns>
        public bool Enqueue(ADT data)
        {
            lock (lockProcess)
            {
                if (!IsFull)
                {
                    circularQueue[head] = data;
                    head = ++head % maxSize;
                    currentSize++;
                    return true;
                }
                else
                    Debug.WriteLine("Queue is full!");
                return false;
            }
        }

        /// <summary>
        /// Dequeue data. Check if there is anything inside of it first
        /// </summary>
        /// <returns>Returns the data in queue's tail</returns>
        public ADT Dequeue()
        {
            lock (lockProcess)
            {
                ADT data = circularQueue[tail];
                tail = ++tail % maxSize;
                currentSize--;
                return data;
            }
        }

        /// <summary>
        /// Peeks first out element
        /// </summary>
        /// <returns>ADT data</returns>
        public ADT Peek()
        {
            lock (lockProcess)
            {
                if (!IsEmpty)
                {
                    return circularQueue[tail];
                }
                else
                    Debug.WriteLineIf(IsEmpty, "Queue is empty!");
                return default(ADT);
            }
        }
    }
}