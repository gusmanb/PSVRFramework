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
