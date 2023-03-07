
using System;
using System.Collections.Generic;
using System.Linq;

namespace benchmark
{
    internal class CircularBuffer
    {
        double[][] buffer;
        readonly int bufferSize;
        // Index for Write and Read
        public int iWr; 
        public int iRdPlot_el;             // Write, Read
        public int iRdPlot_pr;
        
        int iRdSave;                // Indíce para leitura do buffer para Salvamento dos dados
        int iRdShowLabels;
        public int sampleCount;     // Número de amostras no buffer
        public int countOverFlow;   // Conta quantas vezes o buffer perdeu dados
        int sampleCountWindow;      // Auxiliar para janelamento do sinal em 5 ms
        int sampleCountSave;        // Auxiliar para o salvamento
        int sampleCountShowLabels;
        int sampleCountPlot_el;
        int sampleCountPlot_pr;

        int iRdWindow_Electret, iRdWindow_Pressure;
        bool flag_window;           // Tem que esperar acumular as 5 primeiras amostras para preencher a janela
        double[] window;
        List<double> window_List = new List<double>();
        List<double> window_Pressure = new List<double>();

        public double countTime;

        // Constructor
        public CircularBuffer(int size)
        {
            bufferSize = size;
            buffer = new double[size][];
            iWr = 0;            
            iRdSave = 0;
            iRdPlot_el = 0;
            iRdPlot_pr = 0;
            iRdShowLabels = 0;
            sampleCount = 0;
            sampleCountWindow = 0;
            sampleCountSave = 0;
            sampleCountShowLabels = 0;
            sampleCountPlot_el = 0;
            sampleCountPlot_pr = 0;
            countOverFlow = 0;
            countTime = 0;

            iRdWindow_Electret = 0;
            iRdWindow_Pressure = 0;
            flag_window = true;
            window = new double[15];
        }

        // PUSH - POP ~~> Pegar informação - Ler informação

        // Write amplitude and time into the variable val[x y]
        public bool Push(double[] val)
        {   // store in buffer the values of x e y (time and amplitude)
            bool continuar = true; 
            buffer[iWr] = new double[5] { val[0], val[1], val[2], val[3], val[4] }; // values of x and y
            if (sampleCount >= bufferSize - 1)  // The buffer is full, check if it will be overwritten
            {
                continuar = false; // Buffer keeps the same size, but had overflow (stop)
                countOverFlow++;
            }
            else                // if there is no overflow 
            {
                sampleCount++; // increase one sample on buffer (head)
                
                //sampleCountWindow++;
                
                //sampleCountSave++;
                //sampleCountPlot_el++;
                //sampleCountPlot_pr++;
                //sampleCountShowLabels++;
            }
            countTime += 0.001;   
            iWr++; // increase the pointer to the next position
            if (iWr > bufferSize-1) // mas se já tava na última posição
                iWr = 0; //faço o ponteiro circular, ou seja, volta à primeira posição
            return continuar; // true = tudo ok , false = perdendo dados            
        }

        public bool Pop_Classifier(ref double[] v)
        {
            if (flag_window)
            {
                if (sampleCount < 10)
                    return false; // espera acumular 10 amostras
                else
                {
                    for (int ii = 0; ii < 10; ii++)
                    {
                        window_List.Add(buffer[iRdWindow_Electret][1]);
                        //window_Pressure.Add(buffer[iRdWindow][3]);
                        sampleCount--;
                        iRdWindow_Electret++; //passo para a próxima linha de leitura
                        if (iRdWindow_Electret > bufferSize - 1) //se chegou no final do buffer
                            iRdWindow_Electret -= bufferSize; //solta pro começo
                    }
                    for (int ii = 0; ii < 10; ii++)
                    {
                        window_List.Add(buffer[iRdWindow_Pressure][3]);
                        //window_Pressure.Add(buffer[iRdWindow][3]);
                        iRdWindow_Pressure++; //passo para a próxima linha de leitura
                        if (iRdWindow_Pressure > bufferSize - 1) //se chegou no final do buffer
                            iRdWindow_Pressure -= bufferSize; //solta pro começo
                    }

                    if (window_List.Count >= 80)
                    {

                        v = window_List.ToArray();
                        //Remove last position (Pressure value)
                        window_List.RemoveRange(0, 20);     //Remove first 10 data points   
                        return true;
                    }
                    else
                        return false;

                    




                    /*
                    //window_List.RemoveRange(0, 3);

                    window_List.Add(buffer[iRdWindow][1]);
                    //window_List.Add(buffer[iRdWindow][2]);
                    window_List.Add(buffer[iRdWindow][3]);
                                  
                    window[0] = window[1];
                    window[1] = window[2];
                    window[2] = window[3];
                    window[3] = window[4];
                    window[4] = buffer[iRdWindow][1];
                    
                    sampleCount--;
                    iRdWindow++;
                    if (iRdWindow > bufferSize - 1) //se chegou no final do buffer
                        iRdWindow = 0; //salta pro começo
                    */

                }
            }
            else
                return false;
            /*
            else
            {
                if (sampleCount < 5)
                    return false; // amostras não são suficientes para preencher a janela
                else
                {
                    //int count_window = 0; 
                    for (int ii = 0; ii < 5; ii++)
                    {
                        for (int jj = 1; jj < 4; jj++)
                        {
                            //window[count_window] = buffer[jj][ii];
                            window_List.Add(buffer[ii][jj]);
                            //count_window++;
                        }
                        sampleCount--;
                        iRdWindow++;
                    }
                    v = window_List.ToArray();
                    flag_window = true;
                    return true;
                }
            }   */         
        }

