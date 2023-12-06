using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

namespace MeteoApp
{
    public partial class Form1 : Form
    {
        private List<DateTime> xValues = new List<DateTime>();
        private List<double> yValuesHumidity = new List<double>();
        private List<double> yValuesTemperature = new List<double>();
        private List<double> yValuesPressure = new List<double>();
        private static bool _continue;
        private static SerialPort _serialPort;
        private Thread ThreadReadComPort;
        private TimeSpan ts1 = new TimeSpan(1, 0, 0);
        private string comPortName;
        private bool connFailure = false;
        private static readonly string sCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string sFile = Path.Combine(sCurrentDirectory, @"..\..\meteo_db.db");
        private static readonly string sFilePath = Path.GetFullPath(sFile);

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckDb();
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = (2 * 1000); // 5 secs
            timer.Tick += new EventHandler(timer1_Tick);
            timer.Start();
            progressBar2.Maximum = 50;
            progressBar3.Maximum = 150;
            foreach (var s in SerialPort.GetPortNames())
            {
                comboBox1.Items.Add(s);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            comPortName = comboBox1.SelectedItem.ToString();
            ThreadReadComPort = new Thread(ReadComPort);
            ThreadReadComPort.Start();
            button1.Visible = false;
            button2.Visible = true;
            label8.Visible = true;
            label9.Visible = false;
            label8.Text = "Connected to port: " + _serialPort.PortName;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _serialPort.Close();
            ThreadReadComPort.Abort();
            button1.Visible = true;
            button2.Visible = false;
            label9.Visible = true;
            label8.Visible = false;
            label9.Text = "Disconnected";
        }

        public void ReadComPort()
        {
            Thread readThread = new Thread(Read);
            _serialPort = new SerialPort
            {
                PortName = SetPortName(comPortName),
                ReadTimeout = 500,
                WriteTimeout = 500
            };
            connFailure = false;
            try
            {
                _serialPort.Open();
                _continue = true;
                readThread.Start();
                readThread.Join();
                _serialPort.Close();
            }
            catch (IOException)
            {
                connFailure = true;
                MessageBox.Show($"Connection to {comPortName} failed.\n" +
                    $"Try another port from the list");
            }
        }

