using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace Monitoring
{
    public partial class MainWindow : Window
    {
        private System.Timers.Timer _monitorTimer;
        private int _interval = 1000;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void StartMonitoring_Click(object sender, RoutedEventArgs e)
        {
            // Прочитать интервал с TextBox
            if (int.TryParse(IntervalTextBox.Text, out int interval))
            {
                _interval = interval;
            }
            else
            {
                MessageBox.Show("Invalid interval format. Please enter a number.");
                return;
            }

            _monitorTimer = new System.Timers.Timer(_interval);
            _monitorTimer.Elapsed += async (s, args) => await UpdateConnections();
            _monitorTimer.Start();
            await UpdateConnections();
        }

        private async Task UpdateConnections()
        {
            var connections = await GetConnectionsAsync();
            Dispatcher.Invoke(() => ConnectionsDataGrid.ItemsSource = connections);
        }

        private async Task<List<NetworkConnection>> GetConnectionsAsync()
        {
            return await Task.Run(() =>
            {
                List<NetworkConnection> connections = new List<NetworkConnection>();
                IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
                TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections();

                foreach (var connection in tcpConnections)
                {
                    connections.Add(new NetworkConnection
                    {
                        LocalIP = connection.LocalEndPoint.Address.ToString(),
                        RemoteIP = connection.RemoteEndPoint.Address.ToString(),
                        Port = connection.LocalEndPoint.Port,
                        Protocol = "TCP",
                        Status = connection.State.ToString(),
                        SentData = 0,     // Данные для трафика будут нулевыми
                        ReceivedData = 0  // Данные для трафика будут нулевыми
                    });
                }

                return connections;
            });
        }

        private void ExportToCSV_Click(object sender, RoutedEventArgs e)
        {
            var connections = ConnectionsDataGrid.ItemsSource as List<NetworkConnection>;
            if (connections != null)
            {
                using (StreamWriter sw = new StreamWriter("network_connections.csv"))
                {
                    sw.WriteLine("LocalIP,RemoteIP,Port,Protocol,Status,SentData,ReceivedData");
                    foreach (var conn in connections)
                    {
                        sw.WriteLine($"{conn.LocalIP},{conn.RemoteIP},{conn.Port},{conn.Protocol},{conn.Status},{conn.SentData},{conn.ReceivedData}");
                    }
                }
                MessageBox.Show("Data exported to network_connections.csv");
            }
        }
    }

    public class NetworkConnection
    {
        public string LocalIP { get; set; }
        public string RemoteIP { get; set; }
        public int Port { get; set; }
        public string Protocol { get; set; }
        public string Status { get; set; }
        public long SentData { get; set; }
        public long ReceivedData { get; set; }
    }
}
