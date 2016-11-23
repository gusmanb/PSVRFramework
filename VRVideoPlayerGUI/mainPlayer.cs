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
using VRVideoPlayer;

namespace VRVideoPlayerGUI
{
    public partial class mainPlayer : Form
    {
        VRPlayer player;
        SensorListener listener;
        public mainPlayer()
        {
            InitializeComponent();
            var mons = VRPlayer.GetMonitors();
            cbMonitor.DisplayMember = "Name";
            cbMonitor.ValueMember = "Index";
            cbMonitor.DataSource = mons;

            player = new VRPlayer();
            player.PlayFinished += Player_PlayFinished;
            listener = new SensorListener();
            listener.DataReceived += Listener_DataReceived;
        }

        private void Listener_DataReceived(object sender, SensorEventArgs e)
        {
            if (player != null)
                //player.UpdateRotation(e.SensorReport.Orientation.X, e.SensorReport.Orientation.Y, 0);// e.SensorReport.Orientation.Z);
            player.UpdateRotation(e.SensorReport.Pose.X, e.SensorReport.Pose.Y, e.SensorReport.Pose.Z, e.SensorReport.Pose.W);
        }

        private void Player_PlayFinished(object sender, EventArgs e)
        {
            RemoteVRControl.SwitchToCinematic();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (cbMonitor.SelectedItem == null)
            {
                MessageBox.Show("Select which monitor to use");
                return;
            }

            var monIn = (int)cbMonitor.SelectedValue;

            if (player != null)
                player.Dispose();

            var dlg = new OpenFileDialog();
            dlg.Filter = "MP4 video files|*.mp4";

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            Uri uri = new Uri(dlg.FileName, UriKind.Absolute);

            string videoMode = cbVideoMode.SelectedItem?.ToString();
            string frameMode = cbFrameMode.SelectedItem?.ToString();

            VideoSettings settings = new VideoSettings();

            Vector2 offsetLeft = null;
            Vector2 offsetRight = null;
            Vector2 scaleLeft = null;
            Vector2 scaleRight = null;
            Vector3 initialRotation = null;
            float hfov = 0;
            switch (videoMode)
            {


                case "180 Mono":

                    hfov = 180;
                    scaleLeft = scaleRight = new Vector2 { X = 2, Y = 1 };
                    offsetLeft = offsetRight = new Vector2 { X = 0, Y = 0 };
                    initialRotation = new Vector3();
                    break;

                case "180 Stereo":

                    hfov = 180;

                    if (frameMode == "Side by side")
                    {
                        scaleLeft = scaleRight = new Vector2 { X = 1f, Y = 1 };
                        offsetLeft = new Vector2 { X = 0.5f, Y = 0 }; 
                        offsetRight = new Vector2();
                        initialRotation = new Vector3 { Y = (float)(Math.PI / 2) };
                        
                    }
                    else
                    {
                        scaleLeft = scaleRight = new Vector2 { X = 2, Y = 0.5f };
                        offsetLeft = new Vector2 { Y = 0.5f, X = 0 }; 
                        offsetRight = new Vector2();
                        initialRotation = new Vector3 { Y = (float)(Math.PI / 2) };
                    }

                    break;

                case "360 Mono":

                    hfov = 460;

                    scaleLeft = scaleRight = new Vector2 { X = 1, Y = 1 };
                    offsetLeft = offsetRight = new Vector2();
                    initialRotation = new Vector3 { Y = (float)(Math.PI / 4) };
                    break;

                case "360 Stereo":

                    hfov = 460;

                    if (frameMode == "Side by side")
                    {
                        scaleLeft = scaleRight = new Vector2 { X = 0.5f, Y = 1 };
                        offsetLeft = new Vector2();
                        offsetRight = new Vector2 { X = 0.5f, Y = 0 };
                        initialRotation = new Vector3 { Y = (float)(Math.PI / 4) };
                    }
                    else
                    {
                        scaleLeft = scaleRight = new Vector2 { X = 1, Y = 0.5f };
                        offsetLeft = new Vector2();
                        offsetRight = new Vector2 { X = 0, Y = 0.5f };
                        initialRotation = new Vector3 ();
                    }

                    break;

            }

            scaleLeft.Y = scaleRight.Y = 1;
            offsetLeft.Y = offsetRight.Y = 0;
            
            VideoSettings vs = new VideoSettings();
            vs.LeftEye.Offset = offsetLeft;
            vs.LeftEye.Scale = scaleLeft;
            vs.RightEye.Offset = offsetRight;
            vs.RightEye.Scale = scaleRight;
            vs.InitialRotation = initialRotation;
            vs.MonitorIndex = monIn;
            vs.HFOV = hfov;
            vs.VFOV = float.Parse(cbVFOV.SelectedItem.ToString());
            vs.Equilateral = true; 

            RemoteVRControl.PowerOnHeadset();
            RemoteVRControl.SwitchToVR();
            
            player.FileName = uri.ToString();
            player.LaunchPlayer(vs);
        }
        
    }
}
