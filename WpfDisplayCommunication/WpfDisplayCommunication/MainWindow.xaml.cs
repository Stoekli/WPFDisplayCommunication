using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WpfDisplayCommunication.Data;
using ScottPlot;

namespace WpfDisplayCommunication
{
    public partial class MainWindow : Window
    {
        private SerialPort _serialPort;
        private DispatcherTimer _timer;
        private string _output = "";
        private string _textToShow = ":   00";
        private DisplayViewModel _displayViewModel = new DisplayViewModel();

        ScottPlot.Plottables.DataLogger Logger;


        public MainWindow()
        {
            InitializeComponent();

            Logger = formsPlot1.Plot.Add.DataLogger();

            // disable mouse interaction by default
            formsPlot1.Interaction.Disable();
            formsPlot1.Plot.Axes.AutoScale();

            _serialPort = new SerialPort("COM7", 9600);
            _serialPort.DataReceived += SerialPort_DataReceived;
            _serialPort.Open();

            txtDisplayText.GotFocus += TxtDisplayText_GotFocus;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();


            this.DataContext = _displayViewModel;
            
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Debug.WriteLine(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
            SendDisplayCommand(_textToShow);
        }

        private void SendDisplayCommand(string command)
        {
            if (_serialPort.IsOpen)
            {
                _output = "";
                _serialPort.Write(command);
                
            }
            else
            {
                MessageBox.Show("COM-Port is not open.");
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            _output += _serialPort.ReadExisting();
            if (_output.Contains('<'))
            {
                Dispatcher.Invoke(() => txtDisplayCommand.Text = "Command: " + _textToShow + _output);

                Regex regex = new Regex(@">([0-9A-F]{3})([0-9A-F]{3})([0-9A-F]{3})([0-9A-F]{3})<");
                Match match = regex.Match(_output);

                if (match.Success)
                {
                    int yyy = Convert.ToInt32(match.Groups[3].Value, 16);
                    if (yyy > 0x7ff)
                    {
                        yyy = (yyy - 0x1000);
                    }

                    _displayViewModel.Temperatur1 = "Die gemessene Temperatur beträgt " + yyy + " Grad";
                    Logger.Add(yyy);
                    if (Logger.HasNewData)
                    {
                        formsPlot1.Refresh();
                    }

                }
                else
                {
                    _displayViewModel.Temperatur1 = "Error";
                }
            }

        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (txtDisplayText.Text.Length != 3)
            {
                MessageBox.Show("Bitte geben Sie drei Ziffern ein.");
                return;
            }

            if (chkBlink.IsChecked == true)
            {
                _textToShow = ":" + txtDisplayText.Text + "01";
            }
            else
            {
                _textToShow = ":" + txtDisplayText.Text + "00";
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }

            _timer.Stop();
        }
        private void TxtDisplayText_GotFocus(object sender, RoutedEventArgs e)
        {
            txtDisplayText.Text = "";
            txtDisplayText.GotFocus -= TxtDisplayText_GotFocus;
        }

        private void txtDisplayText_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
              if (!char.IsDigit(e.Text, e.Text.Length - 1))
              {
                  e.Handled = true;
              }

              TextBox textBox = sender as TextBox;
              if (textBox != null && textBox.Text.Length >= 3)
              {
                  e.Handled = true;
              }
        }
    }
}
