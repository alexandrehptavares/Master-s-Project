
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.IO.Ports;
using benchmark.Toolbox;
using System.Windows.Forms.DataVisualization.Charting;
using Accord.Math;
using Accord.IO;

namespace benchmark
{
    public partial class MainForm : Form
    {
        private SerialHandler Controller_Serial;
        private Int16 normSpeed;
        
        // Variáveis relacionadas ao gráfico
        ChartArea graphArea_Electret; // manipular os parâmetros do gráfico
        ChartArea graphArea_Pression;
        Series electret_signal; //série que será desenhada no gráfico
        Series pression_1_signal;
        Series pression_2_signal;
        double window_plot_Size; // Controle do tamanho da janela de plotagem
        
        // Circular Buffer
        CircularBuffer BC;
        double[] y;
        double[] y_plot_el;
        double[] y_plot_pr;
        byte Byte;
        byte[] dataBytes = new byte[packageSize-1];
        int bytesToRead = 0;
        // Classificador
        double[] window;
        bool state;
        // Salvamento
        double[][] dataToSave;
        double[] dataToShow;
        String[] line_table;
        FIFObytes fifoBytes;
        // Controle
        //byte[] sendByte = new byte[1];

        /*
        byte StartByte;
        byte LSB_el;
        byte MSB_el;
        byte LSB_p_1;
        byte MSB_p_1;
        byte LSB_p_2;
        byte MSB_p_2;
        byte LSB_pI;
        byte MSB_pI;
        byte soma_valores_MSBs;
        byte StopByte;
        */
        // RNA
        ArtificialNeuralNetwork_MLP ANN_MLP;
        public static double[,] weigths_L1;
        public static double[,] weigths_L2;
        public static double[,] weigths_Output;
        public static double[,] bias_L1;
        public static double[,] bias_L2;
        public static double[,] bias_Output;

        // Control System
        ControlSystem SC;

        // Flags para as Threads
        bool runThAq, runThClass, runThPlot, runThSave, runThLabels;
        bool flagStart, flagLife;
        Thread thrAq, thrGetBytes, thrClass, thrPlot_el, thrPlot_pr, thrSave, thrLabels;
        Mutex mutex;            // Mutex Execution evita que duas threads utilizem a mesma função ao msm tempo
        Stopwatch stopwatch;    // Controle do tempo de aquisição;

        // Outras
        public const int packageSize = 10;  // Número de Bytes do pacote de dados 
        const uint bits = 12;               // Número de bits do conversor AD 
        double ampScale = Math.Pow(2, bits);
        const int sampFreq = 1000;
        const double dt = 1.0 / sampFreq;
        double countSerial = 0;


        public MainForm()
        {
            InitializeComponent(); // Inicialização dos componentes gráficos
            UploadWeigthsMatrix(); // Carregamento dos pesos treinados da RNA

            Controller_Serial = new SerialHandler("COM4", 230400); // Definição do Controller Serial
            Set_Aveilable_Ports();                                 // Configuração das portas seriais
            stopwatch = new Stopwatch();

            runThAq = true;
            runThClass = true;
            runThPlot = false;
            runThSave = false;
            runThLabels = true;
            flagStart = true;
            flagLife = true;

            thrAq = new Thread(Acquisition);
            thrAq.Priority = ThreadPriority.Highest;
            thrClass = new Thread(Classification);
            thrClass.Priority = ThreadPriority.Highest;
            thrGetBytes = new Thread(GetDataIntoBytes);
            thrGetBytes.Priority = ThreadPriority.Highest;
            //thrSave = new Thread(Saving);
            //thrSave.Priority = ThreadPriority.AboveNormal;
            //thrPlot_el = new Thread(Plotting_electret);
            //thrPlot_el.Priority = ThreadPriority.Normal;
            //thrPlot_pr = new Thread(Plotting_pression);
            //thrPlot_pr.Priority = ThreadPriority.Normal;
            thrLabels = new Thread(Show_Labels);
            thrLabels.Priority = ThreadPriority.Lowest;

            //ampScale = Math.Pow(2, bits);
            window_plot_Size = 2;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            BC = new CircularBuffer(100);                   // Criando o objeto do buffer circular 
            fifoBytes = new FIFObytes(100);                 // Criando objeto da FIFO bytes
            ANN_MLP = new ArtificialNeuralNetwork_MLP();    // Criando objeto da Rede Neural MLP
            mutex = new Mutex();
            //StartPlotting();
        }

        private void UploadWeigthsMatrix()
        {
            // Weights of Inputs to Hidden Layer 1
            var reader = new MatReader("C:/Users/Alexandre/Desktop/Arquivos 2019/UFU/MESTRADO/VERSÂO FINAL/Dados coletados/Weights ANN/19-12-19/w1.mat");
            weigths_L1 = reader.Read<double[,]>("w1");
            // Weights of Hidden Layer 1 to Hidden Layer 2
            reader = new MatReader("C:/Users/Alexandre/Desktop/Arquivos 2019/UFU/MESTRADO/VERSÂO FINAL/Dados coletados/Weights ANN/19-12-19/w2.mat");
            weigths_L2 = reader.Read<double[,]>("w2");
            // Weights of Hidden Layer 2 to Output Layer
            reader = new MatReader("C:/Users/Alexandre/Desktop/Arquivos 2019/UFU/MESTRADO/VERSÂO FINAL/Dados coletados/Weights ANN/19-12-19/w3.mat");
            weigths_Output = reader.Read<double[,]>("w3");
            // Bias Hidden Layer 1
            reader = new MatReader("C:/Users/Alexandre/Desktop/Arquivos 2019/UFU/MESTRADO/VERSÂO FINAL/Dados coletados/Weights ANN/19-12-19/b1.mat");
            bias_L1 = reader.Read<double[,]>("b1");
            // Bias Hidden Layer 2
            reader = new MatReader("C:/Users/Alexandre/Desktop/Arquivos 2019/UFU/MESTRADO/VERSÂO FINAL/Dados coletados/Weights ANN/19-12-19/b2.mat");
            bias_L2 = reader.Read<double[,]>("b2");
            // Bias Output Layer
            reader = new MatReader("C:/Users/Alexandre/Desktop/Arquivos 2019/UFU/MESTRADO/VERSÂO FINAL/Dados coletados/Weights ANN/19-12-19/b3.mat");
            bias_Output = reader.Read<double[,]>("b3");
        }

