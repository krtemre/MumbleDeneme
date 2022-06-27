using System;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

using MumbleSharp;
using MumbleSharp.Model;

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
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();
            mciSendString("open new Type waveaudio alias recsound", null, 0, IntPtr.Zero);
            protocol = new ConnectionMumbleProtocol();
            protocol.encodedVoice = EncodedVoiceDelegate;
            protocol.userJoinedDelegate = UserJoinedDelegate;

            playback = new SpeakerPlayback();

            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            dispatcherTimer.Start();

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
            SpeakerPlayback.SelectedDevice = cbPlayBackDevices.SelectedIndex;

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
            tbIp.Text = "localhost";
        }
        private void AddPlayback(User user)
        {
            if (user.Id != connection.Protocol.LocalUser.Id)
                SpeakerPlayback.AddPlayer(user.Id, user.Voice);
        }
        
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (connection != null)
                if (connection.Process())
                    Thread.Yield();
                else
                    Thread.Sleep(1);
        }

        void EncodedVoiceDelegate(BasicMumbleProtocol proto, byte[] data, uint userId, long sequence, MumbleSharp.Audio.Codecs.IVoiceCodec codec, MumbleSharp.Audio.SpeechTarget target)
        {
            User user = proto.Users.FirstOrDefault(u => u.Id == userId);
            AddPlayback(user);
        }
        
        void UserJoinedDelegate(BasicMumbleProtocol proto, User user)
        {
            lbUsers.Items.Add(user.Name);
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
                protocol.Close();
                connection = null;
            }
            string addr = ip;
            int port = 64738;
            string srvConnectName = "Test" + "@" + ip + ":" + port;

            connection = new MumbleConnection(new IPEndPoint(Dns.GetHostAddresses(addr).First(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork), port), protocol);
            connection.Connect("Test", "", new string[0], srvConnectName);

            //if (connection != null)
            //    MessageBox.Show("Bağlandınız...", "Bağlantı", MessageBoxButton.OK, MessageBoxImage.Information);

            MessageBox.Show(connection.Host.ToString(), "Host",MessageBoxButton.OK);
            //var loc = connection.Protocol.LocalUser;

            while (connection.Protocol.LocalUser == null)
            {
                connection.Process();
                Thread.Sleep(1);
            }
        }
        private void DisconnectBtn_Clicked(object sender, RoutedEventArgs e)
        {
            if (connection != null)
            {
                connection.Close();
                connection = null;
                protocol.Close();
                lbUsers.Items.Clear();
            }

            if (connection == null)
                MessageBox.Show("Bağlantı Kesildi...", "Bağlantı", MessageBoxButton.OK, MessageBoxImage.Information);
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

            //string recordStr = "save recsound " + Directory.GetCurrentDirectory() + @"\Recordss\" + _recordedIndex.ToString() + ".wav";
            string recordStr = @"save recsound C:\Users\emre.kurt\Desktop\Emre\rec" + _recordedIndex.ToString() + ".wav";
            mciSendString(recordStr, null, 0, IntPtr.Zero);
            mciSendString("close recsound", null, 0, IntPtr.Zero);
            _recording = false;
            _recordedIndex++;
        }

        private void CPlayBackDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SpeakerPlayback.SelectedDevice = cbPlayBackDevices.SelectedIndex; //cb combo box
        }

        private void CbRecordingDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MicrophoneRecorder.SelectedDevice = cbRecordingDevices.SelectedIndex;
        }
        
    }
}
