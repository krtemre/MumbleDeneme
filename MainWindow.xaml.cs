using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using MumbleSharp;
using MumbleSharp.Model;
using Message = MumbleSharp.Model.Message;

namespace MumbleDeneme
{
    /// <summary>
    /// MainWindow.xaml etkileşim mantığı
    /// </summary>
    public partial class MainWindow : Window
    {
        string ip, channelName;
        MumbleConnection connection;
        ConnectionMumbleProtocol protocol;
        public MainWindow()
        {
            InitializeComponent();

            protocol = new ConnectionMumbleProtocol();
        }

        private void cbPlaybackDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            // SpeakerPlayback.SelectedDevice = cbPlaybackDevices.SelectedIndex - 1; //cb combo box
        }

        private void cbRecordingDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            // MicrophoneRecorder.SelectedDevice = cbRecordingDevices.SelectedIndex;
        }

        private void ConnectBtn_Clicked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbChannelName.Text))
                channelName = "Root";
            else
                channelName = tbChannelName.Text;

            if (string.IsNullOrEmpty(tbIp.Text))
                ip = "127.0.0.1";
            else
                ip = tbIp.Text;

            if (connection != null)
            {
                connection.Close();
                connection = null;
            }

            string srvConnectName = "dummy" + "@" + ip + ":" + "64738";

            connection = new MumbleConnection(new IPEndPoint(Dns.GetHostAddresses(ip).FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                , 64738), protocol);
        }

        private void DisconnectBtn_Clicked(object sender, RoutedEventArgs e)
        {
            if(connection != null)
            {
                connection.Close();
                connection = null;
            }
        }
    }
}