        public void GetData()
        {
            using (var connection = new SqliteConnection($"Data Source={sFilePath}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
               @"SELECT * FROM meteo_data";
                using (var reader = command.ExecuteReader())
                {
                    xValues.Clear();
                    yValuesHumidity.Clear();
                    yValuesTemperature.Clear();
                    yValuesPressure.Clear();
                    int id;
                    string date;
                    float humidity;
                    float temperature;
                    float pressure;
                    while (reader.Read())
                    {
                        id = reader.GetInt32(0);
                        date = reader.GetString(1);
                        humidity = reader.GetFloat(2);
                        temperature = reader.GetFloat(3);
                        pressure = reader.GetFloat(4);
                        long ts1Ticks = ts1.Ticks;
                        DateTime dt1 = new DateTime(DateTime.Now.Ticks - ts1.Ticks);
                        if (dt1 <= DateTime.Parse(date))
                        {
                            xValues.Add(DateTime.Parse(date));
                            yValuesHumidity.Add(humidity);
                            yValuesTemperature.Add(temperature);
                            yValuesPressure.Add(pressure);
                        }
                    }

                    if (xValues.Count != 0)
                    {
                        textBox1.Text = Math.Round(yValuesHumidity[yValuesHumidity.Count - 1], 2).ToString();
                        progressBar1.Value = (int)yValuesHumidity[yValuesHumidity.Count - 1];
                        if (yValuesHumidity[yValuesHumidity.Count - 1] > 60 || yValuesHumidity[yValuesHumidity.Count - 1] < 40)
                        {
                            progressBar1.ForeColor = Color.Red;
                            if (yValuesHumidity[yValuesHumidity.Count - 1] > 60)
                            {
                                label10.Text = "Too high";
                            }
                            else
                            {
                                label10.Text = "Too low";
                            }
                        }
                        else
                        {
                            progressBar1.ForeColor = Color.ForestGreen;
                            label10.Text = "OK";
                        }

                        textBox2.Text = Math.Round(yValuesTemperature[yValuesTemperature.Count - 1], 2).ToString();
                        progressBar2.Value = (int)yValuesTemperature[yValuesTemperature.Count - 1];
                        if (yValuesTemperature[yValuesTemperature.Count - 1] > 28 || yValuesTemperature[yValuesTemperature.Count - 1] < 16)
                        {
                            progressBar2.ForeColor = Color.Red;
                            if (yValuesTemperature[yValuesTemperature.Count - 1] > 28)
                            {
                                label11.Text = "Too high";
                            }
                            else
                            {
                                label11.Text = "Too low";
                            }
                        }
                        else
                        {
                            progressBar2.ForeColor = Color.ForestGreen;
                            label11.Text = "OK";
                        }

                        textBox3.Text = Math.Round(yValuesPressure[yValuesPressure.Count - 1], 2).ToString();
                        progressBar3.Value = (int)yValuesPressure[yValuesPressure.Count - 1];
                        if (yValuesPressure[yValuesPressure.Count - 1] > 98 || yValuesPressure[yValuesPressure.Count - 1] < 94)
                        {
                            progressBar3.ForeColor = Color.Red;
                            if (yValuesPressure[yValuesPressure.Count - 1] > 98)
                            {
                                label12.Text = "Too high";
                            }
                            else
                            {
                                label12.Text = "Too low";
                            }
                        }
                        else
                        {
                            progressBar3.ForeColor = Color.ForestGreen;
                            label12.Text = "OK";
                        }

                        textBox4.Text = xValues[xValues.Count - 1].ToString();
                        chart1.Series["Humidity"].Points.DataBindXY(xValues, yValuesHumidity);
                        chart1.Series["Temperature"].Points.DataBindXY(xValues, yValuesTemperature);
                        chart1.Series["Pressure"].Points.DataBindXY(xValues, yValuesPressure);
                    }
                }
            }
        }

        public void Read()
        {
            MeteoData meteoData = new MeteoData();
            while (_continue)
            {
                try
                {
                    string message = _serialPort.ReadLine();
                    if (string.IsNullOrWhiteSpace(message))
                    {
                        meteoData.DateTime = DateTime.Now;
                        using (var connection = new SqliteConnection($"Data Source={sFilePath}"))
                        {
                            connection.Open();
                            var command = connection.CreateCommand();
                            command.CommandText =
                            @"INSERT INTO meteo_data (time, humidity, temperature, pressure) VALUES($time, $humidity, $temperature, $pressure)";
                            command.Parameters.AddWithValue("$time", meteoData.DateTime);
                            command.Parameters.AddWithValue("$humidity", Math.Round(meteoData.Humidity, 5));
                            command.Parameters.AddWithValue("$temperature", Math.Round(meteoData.Temperature, 5));
                            command.Parameters.AddWithValue("$pressure", Math.Round(meteoData.Pressure, 5));
                            command.ExecuteNonQuery();
                        }

                        throw new ArgumentNullException("message");
                    }
                    else if (message.ToLower().StartsWith("humidity"))
                    {
                        meteoData.Humidity = float.Parse(message.Split(':')[1].Trim().Remove(6).Trim());
                        if (meteoData.Humidity == 0)
                        {
                            continue;
                        }
                    }
                    else if (message.ToLower().StartsWith("temperature"))
                    {
                        meteoData.Temperature = float.Parse(message.Split(':')[1].Trim().Remove(6).Trim());
                        if (meteoData.Temperature == 0)
                        {
                            continue;
                        }
                    }
                    else if (message.ToLower().StartsWith("pressure"))
                    {
                        meteoData.Pressure = float.Parse(message.Split(':')[1].Trim().Remove(6).Trim());
                        if (meteoData.Pressure == 0)
                        {
                            continue;
                        }
                    }
                }
                catch (TimeoutException) { }
                catch (IOException)
                {
                    break;
                }
                catch (IndexOutOfRangeException) { }
                catch (ArgumentNullException)
                {
                    xValues.Add(meteoData.DateTime);
                    yValuesHumidity.Add(meteoData.Humidity);
                    yValuesTemperature.Add(meteoData.Temperature);
                    yValuesPressure.Add(meteoData.Pressure);
                }
            }
        }

        public string SetPortName(string defaultPortName)
        {
            string portName;
            portName = defaultPortName;
            return portName;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (connFailure)
            {
                button2.PerformClick();
            }

            GetData();
            progressBar4.Value = 100;
            if (ThreadReadComPort != null && ThreadReadComPort.IsAlive)
            {
                progressBar4.ForeColor = Color.Green;
                label6.Text = "Data is being collecting";
                label6.ForeColor = Color.Green;
            }
            else
            {
                progressBar4.ForeColor = Color.Red;
                label6.Text = "Press \"Start\" to collect data";
                label6.ForeColor = Color.Black;
            }

            chart1.Series["Humidity"].Points.DataBindXY(xValues, yValuesHumidity);
            chart1.Series["Temperature"].Points.DataBindXY(xValues, yValuesTemperature);
            chart1.Series["Pressure"].Points.DataBindXY(xValues, yValuesPressure);
        }


        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            ts1 = new TimeSpan(24, 0, 0);
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            ts1 = new TimeSpan(1, 0, 0);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            ts1 = new TimeSpan(12, 0, 0);
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            ts1 = new TimeSpan(6, 0, 0);
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            ts1 = new TimeSpan(3, 0, 0);
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            ts1 = new TimeSpan(0, 30, 0);
        }

        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            ts1 = new TimeSpan(0, 15, 0);
        }

        private void radioButton8_CheckedChanged(object sender, EventArgs e)
        {
            ts1 = new TimeSpan(0, 5, 0);
        }

        private void radioButton9_CheckedChanged(object sender, EventArgs e)
        {
            ts1 = new TimeSpan(0, 2, 0);
        }

        private void radioButton10_CheckedChanged(object sender, EventArgs e)
        {
            ts1 = new TimeSpan(0, 1, 0);
        }

        public void CheckDb()
        {

            if (!File.Exists($"{sFilePath}"))
            {
                using (var connection = new SqliteConnection($"Data Source={sFilePath}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                    CREATE TABLE meteo_data (
	                id	INTEGER NOT NULL UNIQUE,
	                time	TEXT NOT NULL UNIQUE,
	                humidity	REAL NOT NULL,
	                temperature	REAL NOT NULL,
	                pressure	REAL NOT NULL,
	                PRIMARY KEY(id AUTOINCREMENT)
                    );
                ";
                    command.ExecuteNonQuery();
                }
            }

        }

        #region unused declarations
        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void progressBar3_Click(object sender, EventArgs e)
        {

        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void progressBar4_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }
        #endregion

        private void radioButton11_CheckedChanged(object sender, EventArgs e)
        {
            ts1 = new TimeSpan(720, 0, 0);
        }
    }
}