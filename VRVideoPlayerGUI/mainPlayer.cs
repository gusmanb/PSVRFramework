using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using PSVRFramework;
using VRVideoPlayer;

namespace VRVideoPlayerGUI
{
    public partial class mainPlayer : Form
    {
        VRPlayer player;
        public mainPlayer()
        {
            InitializeComponent();
            Listen();


        }

        UdpClient client;
        unsafe void Listen()
        {
            bool ip = false;
            bool pt = false;

            IPAddress address = IPAddress.Any;
            int port = 9090;

            IPEndPoint ep = new IPEndPoint(address, port);

            client = new UdpClient();
            client.EnableBroadcast = true;
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.ExclusiveAddressUse = false;
            client.Client.Bind(ep);

            Task.Run(() =>
            {
                try
                {
                    while (true)
                    {


                        if (player != null)
                        {
                            byte[] data = client.Receive(ref ep);
                            string sData = Encoding.UTF8.GetString(data);
                            PSVRSensor sensor = JsonConvert.DeserializeObject<PSVRSensor>(sData);

                            if (sensor.Orientation == null)
                                continue;

                            fixed (float* d = &sensor.Orientation[0])
                                player.RotateMesh(d);
                        }
                        else
                            Thread.Sleep(10);
                    }
                }
                catch { }
            });

        }
        private void button1_Click(object sender, EventArgs e)
        {

            if (player != null)
                player.Dispose();

            var dlg = new OpenFileDialog();
            dlg.Filter = "MP4 video files|*.mp4";

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            Uri uri = new Uri(dlg.FileName, UriKind.Absolute);

            player = new VRPlayer();
            player.FileName = uri.ToString();
            player.LaunchPlayer();
        }
    }
}