        private void Set_Aveilable_Ports()
        {
            this.UpdatePortList();
            mainMetroTabControl.SelectedTab = mainMetroTabControl.TabPages[0];
            try
            {
                Controller_Serial.Board.PortName = comboBox_serialPort.SelectedItem.ToString();
                Controller_Serial.Board.PortName = cb_SerialPorts_Alex.SelectedItem.ToString();
            }
            catch (Exception)
            {
                comboBox_serialPort.Items.Add("No Port");
                comboBox_serialPort.SelectedIndex = 0;

                cb_SerialPorts_Alex.Items.Add("No Port");
                cb_SerialPorts_Alex.SelectedIndex = 0;
            }
            button_conectar.BackgroundImage = buttons_imageList.Images["Disconnected"];
            button_LED.BackgroundImage = buttons_imageList.Images["LightOff"];
        }

        private void StartPlotting()
        {
            // Limpar qualquerq chartArea ou Series que já existe no gráfico
            graph_Eletreto.ChartAreas.Clear(); //Limpa as áreas
            graph_Eletreto.Series.Clear(); //Limpa as séries
            graph_Pression_Sensors.ChartAreas.Clear();
            graph_Pression_Sensors.Series.Clear();

            // Cria uma nova séries com um nome específico que serve para identificaçãO
            electret_signal = new Series("Eletreto_Signal");
            pression_1_signal = new Series("Pression_1_Signal");
            pression_2_signal = new Series("Pression_2_Signal");
            // Definindo o tipo de série 
            // FastLine garante melhor desempenho
            electret_signal.ChartType = SeriesChartType.FastLine; //fastline para desenhar mais rápido, vai interpolar os pontos e gera uma linha/curva
            electret_signal.BorderWidth = 2; //espessura
            pression_1_signal.ChartType = SeriesChartType.FastLine;
            pression_1_signal.BorderWidth = 2;
            pression_2_signal.ChartType = SeriesChartType.FastLine;
            pression_2_signal.BorderWidth = 2;
            // Adiciona a nova série ao gráfico
            graph_Eletreto.Series.Add(electret_signal);
            graph_Pression_Sensors.Series.Add(pression_1_signal);
            graph_Pression_Sensors.Series.Add(pression_2_signal);

            //Cria uma nova chartArea com um nome específico
            //ELETRETO
            graphArea_Electret = new ChartArea("Eletreto");
            //Configurando os nomes do eixos
            graphArea_Electret.AxisX.Title = "Time (s)";
            graphArea_Electret.AxisY.Title = "Amplitude (V)";
            //Configurando os limites do gráfico
            graphArea_Electret.AxisX.Minimum = 0; //min X
            graphArea_Electret.AxisX.Maximum = 1; //max x
            // window_plot_Size = graphArea_Electret.AxisX.Maximum - graphArea_Electret.AxisX.Minimum; //Continuar com o mesmo tamanho de janela
            //graphArea_Electret.AxisY.Minimum = 0;//min y
            //graphArea_Electret.AxisY.Maximum = 1;//max y
            //Adiciona a nova área ao gráfico
            graph_Eletreto.ChartAreas.Add(graphArea_Electret);

            //SENSORES DE PRESSÃO
            graphArea_Pression = new ChartArea("Pression Sensors");
            //Configurando os nomes do eixos
            graphArea_Pression.AxisX.Title = "Time (s)";
            graphArea_Pression.AxisY.Title = "Amplitude (V)";
            //Configurando os limites do gráfico
            graphArea_Pression.AxisX.Minimum = 0; //min X
            graphArea_Pression.AxisX.Maximum = 1;//max x
            // window_plot_Size = graphArea_Pression.AxisX.Maximum - graphArea_Pression.AxisX.Minimum; //Continuar com o mesmo tamanho de janela
            graphArea_Pression.AxisY.Minimum = -0.1;//min y
            graphArea_Pression.AxisY.Maximum = 1.1;//max y
            //Adiciona a nova área ao gráfico
            graph_Pression_Sensors.ChartAreas.Add(graphArea_Pression);
        }

        #region UI buttons
        private void button_inicio_Click(object sender, EventArgs e)
        {
            mainMetroTabControl.SelectedTab = mainMetroTabControl.TabPages[0];
        }

        private void button_controll_Click(object sender, EventArgs e)
        {
            mainMetroTabControl.SelectedTab = mainMetroTabControl.TabPages[1];
        }

        private void button_config_Click(object sender, EventArgs e)
        {
            mainMetroTabControl.SelectedTab = mainMetroTabControl.TabPages[2];
        }

        private void button_minimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button_close_Click(object sender, EventArgs e)
        {
            // Parada das Threads
            flagLife = false;

            runThAq = false;
            runThPlot = false;
            runThSave = false;

            thrAq.Abort();
            thrGetBytes.Abort();
            thrPlot_el.Abort();
            thrPlot_pr.Abort();
            thrSave.Abort();
            thrLabels.Abort();
            flagStart = false;

            // Fechamento da porta Serial
            if (Controller_Serial.Board.IsOpen)
            {
                Controller_Serial.Board.Close();
            }
            this.Close();
        }

