/*
* PSVRFramework - PlayStation VR PC framework
* Copyright (C) 2016 Agustín Giménez Bernad <geniwab@gmail.com>
*
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU Affero General Public License as
* published by the Free Software Foundation, either version 3 of the
* License, or (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU Affero General Public License for more details.
*
* You should have received a copy of the GNU Affero General Public License
* along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

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
