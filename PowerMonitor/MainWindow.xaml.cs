using System;
using System.Reflection;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Windows.Media;

namespace PowerMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        class CustomTimer : System.Timers.Timer
        {
            public MainWindow window;
        }
        
        private CustomTimer monitorTimer;
        System.Windows.Forms.NotifyIcon ni;

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += OnWindowClosing;

            ni = new NotifyIcon();
            ni.Icon = new System.Drawing.Icon("battery.ico");
            ni.Visible = true;
            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        public void OnWindowClosing(object sender, EventArgs e)
        {
            ni.Visible = false;
        }


        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;
            monitorTimer = new CustomTimer();
            monitorTimer.window = this;
            monitorTimer.Interval = 1000;
            monitorTimer.Elapsed += OnTimerElapsed;
            monitorTimer.AutoReset = true;
            monitorTimer.Enabled = true;
        }

        private static void OnTimerElapsed(Object src, ElapsedEventArgs e)
        {
            // Query battery information
            BatteryInformation battInfo = BatteryInfo.GetBatteryInformation();
            Object batteryChargeStatus = null;

            Type t = typeof(System.Windows.Forms.PowerStatus);
            PropertyInfo[] pi = t.GetProperties();
            for (int i = 0; i < pi.Length; i++)
            {
                if (pi[i].Name == "BatteryChargeStatus")
                {
                    batteryChargeStatus = pi[i].GetValue(SystemInformation.PowerStatus, null);
                    break;
                }
                else
                {
                    continue;
                }
            }

            // Record the data
            double voltage = battInfo.Voltage / 1000.0;
            BatteryStatus.Instance.RecordData(voltage, battInfo.CurrentCapacity, batteryChargeStatus.ToString());
            BatteryStatus.BatteryStatusCode status = BatteryStatus.Instance.QueryStatus();

            if (status != BatteryStatus.BatteryStatusCode.OK)
            {
                Console.Beep(5000, 200);
                Console.Beep(5000, 200);
                Console.Beep(5000, 200);
            }

            // Update the GUI
            MainWindow window = ((CustomTimer)src).window;
            
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate {
                window.lblMode.Content = batteryChargeStatus.ToString();
                window.lblVoltage.Content = String.Format("{0} V", battInfo.Voltage);
                window.lblCapacity.Content = String.Format("{0} mWh", battInfo.CurrentCapacity);
                window.lblCharging.Content = BatteryStatus.Instance.Charging;

                switch(status)
                {
                    case BatteryStatus.BatteryStatusCode.OK:
                        window.lblStatus.Content = "OK";
                        window.lblStatus.Foreground = Brushes.Black;
                        break;
                    case BatteryStatus.BatteryStatusCode.LowVoltage:
                        window.lblStatus.Content = "Low Voltage!";
                        window.lblStatus.Foreground = Brushes.Red;
                        break;
                    case BatteryStatus.BatteryStatusCode.Discharging:
                        window.lblStatus.Content = "Discharging!";
                        window.lblStatus.Foreground = Brushes.Red;
                        break;
                }
            }));
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            btnStop.IsEnabled = false;
            btnStart.IsEnabled = true;
            monitorTimer.Stop();
            monitorTimer.Dispose();
            monitorTimer = null;

            lblMode.Content = "Monitoring Disabled";
            lblVoltage.Content = "Monitoring Disabled";
            lblCapacity.Content = "Monitoring Disabled";
            lblStatus.Content = "";
        }
    }
}