        public bool Pop_Save(ref double[][] v)
        {
            if (sampleCountSave == 0)
                return false; // nenhuma amostra disponível para leitura
            else
            {
                int auxSampleCountSave = sampleCountSave;
                double[][] dataToSave = new double[auxSampleCountSave][];
                for (int ii = 0; ii < auxSampleCountSave; ii++)
                {
                    dataToSave[ii] = buffer[iRdSave];
                    sampleCountSave--;
                    sampleCount = sampleCountSave;
                    iRdSave++; //passo para a próxima linha de leitura
                    if (iRdSave > bufferSize-1) //se chegou no final do buffer
                        iRdSave = 0; //solta pro começo
                }
                v = dataToSave;
            }
            return true;
        }

        // Passagem de valor por referencia, mando para pop o end de memoria do vetor v, vai escrever na variavel q chamou
        public bool Pop_Plot_electret(ref double[] v)
        {
            if (sampleCount == 0)
                return false; //não tem nada para ler
            else
            {
                // Take the last point written in the buffer
                iRdPlot_el = iWr - 1;
                if (iRdPlot_el < 0) //se chegou no final do buffer
                    iRdPlot_el += bufferSize;
                    
                v = buffer[iRdPlot_el];

                /*
                v[0] = buffer[iRdPlot_el][0];
                v[1] = buffer[iRdPlot_el][1];
                v[2] = buffer[iRdPlot_el][2];
                v[3] = buffer[iRdPlot_el][3];
                v[4] = buffer[iRdPlot_el][4];
              
                //bRdPlot_el++;
                //sampleCountPlot_el--;
                iRdPlot_el += sampleCountPlot_el;
                sampleCountPlot_el = 0;
                if (iRdPlot_el >= bufferSize - 1) //se chegou no final do buffer
                    iRdPlot_el = 0; //salta para o começo
                */
                return true;
            }
        }

        public bool Pop_Plot_pression(ref double[] v)
        {
            if (sampleCount == 0)
                return false; //não tem nada para ler
            else
            {
                // Take the last point written in the buffer
                iRdPlot_pr = iWr - 1;
                if (iRdPlot_pr < 0) //se chegou no final do buffer
                    iRdPlot_pr += bufferSize;

                v = buffer[iRdPlot_pr];

                /*
                v[0] = buffer[iRdPlot_pr][0];
                v[1] = buffer[iRdPlot_pr][1];
                v[2] = buffer[iRdPlot_pr][2];
                v[3] = buffer[iRdPlot_pr][3];
                v[4] = buffer[iRdPlot_pr][4];
                //bRdPlot_pr++;
                //sampleCountPlot_pr--;
                iRdPlot_pr += sampleCountPlot_pr;
                sampleCountPlot_pr = 0;
                if (iRdPlot_pr >= bufferSize - 1) //se chegou no final do buffer
                    iRdPlot_pr = 0; //salta para o começo
                    */
                return true;
            }
        }

        public bool Pop_Show_Labels(ref double[] v)
        {
            if (sampleCount == 0)
                return false; // nenhuma amostra disponível para leitura
            else
            {
                iRdShowLabels = iWr - 1;
                if (iRdShowLabels < 0) // se chegou no final do buffer
                    iRdShowLabels += bufferSize;

                v = buffer[iRdShowLabels];

                /*
                v = buffer[iRdShowLabels];
                iRdShowLabels += sampleCountShowLabels; //passo para a próxima linha de leitura
                sampleCountShowLabels = 0;
                if (iRdShowLabels >= bufferSize - 1) //se chegou no final do buffer
                    iRdShowLabels = 0; //solta pro começo
                    */
                return true;
            }            
        }
    }
}