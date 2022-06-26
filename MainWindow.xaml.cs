using System;
using System.Collections.Generic;
using System.IO;
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
        [DllImport("winmm.dll")]
        private static extern long mciSendString(string command, StringBuilder returninString, int returnLenght, IntPtr callBack);
        string ip, channelName;

        bool _recording;
        int _recordedIndex;

        MumbleConnection connection;
        ConnectionMumbleProtocol protocol;
        SpeakerPlayback playback;
        MicrophoneRecorder recorder;
        public MainWindow()
        {
            InitializeComponent();
            mciSendString("open new Type waveaudio alias recsound", null, 0, IntPtr.Zero);
            protocol = new ConnectionMumbleProtocol();
            playback = new SpeakerPlayback();

            _recording = false;
            _recordedIndex = 0;
            int playbackDeviceCount = NAudio.Wave.WaveOut.DeviceCount;
            //cbPlayBackDevices.Items.Add("Default Playback Device");
            for(int i = 0; i < playbackDeviceCount; i++)
            {
                NAudio.Wave.WaveOutCapabilities deviceInfo = NAudio.Wave.WaveOut.GetCapabilities(i);
                string deviceText = string.Format("{0}, {1} channels", deviceInfo.ProductName, deviceInfo.Channels);
                cbPlayBackDevices.Items.Add(deviceText);
            }

            cbPlayBackDevices.SelectedIndex = 0;
            SpeakerPlayback.SelectedDevice = cbPlayBackDevices.SelectedIndex - 1;

            recorder = new MicrophoneRecorder(protocol);
            int recorderDeviceCount = NAudio.Wave.WaveIn.DeviceCount;
            for(int i = 0; i < recorderDeviceCount; i++)
            {
                NAudio.Wave.WaveInCapabilities deviceInfo = NAudio.Wave.WaveIn.GetCapabilities(i);
                string deviceText = string.Format("{0}, {1} channels", deviceInfo.ProductName, deviceInfo.Channels);
                cbRecordingDevices.Items.Add(deviceText);
            }
            if(recorderDeviceCount > 0)
            {
                MicrophoneRecorder.SelectedDevice = 0;
                cbRecordingDevices.SelectedIndex = 0;
            }
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

        private void RecordBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_recording)
                return;

            mciSendString("record recsound", null, 0, IntPtr.Zero);
            _recording = true;
        }

        private void Savebtn_Click(object sender, RoutedEventArgs e)
        {
            if (!_recording)
                return;

            string recordStr = "save recsound " + Directory.GetCurrentDirectory() + @"\Recordss\" + _recordedIndex.ToString() + ".wav";

            mciSendString(recordStr, null, 0, IntPtr.Zero);
            mciSendString("close recsound", null, 0, IntPtr.Zero);
            _recording = false;
            _recordedIndex++;
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