        private void button_facebook_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://www.facebook.com/keatech");
            }
            catch { }
        }

        private void button_twitter_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://twitter.com/keatech");
            }
            catch { }
        }

        private void button_youtube_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://www.youtube.com/channel/UChabKVWNcH3YwjH53hB_Haw");
            }
            catch { }
        }

        private void button_conectar_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(Controller_Serial.Board.PortName);
            System.Diagnostics.Debug.WriteLine(comboBox_serialPort.SelectedItem.ToString());
            Connect();
        }

        private void button_LED_Click(object sender, EventArgs e)
        {
            ToggleLed();
        }

        private void comboBox_serialPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            Controller_Serial.Board.PortName = comboBox_serialPort.SelectedItem.ToString();
            System.Diagnostics.Debug.WriteLine(Controller_Serial.Board.PortName);
        }
        #endregion

        #region UI buttons callback functions
        private void UpdatePortList()
        {
            // Clear former listed ports and add new ones in combobox list
            // The last port encountered port is selected
            comboBox_serialPort.Items.Clear();
            cb_SerialPorts_Alex.Items.Clear();
            int counter = 0;
            foreach (string port in SerialPort.GetPortNames())
            {
                comboBox_serialPort.Items.Add(port);
                cb_SerialPorts_Alex.Items.Add(port);
                comboBox_serialPort.SelectedIndex = counter;
                counter++;
            }
        }

        private void Connect()
        {
            if (!Controller_Serial.OpenConnection())
            {
                button_conectar.BackgroundImage = buttons_imageList.Images["Disconnected"];
            }
            else
            {
                if (Controller_Serial.Handshake)
                    button_conectar.BackgroundImage = buttons_imageList.Images["Connected"];
                else
                    button_conectar.BackgroundImage = buttons_imageList.Images["Disconnected"];
            }
        }

        private void ToggleLed()
        {
            button_LED.Visible = false;
            if (Controller_Serial.ToggleLED())
            {
                if (Controller_Serial.LedState)
                    button_LED.BackgroundImage = buttons_imageList.Images["LightOn"];
                else
                    button_LED.BackgroundImage = buttons_imageList.Images["LightOff"];
                button_LED.Visible = true;
            }
        }
        #endregion

        #region Motor Control

        #region X related commands
        private void Button_XF_MouseDown(object sender, MouseEventArgs e)
        {
            Controller_Serial.MoveCommand(SerialCommands.CMD_X_BCK, SerialCommands.CO_X_BCK);
        }

        private void Button_XF_MouseUp(object sender, MouseEventArgs e)
        {
            Controller_Serial.MoveCommand(SerialCommands.CMD_X_STOP, SerialCommands.CO_X_STOP);
        }

        private void Button_XB_MouseDown(object sender, MouseEventArgs e)
        {
            Controller_Serial.MoveCommand(SerialCommands.CMD_X_FWD, SerialCommands.CO_X_FWD);
        }

        private void Button_XB_MouseUp(object sender, MouseEventArgs e)
        {
            Controller_Serial.MoveCommand(SerialCommands.CMD_X_STOP, SerialCommands.CO_X_STOP);
        }

        private void Button_XmoveN_MouseClick(object sender, MouseEventArgs e)
        {
            var steps = textBox_moveX.Text;

            bool isValid = Toolbox.HelpfulFunctions.ValidateInt32(steps);

            if (isValid)
            {
                Int32.TryParse(steps, out Int32 N);
                Controller_Serial.MoveNSteps(N, SerialCommands.CMD_X_STEPS, SerialCommands.CO_X_STEPS);
            }
            else
            {
                MessageBox.Show("Entrada inválida!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Y related commands
        private void Button_YmoveN_Click(object sender, EventArgs e)
        {
            var steps = textBox_moveY.Text;

            bool isValid = Toolbox.HelpfulFunctions.ValidateInt32(steps);

            if (isValid)
            {
                Int32.TryParse(steps, out Int32 N);
                Controller_Serial.MoveNSteps(N, SerialCommands.CMD_Y_STEPS, SerialCommands.CO_Y_STEPS);
            }
            else
            {
                MessageBox.Show("Entrada inválida!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Button_YB_MouseDown(object sender, MouseEventArgs e)
        {
            Controller_Serial.MoveCommand(SerialCommands.CMD_Y_BCK, SerialCommands.CO_Y_BCK);
        }

        private void Button_YB_MouseUp(object sender, MouseEventArgs e)
        {
            Controller_Serial.MoveCommand(SerialCommands.CMD_Y_STOP, SerialCommands.CO_Y_STOP);
        }

        private void Button_YF_MouseDown(object sender, MouseEventArgs e)
        {
            Controller_Serial.MoveCommand(SerialCommands.CMD_Y_FWD, SerialCommands.CO_Y_FWD);
        }

        private void Button_YF_MouseUp(object sender, MouseEventArgs e)
        {
            Controller_Serial.MoveCommand(SerialCommands.CMD_Y_STOP, SerialCommands.CO_Y_STOP);
        }
        #endregion

        #region Z related commands 
        private void Button_ZB_MouseDown(object sender, MouseEventArgs e)
        {
            //Controller_Serial.MoveCommand(SerialCommands.CMD_Z_FWD, SerialCommands.CO_Z_FWD);
        }

        private void Button_ZB_MouseUp(object sender, MouseEventArgs e)
        {
            Controller_Serial.MoveCommand(SerialCommands.CMD_Z_STOP, SerialCommands.CO_Z_STOP);
        }

        private void Button_ZF_MouseDown(object sender, MouseEventArgs e)
        {
            //Controller_Serial.MoveCommand(SerialCommands.CMD_Z_BCK, SerialCommands.CO_Z_BCK);            
        }

        private void Button_ZF_MouseUp(object sender, MouseEventArgs e)
        {
            Controller_Serial.MoveCommand(SerialCommands.CMD_Z_STOP, SerialCommands.CO_Z_STOP);
        }

        private void Button_ZmoveN_Click(object sender, EventArgs e)
        {
            var steps = textBox_moveZ.Text;

            bool isValid = Toolbox.HelpfulFunctions.ValidateInt32(steps);

            if (isValid)
            {
                Int32.TryParse(steps, out Int32 N);
                Controller_Serial.MoveNSteps(N, SerialCommands.CMD_Z_STEPS, SerialCommands.CO_Z_STEPS);
            }
            else
            {
                MessageBox.Show("Entrada inválida!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region P related commands
        private void Button_PmoveN_Click(object sender, EventArgs e)
        {
            var steps = textBox_moveP.Text;

            bool isValid = Toolbox.HelpfulFunctions.ValidateInt32(steps);

            if (isValid)
            {
                Int32.TryParse(steps, out Int32 N);
                Controller_Serial.MoveNSteps(N, SerialCommands.CMD_P_STEPS, SerialCommands.CO_P_STEPS);
            }
            else
            {
                MessageBox.Show("Entrada inválida!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Button_PB_MouseDown(object sender, MouseEventArgs e)
        {
            Controller_Serial.MoveCommand(SerialCommands.CMD_P_BCK, SerialCommands.CO_P_BCK);
        }

        private void Button_PB_MouseUp(object sender, MouseEventArgs e)
        {
            Controller_Serial.MoveCommand(SerialCommands.CMD_P_STOP, SerialCommands.CO_P_STOP);
        }

        private void Button_PF_MouseDown(object sender, MouseEventArgs e)
        {
            Controller_Serial.MoveCommand(SerialCommands.CMD_P_FWD, SerialCommands.CO_P_FWD);
        }

        private void Button_PF_MouseUp(object sender, MouseEventArgs e)
        {
            Controller_Serial.MoveCommand(SerialCommands.CMD_P_STOP, SerialCommands.CO_P_STOP);
        }
        #endregion

        #region T related commands
        private void Button_TmoveN_Click(object sender, EventArgs e)
        {
            var steps = textBox_moveT.Text;

            bool isValid = Toolbox.HelpfulFunctions.ValidateInt32(steps);

            if (isValid)
            {
                Int32.TryParse(steps, out Int32 N);
                Controller_Serial.MoveNSteps(N, SerialCommands.CMD_T_STEPS, SerialCommands.CO_T_STEPS);
            }
            else
            {
                MessageBox.Show("Entrada inválida!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Button_TB_MouseDown(object sender, MouseEventArgs e)
        {
            Controller_Serial.MoveCommand(SerialCommands.CMD_T_BCK, SerialCommands.CO_T_BCK);
        }

        private void Button_TB_MouseUp(object sender, MouseEventArgs e)
        {
            Controller_Serial.MoveCommand(SerialCommands.CMD_T_STOP, SerialCommands.CO_T_STOP);
        }

        private void Button_TF_MouseDown(object sender, MouseEventArgs e)
        {
            Controller_Serial.MoveCommand(SerialCommands.CMD_T_FWD, SerialCommands.CO_T_FWD);
        }

        private void Button_TF_MouseUp(object sender, MouseEventArgs e)
        {
            Controller_Serial.MoveCommand(SerialCommands.CMD_T_STOP, SerialCommands.CO_T_STOP);
        }
        #endregion

        #region Special commands
        private void bunifuSwitch_spindle_Click(object sender, EventArgs e)
        {
            Controller_Serial.Spindle();
            bunifuSwitch_spindle.Value = Controller_Serial.SpindleState;
        }

        private void bunifuSwitch_circle_Click(object sender, EventArgs e)
        {
            Controller_Serial.Circle();
            bunifuSwitch_circle.Value = Controller_Serial.DrillState;
        }
        #endregion

        #endregion
               

        void GetDataInBytes() 
        {
            if (fifoBytes.Pop_One(ref Byte))
            {
                if (Byte == 0x7E) // && (Byte[packageSize-1] == 0x81)) // StartByte: 126 & StopByte: 129
                {
                    fifoBytes.Pop_Package(ref dataBytes);  // Get the entire package
                                                           /*
                                                            * TABELA DAS DADOS REFERENTES ÀS POSIÇÕES DO ARRAY dataBytes:
                                                           StartByte = Byte;
                                                           LSB_el  = dataBytes[0];             MSB_el  = dataBytes[1];
                                                           LSB_p_1 = dataBytes[2];             MSB_p_1 = dataBytes[3];
                                                           LSB_p_2 = dataBytes[4];             MSB_p_2 = dataBytes[5];
                                                           LSB_pI  = dataBytes[6];             MSB_pI  = dataBytes[7];
                                                           soma_valores_MSBs = dataBytes[8];   
                                                           StopByte          = dataBytes[9];
                                                           */
                    int CheckSum = dataBytes[1] + dataBytes[3] + dataBytes[5]; // + dataBytes[7];
                    if ((dataBytes[dataBytes.Length - 1] == 0x81) && (CheckSum == dataBytes[dataBytes.Length - 2])) // Check StopByte (129) e CheckSum 
                    {
                        int ampElectret = (dataBytes[1] << 8) + dataBytes[0];
                        int ampPression_1 = (dataBytes[3] << 8) + dataBytes[2];
                        int ampPression_2 = (dataBytes[5] << 8) + dataBytes[4];
                        int ampPhotoInterruptor = (dataBytes[7] << 8) + dataBytes[6];

                        double el_signal = ampElectret / ampScale;
                        double p_1_signal = ampPression_1 / ampScale;
                        double p_2_signal = ampPression_2 / ampScale;
                        double ph_int_signal = ampPhotoInterruptor / ampScale; 

                        // Passagem do pacote de dados pro Main Buffer Circular
                        y = new double[5];
                        y[0] = Math.Round(countSerial, 3);
                        y[1] = el_signal;
                        y[2] = p_1_signal;
                        y[3] = p_2_signal;
                        y[4] = ph_int_signal;

                        //tempo do arduino 
                        countSerial += dt;
                        BC.Push(y);
                    }
                    else // if the package is not entirely correct, push it back to the end of the line
                        fifoBytes.Push_Back_to_the_End();
                }
            }
        }
        

        #region Threads
        #region THREAD: Função para aquisição dos Bytes
        private void Acquisition()
        {
            while (flagLife)
            {
                while (runThAq)
                {
                    //mutex.WaitOne();

                    bytesToRead = Controller_Serial.Board.BytesToRead;
                    for(int ii=0; ii<bytesToRead; ii++)
                    {
                        fifoBytes.Push(Controller_Serial.Board.ReadByte());
                    }
                    GetDataInBytes();

                    //mutex.ReleaseMutex();
                }
            }
        }
        #endregion
        #region THREAD: Função junção dos dados vindos da Serial
        private void GetDataIntoBytes()
        {
            while (flagLife)
            {
                while (runThAq)
                {
                    if (fifoBytes.Pop_One(ref Byte))
                    {
                        if (Byte == 0x7E) // && (Byte[packageSize-1] == 0x81)) // StartByte: 126 & StopByte: 129
                        {
                            fifoBytes.Pop_Package(ref dataBytes);  // Get the entire package
                            /*
                             * TABELA DAS DADOS REFERENTES ÀS POSIÇÕES DO ARRAY dataBytes:
                            StartByte = Byte;
                            LSB_el  = dataBytes[0];             MSB_el  = dataBytes[1];
                            LSB_p_1 = dataBytes[2];             MSB_p_1 = dataBytes[3];
                            LSB_p_2 = dataBytes[4];             MSB_p_2 = dataBytes[5];
                            LSB_pI  = dataBytes[6];             MSB_pI  = dataBytes[7];
                            soma_valores_MSBs = dataBytes[8];   
                            StopByte          = dataBytes[9];
                            */
                            int CheckSum = dataBytes[1] + dataBytes[3] + dataBytes[5] + dataBytes[7];
                            if ((dataBytes[dataBytes.Length - 1] == 0x81) && (CheckSum == dataBytes[dataBytes.Length - 2])) // Check StopByte (129) e CheckSum 
                            {
                                int ampElectret = (dataBytes[1] << 8) + dataBytes[0];
                                int ampPression_1 = (dataBytes[3] << 8) + dataBytes[2];
                                int ampPression_2 = (dataBytes[5] << 8) + dataBytes[4];
                                int ampPhotoInterruptor = (dataBytes[7] << 8) + dataBytes[6];

                                double el_signal = ampElectret / ampScale;
                                double p_1_signal = ampPression_1 / ampScale;
                                double p_2_signal = ampPression_2 / ampScale;
                                double ph_int_signal = ampPhotoInterruptor / ampScale;

                                // Passagem do pacote de dados pro Main Buffer Circular
                                y = new double[5];
                                y[0] = Math.Round(countSerial, 3);
                                y[1] = el_signal;
                                y[2] = p_1_signal;
                                y[3] = p_2_signal;
                                y[4] = ph_int_signal;

                                //tempo do arduino 
                                countSerial += dt;
                                BC.Push(y);
                            }
                            else // if the package is not entirely correct, push it back to the end of the line
                                fifoBytes.Push_Back_to_the_End();
                        }
                    }
                }
            }
        }
        #endregion
        #region THREAD: Função para classificação do sinal pela RNA
        private void Classification()
        {
            while (flagLife)
            {
                while (runThClass)
                {
                    if (BC.Pop_Classifier(ref window))
                    {
                            if (window[79] > 0.16) // Wait for pression aplied to start the classifier
                            {
                                state = ANN_MLP.OutputANN(window);
                                lbSlipState.Invoke(new Action(() => lbSlipState.Text = state.ToString()));
                                if (state) // With Slippage detected
                                {
                                    // Move motor foward N steps
                                    byte[] sendByte = { 0x0B };
                                    Controller_Serial.Board.Write(sendByte, 0, 1);
                                    //Controller_Serial.MoveCommand(SerialCommands.CMD_X_FWD, SerialCommands.CO_X_FWD);
                                }
                                else
                                {
                                    // Move backwark N steps
                                    byte[] sendByte = { 0x0C };
                                    Controller_Serial.Board.Write(sendByte, 0, 1);
                                    Controller_Serial.MoveCommand(SerialCommands.CMD_X_STOP, SerialCommands.CMD_X_STOP);
                                }
                            }   
                            else
                            {
                                // Move motor foward
                                byte[] sendByte = { 0x0D };
                                Controller_Serial.Board.Write(sendByte, 0, 1);
                            }
                    }
                }
            }
        }
        #endregion
        #region THREAD: Função para plotagem do sinal do ELETRETO
                    private void Plotting_electret()
        {
            while (flagLife)
            {
                while (runThPlot)
                {
                    if (BC.Pop_Plot_electret(ref y_plot_el))
                    {
                        //setSignal(y[0], y[1], y[2], y[3]); //Adiciono o novo vetor gerado (eixo x, eixo y)
                        graph_Eletreto.Invoke( new Action( () => 
                        {
                            if (y_plot_el[0] > graphArea_Electret.AxisX.Maximum)  // Desloca a janela para a direita
                            {
                                electret_signal.Points.RemoveAt(0); // Removo o primeiro ponto da série
                                graphArea_Electret.AxisX.Maximum = Math.Round(y_plot_el[0], 1); // Faço com que o máximo do eixo x seja o próprio contador
                                graphArea_Electret.AxisX.Minimum = Math.Round(graphArea_Electret.AxisX.Maximum - window_plot_Size, 1); // O mínimo passa a ser o máximo menos a largura inicial
                            }
                            electret_signal.Points.AddXY(y_plot_el[0], y_plot_el[1]);
                        }));
                    }
                }
            }
        }
        #endregion
        #region THREAD: Função para plotagem do sinal dos SENSORES DE PRESSÃO
        private void Plotting_pression()
        {
            while (flagLife)
            {
                while (runThPlot)
                {
                    if (BC.Pop_Plot_pression(ref y_plot_pr))
                    {
                        graph_Pression_Sensors.Invoke(new Action(() =>
                        {
                            if (y_plot_pr[0] > graphArea_Pression.AxisX.Maximum)
                            {
                                pression_1_signal.Points.RemoveAt(0);
                                pression_2_signal.Points.RemoveAt(0);
                                graphArea_Pression.AxisX.Maximum = Math.Round(y_plot_pr[0], 1);
                                graphArea_Pression.AxisX.Minimum = Math.Round(graphArea_Pression.AxisX.Maximum - window_plot_Size, 1);
                            }
                            pression_1_signal.Points.AddXY(y_plot_pr[0], y_plot_pr[2]);
                            pression_2_signal.Points.AddXY(y_plot_pr[0], y_plot_pr[3]);
                        }));
                    }
                }
            }
        }
        #endregion 
        #region THREAD: Função para Salvamento
        private void Saving()
        {
            while (flagLife)
            {
                while (runThSave)
                {
                    if (BC.Pop_Save(ref dataToSave))
                    {
                        line_table = new String[dataToSave.Length];
                        // Criar String com os dados                         
                        for (int ii = 0; ii < dataToSave.Length; ii++)
                        {
                            //line_table[ii] += Convert.ToString(Math.Round(dataToSave[ii][0], 3)) + " " + Convert.ToString(dataToSave[ii][1]) + " " + Convert.ToString(dataToSave[ii][2]) + " " + Convert.ToString(dataToSave[ii][3]) + " " + Convert.ToString(dataToSave[ii][4]);
                            line_table[ii] += Math.Round(dataToSave[ii][0], 3) + " " + dataToSave[ii][1] + " " + dataToSave[ii][2] + " " + dataToSave[ii][3] + " " + dataToSave[ii][4];
                        }
                        // Salvamento dos dados num arquivo .txt
                        System.IO.File.AppendAllLines("C:/Users/BioLAB2/Documents/Alexandre Henrique/Dados coletados/11-07/bola_bilhar_Exp5.txt", line_table);
                    }
                }
            }
        }
        #endregion
        #region THREAD: Função para mostrar os Labels
        private void Show_Labels()
        {
            while (flagLife)
            {
                while (runThLabels)
                {
                    if (BC.Pop_Show_Labels(ref dataToShow))
                    { 
                        //lbSlipState.Invoke( new Action( () => lbSlipState.Text = Math.Round(dataToShow[4], 3).ToString() ) );
                        lbTempo.Invoke( new Action( () => lbTempo.Text = dataToShow[0].ToString() ) );
                        lbBytestoRead.Invoke( new Action( () => lbBytestoRead.Text = bytesToRead.ToString() ) );
                        lbCountOverFlow.Invoke( new Action( () => lbCountOverFlow.Text = BC.countOverFlow.ToString() ) );
                        lbEspBuf.Invoke( new Action( () => lbEspBuf.Text = BC.sampleCount.ToString() ) );

                        //setText_label_phot_int(Math.Round(dataToShow[4],3).ToString());
                        //setText_label_tempo(Math.Round(BC.countTime, 3).ToString());
                        //setText_label_bytesToRead(bytesToRead.ToString());
                        //setText_label_CountOverFlow(BC.countOverFlow.ToString());
                        //setText_label_lbEspBuf(BC.sampleCount.ToString());                        
                    }

                }
            }
        }
        #endregion

        #endregion

        
        #region Delegates 

        // (para acessar métodos de interface gráfica de threads diferentes)
        #region Delegate Label_BytestoRead
        //delegate que encapsula um métodos
        //duas propriedade q esta declarando, vai estanciar nessa funçao em cima setText_label_bytesToRead
        public delegate void setText_label_bytesToRead_delegate(string s);
        public setText_label_bytesToRead_delegate delegateHandler_setTextLabelbytesToRead;

        //delegate para acesso ao label lbBytestoRead
        public void setText_label_bytesToRead(string s)
        {
            //check if this method is running on a different thread
            //than the thread that created the control
            if (lbBytestoRead.InvokeRequired)
            {
                //instanciando o delegado
                delegateHandler_setTextLabelbytesToRead = new setText_label_bytesToRead_delegate(setTextLabelBytesToRead);
                //invoka o delegado
                lbBytestoRead.BeginInvoke(delegateHandler_setTextLabelbytesToRead, new object[] { s });
            }
            else
                lbBytestoRead.Text = s;
        }
        private void setTextLabelBytesToRead(string s)
        {
            lbBytestoRead.Text = s;
        }
        #endregion
        #region Delegate Label_Ocupação_Buffer
        //delegate que encapsula um métodos
        //duas propriedade q esta declarando
        public delegate void setText_label_lbEspBuf_delegate(string s);
        public setText_label_lbEspBuf_delegate delegateHandler_setTextLabellbEspBuf;

        public void setText_label_lbEspBuf(string s)
        {
            //check if this method is running on a different thread
            //than the thread that created the control
            if (lbEspBuf.InvokeRequired)
            {
                //instanciando o delegado
                delegateHandler_setTextLabellbEspBuf = new setText_label_lbEspBuf_delegate(setTextLabellbEspBuf);
                //invoka o delegado
                lbEspBuf.BeginInvoke(delegateHandler_setTextLabellbEspBuf, new object[] { s });
            }
            else
                lbEspBuf.Text = s;
        }
        private void setTextLabellbEspBuf(string s)
        {
            lbEspBuf.Text = s;
        }
        #endregion
        #region Delegate Label_Photo_Interruptor
        //delegate que encapsula um métodos
        //duas propriedade q esta declarando, vai estanciar nessa funçao em cima setText_label_bytesToRead
        public delegate void setText_label_ph_int_delegate(string s);
        public setText_label_ph_int_delegate delegateHandler_setTextPhotInt;
        private void setTextLabelPhotInt(string s)
        {
            lbSlipState.Text = s;
        }

        public void setText_label_phot_int(string s)
        {
            //check if this method is running on a different thread
            //than the thread that created the control
            if (lbSlipState.InvokeRequired)
            {
                //instanciando o delegado
                delegateHandler_setTextPhotInt = new setText_label_ph_int_delegate(setTextLabelPhotInt);
                //invoka o delegado
                lbSlipState.BeginInvoke(delegateHandler_setTextPhotInt, new object[] { s });
            }
            else
                lbSlipState.Text = s;
        }
        #endregion
        #region Delegate Label_Tempo
        //delegate que encapsula um métodos
        //duas propriedade q esta declarando, vai estanciar nessa funçao em cima setText_label_bytesToRead
        public delegate void setText_label_tempo_delegate(string s);
        public setText_label_tempo_delegate delegateHandler_setTextTempo;
        private void setTextLabeltempo(string s)
        {
            lbTempo.Text = s;
        }

        public void setText_label_tempo(string s)
        {
            //check if this method is running on a different thread
            //than the thread that created the control
            if (lbSlipState.InvokeRequired)
            {
                //instanciando o delegado
                delegateHandler_setTextTempo = new setText_label_tempo_delegate(setTextLabeltempo);
                //invoka o delegado
                lbTempo.BeginInvoke(delegateHandler_setTextTempo, new object[] { s });
            }
            else
                lbTempo.Text = s;
        }
        #endregion
        #region Delegate Label_Count_OverFlow
        //delegate que encapsula um métodos
        //duas propriedade q esta declarando, vai estanciar nessa funçao em cima setText_label_bytesToRead
        public delegate void setText_label_countOverFlow_delegate(string s);
        public setText_label_countOverFlow_delegate delegateHandler_setTextCountOverFlow;
        private void setTextLabelCountOverFlow(string s)
        {
            lbCountOverFlow.Text = s;
        }

        private void Button_XB_KeyDown(object sender, KeyEventArgs e)
        {
            Controller_Serial.MoveCommand(SerialCommands.CMD_X_FWD, SerialCommands.CO_X_FWD);
        }

        private void Button_XB_KeyUp(object sender, KeyEventArgs e)
        {
            Controller_Serial.MoveCommand(SerialCommands.CMD_X_STOP, SerialCommands.CO_X_STOP);
        }

        private void Button_XF_KeyDown(object sender, KeyEventArgs e)
        {
            Controller_Serial.MoveCommand(SerialCommands.CMD_X_BCK, SerialCommands.CO_X_BCK);
        }

        private void Button_XF_KeyUp(object sender, KeyEventArgs e)
        {
            Controller_Serial.MoveCommand(SerialCommands.CMD_X_STOP, SerialCommands.CO_X_STOP);
        }

        private void btn_Reverse_Click(object sender, EventArgs e)
        {
            // Move motor backward
            byte[] sendByte = { 2 };
            Controller_Serial.Board.Write(sendByte, 0, 1);
            //Controller_Serial.MoveCommand(SerialCommands.CMD_X_BCK, SerialCommands.CO_X_BCK);          
        }

        public void setText_label_CountOverFlow(string s)
        {
            //check if this method is running on a different thread
            //than the thread that created the control
            if (lbCountOverFlow.InvokeRequired)
            {
                //instanciando o delegado
                delegateHandler_setTextCountOverFlow = new setText_label_countOverFlow_delegate(setTextLabelCountOverFlow);
                //invoka o delegado
                lbCountOverFlow.BeginInvoke(delegateHandler_setTextCountOverFlow, new object[] { s });
            }
            else
                lbCountOverFlow.Text = s;
        }
        #endregion
        #region Delegate do Sinal do Chart
        public delegate void setSignal_delegate(double x, double s_e, double s_p1, double s_p2);
        public setSignal_delegate delegateHandler_setSignal;
        private void set_Signal(double t, double s_e, double s_p1, double s_p2)
        {
            //Se o contador/tempo de amostragem for maior que o máximo do eixo X
            if (t > graphArea_Electret.AxisX.Maximum)
            {
                // Eletreto
                electret_signal.Points.RemoveAt(0); // Removo o primeiro ponto da série
                graphArea_Electret.AxisX.Maximum = Math.Round(t, 1); // Faço com que o máximo do eixo x seja o próprio contador
                graphArea_Electret.AxisX.Minimum = Math.Round(graphArea_Electret.AxisX.Maximum - window_plot_Size, 1); // O mínimo passa a ser o máximo menos a largura inicial
                // Sensores de Pressão FSR
                pression_1_signal.Points.RemoveAt(0); // Removo o primeiro ponto da série
                pression_2_signal.Points.RemoveAt(0); // Removo o primeiro ponto da série
                graphArea_Pression.AxisX.Maximum = Math.Round(t, 1); // Faço com que o máximo do eixo x seja o próprio contador
                graphArea_Pression.AxisX.Minimum = Math.Round(graphArea_Pression.AxisX.Maximum - window_plot_Size, 1); // O mínimo passa a ser o máximo menos a largura inicial
            }
            electret_signal.Points.AddXY(t, s_e);
            pression_1_signal.Points.AddXY(t, s_p1);
            pression_2_signal.Points.AddXY(t, s_p2);
        }
        #endregion

        #endregion
                
        #region Botões da Form

        #region Botão Start
        private void btn_Start_Click(object sender, EventArgs e)
        {
            Controller_Serial.Board.DiscardOutBuffer();
            Controller_Serial.Board.DiscardInBuffer();

            if (flagStart)
            {
                flagLife = true;

                runThAq = true;
                runThClass = true;
                //runThPlot = true;
                //runThSave = true;
                runThLabels = true;

                Thread.Sleep(1);

                thrAq.Start();
                thrClass.Start();
                //thrPlot_el.Start();
                //thrPlot_pr.Start();
                thrLabels.Start();
                flagStart = true;

                stopwatch.Start();
            }

            //byte[] sendByte = {4};
            //Controller_Serial.Board.Write(sendByte, 0, 1);

            // Start motor until it detects some pression, than it stops
            //Controller_Serial.MoveCommand(SerialCommands.CMD_X_FWD, SerialCommands.CO_X_FWD);

            
                       
            //Controller_Serial.Board.Write("Start");
        }
        #endregion
        #region Botão Stop
        private void btn_Stop_Click(object sender, EventArgs e)
        {
            /*
            flagLife = false;

            runThAq = false;
            runThClass = false;
            runThPlot = false;
            runThSave = false;

            thrAq.Abort();
            thrClass.Abort();
            thrGetBytes.Abort();
            thrPlot_el.Abort();
            thrPlot_pr.Abort();
            thrSave.Abort();
            thrLabels.Abort();
            flagStart = false;
            

            // Stop motor 
            byte[] sendByte = { 3 };
            Controller_Serial.Board.Write(sendByte,0,1); // Write("Stop");
            //Controller_Serial.MoveCommand(SerialCommands.CMD_X_STOP, SerialCommands.CMD_X_STOP);
            
            
            
            stopwatch.Stop();
            //if (Controller_Serial.Board.IsOpen)
            //{
            //   Controller_Serial.Board.Close();
            //}
            lb_elapsed_time.Text = "Time elapsed: {0}" + stopwatch.Elapsed;
            */

            runThAq = false;
            runThClass = false;
            runThPlot = false;
            runThSave = false;
            runThLabels = false;

            byte[] sendByte = { 0x0A };
            Controller_Serial.Board.Write(sendByte, 0, 1); // Write("Stop");

            stopwatch.Stop();
            lb_elapsed_time.Text = "Time elapsed: {0}" + stopwatch.Elapsed;
        }
        #endregion
        #region Botão Connect
        private void btn_Conect_Alex_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(Controller_Serial.Board.PortName);
            System.Diagnostics.Debug.WriteLine(cb_SerialPorts_Alex.SelectedItem.ToString());
            Connect();
            btn_Conect_Alex.Text = "Connected";
        }
        #endregion

        #endregion

        /*
        #region Setagem do sinal
        public void setSignal(double t, double s_e, double s_p1, double s_p2) // tempo, sinal eletreto, sinal pressão 1, sinal pressão 2 
        {
            //check if this method is running on a different thread
            //than the thread that created the control
            if (graph_Eletreto.InvokeRequired)
            {
                //instanciando o delegado
                delegateHandler_setSignal = new setSignal_delegate(set_Signal);
                //invoka o delegado
                graph_Eletreto.BeginInvoke(delegateHandler_setSignal, new object[] { t, s_e, s_p1, s_p2 });
                graph_Pression_Sensors.BeginInvoke(delegateHandler_setSignal, new object[] { t, s_e, s_p1, s_p2 });
            }
            else
            {
                if (t > graphArea_Electret.AxisX.Maximum)
                {
                    // Eletreto
                    electret_signal.Points.RemoveAt(0); //Removo o primeiro ponto da série
                    graphArea_Electret.AxisX.Maximum = Math.Round(t, 3); //Faço com que o máximo do eixo x seja o próprio contador
                    graphArea_Electret.AxisX.Minimum = Math.Round(graphArea_Electret.AxisX.Maximum - window_plot_Size, 3); //O mínimo passa a ser o máximo menos a largura inicial
                    graphArea_Electret.AxisX.LabelStyle.Format = "#";
                    // Sensores de Pressão FSR
                    pression_1_signal.Points.RemoveAt(0); //Removo o primeiro ponto da série
                    pression_2_signal.Points.RemoveAt(0); //Removo o primeiro ponto da série
                    graphArea_Pression.AxisX.Maximum = Math.Round(t, 3); //Faço com que o máximo do eixo x seja o próprio contador
                    graphArea_Pression.AxisX.Minimum = Math.Round(graphArea_Pression.AxisX.Maximum - window_plot_Size, 3); //O mínimo passa a ser o máximo menos a largura inicial
                    graphArea_Pression.AxisX.LabelStyle.Format = "#";
                }
                electret_signal.Points.AddXY(t, s_e);
                pression_1_signal.Points.AddXY(t, s_p1);
                pression_2_signal.Points.AddXY(t, s_p2);
            }
        }
        #endregion
        */

        private void Button_speed_Click(object sender, EventArgs e)
        {
            Controller_Serial.SetSpeed(normSpeed);
        }

        private void metroTrackBar1_ValueChanged(object sender, EventArgs e)
        {
            label_speed.Text = $"{metroTrackBar1.Value.ToString()} %";
            int value = metroTrackBar1.Value;
            normSpeed = Convert.ToInt16((value - 0) * (0 - 100) / (100 - 0) + 100);
        }


    }
}