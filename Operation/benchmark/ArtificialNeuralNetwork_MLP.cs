
using System;

namespace benchmark
{
    internal class ArtificialNeuralNetwork_MLP
    {
        int qtdInputs;      // Input Neurons
        int qtdNeurons_L1;  // Neurons in Hidden Layer 1
        int qtdNeurons_L2;  // Neurons in Hidden Layer 2
        int qtdOutputs;     // Output Neurons
        bool slipState;

        // Constructor
        public ArtificialNeuralNetwork_MLP()
        {
            qtdInputs = 80;
            qtdNeurons_L1 = 20;
            qtdNeurons_L2 = 20;
            qtdOutputs = 2;
        }

        // Binary sigmoid
        public double SigmoidOutput(double y) // y são as saídas da camada atual
        {
            return (2.0 / (1.0 + Math.Exp(-y))) - 1;
        }

        public bool OutputANN(double[] x) // recebe os dados dos sensores
        {
            double[] inputs = x;
            double[] outHiddenLayer_1 = new double[qtdNeurons_L1];
            double[] outHiddenLayer_2 = new double[qtdNeurons_L2];
            double[] outFinal = new double[qtdOutputs];

            // First Hidden Layer Output
            for (int i = 0; i < qtdNeurons_L1; i++)
            {
                for (int j = 0; j < qtdInputs; j++)
                {
                    outHiddenLayer_1[i] += inputs[j] * MainForm.weigths_L1[j, i] + MainForm.bias_L1[i, 0];
                }
                outHiddenLayer_1[i] = SigmoidOutput(outHiddenLayer_1[i]);
            }
            // Second Hidden Layer Output
            for (int i = 0; i < qtdNeurons_L2; i++)
            {
                for (int j = 0; j < qtdNeurons_L1; j++)
                {
                    outHiddenLayer_2[i] += outHiddenLayer_1[j] * MainForm.weigths_L2[j, i] + MainForm.bias_L2[i, 0];
                }
                outHiddenLayer_2[i] = SigmoidOutput(outHiddenLayer_2[i]);
            }
            // Final Output
            for (int i = 0; i < qtdOutputs; i++)
            {
                for (int j = 0; j < qtdNeurons_L2; j++)
                {
                    outFinal[i] += outHiddenLayer_2[j] * MainForm.weigths_Output[j, i] + MainForm.bias_Output[i, 0];
                }
                outFinal[i] = SigmoidOutput(outFinal[i]);
            }

            // Slippage State (true: presence, false: absence)
            if ((outFinal[0] > 0.95) && (outFinal[1] < -0.95))
            {
                slipState = false;
            }
            else //if ((outFinal[0] < 0.05) && (outFinal[1] > 0.95))
            {
                slipState = true;
            }

            return slipState;
        }
    }
}