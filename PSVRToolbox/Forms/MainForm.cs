using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using PSVRFramework;

namespace PSVRToolbox
{
    
    public partial class MainForm : Form
    {

        bool exit = false;
        IKeyboardMouseEvents hookedEvents;
        PSVR vrSet;
        SensorBroadcaster broadcaster;
        object locker = new object();

        public MainForm()
        {
            InitializeComponent();

            //hookedEvents = Hook.GlobalEvents();
            //hookedEvents.KeyDown += HookedEvents_KeyDown;
        }
        
        #region Button handlers

        private void button1_Click(object sender, EventArgs e)
        {
            HeadsetOn();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            HeadsetOff();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            EnableVRTracking();
        }
        
        private void button4_Click(object sender, EventArgs e)
        {
            EnableVRMode();
        }
        
        private void button5_Click(object sender, EventArgs e)
        {
            EnableTheaterMode();
        }
        
        private void button6_Click(object sender, EventArgs e)
        {
            Recenter();
        }
        
        private void button7_Click(object sender, EventArgs e)
        {
            Shutdown();

        }

        private void button8_Click(object sender, EventArgs e)
        {
            exit = true;
            this.Close();
        }
        
        private void btnSettings_Click(object sender, EventArgs e)
        {
            var setFrm = new SettingsForm();
            setFrm.ShowDialog();
            setFrm.Dispose();
            var set = Settings.Instance;

            if (set.EnableUDPBroadcast && broadcaster == null)
                broadcaster = new SensorBroadcaster(set.UDPBroadcastAddress, set.UDPBroadcastPort);
            else if (!set.EnableUDPBroadcast && broadcaster != null)
            {
                lock(locker)
                {
                    broadcaster.Dispose();
                    broadcaster = null;
                }
            }
        }

        #endregion

        #region Misc. event handlers

        private void MainForm_Load(object sender, EventArgs e)
        {
            var set = Settings.Instance;

            if (set.StartMinimized)
            {
                BeginInvoke(new MethodInvoker(delegate
                {
                    Hide();
                }));
            }

            if (set.EnableUDPBroadcast)
                broadcaster = new SensorBroadcaster(set.UDPBroadcastAddress, set.UDPBroadcastPort);

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (!exit)
            {
                this.Hide();
                e.Cancel = true;
                return;
            }

            if (vrSet != null)
                vrSet.Dispose();

            hookedEvents.KeyDown -= HookedEvents_KeyDown;
            trayIcon.Visible = false;
        }

        private void HookedEvents_KeyDown(object sender, KeyEventArgs e)
        {
            if (vrSet == null)
                return;

            try
            {
                var settings = Settings.Instance;

                if (settings.ControlModifier != e.Control || settings.ShiftModifier != e.Shift || settings.AltModifier != e.Alt)
                    return;

                if (e.KeyCode == settings.HeadSetOn)
                {
                    HeadsetOn();
                    e.Handled = true;
                }
                else if (e.KeyCode == settings.HeadSetOff)
                {
                    HeadsetOff();
                    e.Handled = true;
                }
                else if (e.KeyCode == settings.EnableVRAndTracking)
                {
                    EnableVRTracking();
                    e.Handled = true;
                }
                else if (e.KeyCode == settings.EnableVR)
                {
                    EnableVRMode();
                    e.Handled = true;
                }
                else if (e.KeyCode == settings.EnableTheater)
                {
                    EnableTheaterMode();
                    e.Handled = true;
                }
                else if (e.KeyCode == settings.Recenter)
                {
                    Recenter();
                    e.Handled = true;
                }
                else if (e.KeyCode == settings.Shutdown)
                {
                    Shutdown();
                    e.Handled = true;
                }
            }
            catch { }

        }
        
        private void trayIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.BringToFront();
        }
        
        #endregion

        #region PS VR functions

        private void HeadsetOn()
        {
            vrSet.SendCommand(PSVRCommand.GetHeadsetOn());
        }

        private void HeadsetOff()
        {
            vrSet.SendCommand(PSVRCommand.GetHeadsetOff());
        }

        private void EnableVRTracking()
        {
            vrSet.SendCommand(PSVRCommand.GetEnableVRTracking());
            vrSet.SendCommand(PSVRCommand.GetEnterVRMode());
        }
        private void EnableVRMode()
        {
            vrSet.SendCommand(PSVRCommand.GetEnterVRMode());
        }
        private void EnableTheaterMode()
        {
            vrSet.SendCommand(PSVRCommand.GetExitVRMode());
        }
        private void Recenter()
        {
            vrSet.SendCommand(PSVRCommand.GetEnterVRMode());
            Thread.Sleep(1500);
            vrSet.SendCommand(PSVRCommand.GetExitVRMode());
        }
        private void Shutdown()
        {
            vrSet.SendCommand(PSVRCommand.GetHeadsetOff());
            vrSet.SendCommand(PSVRCommand.GetBoxOff());
        }

        #endregion

        #region Device connected detection

        private void detectTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                vrSet = new PSVR();
                vrSet.SensorDataUpdate += VrSet_SensorDataUpdate;
                vrSet.Removed += VrSet_Removed;
                vrSet.SendCommand(PSVRCommand.GetHeadsetOn());
                vrSet.SendCommand(PSVRCommand.GetEnterVRMode());
                Thread.Sleep(1500);
                vrSet.SendCommand(PSVRCommand.GetExitVRMode());
                detectTimer.Enabled = false;
                lblStatus.Text = "VR set found";
                grpFunctions.Enabled = true;
            }
            catch { detectTimer.Enabled = true; }
        }

        #endregion

        #region VR set events

        private void VrSet_Removed(object sender, EventArgs e)
        {
            BeginInvoke((Action)(() =>
            {
                grpFunctions.Enabled = false;
                vrSet = null;
                lblStatus.Text = "Waiting for PS VR...";
                detectTimer.Enabled = true;
            }));
        }

        private void VrSet_SensorDataUpdate(object sender, PSVRSensorEventArgs e)
        {
            lock (locker)
            {
                if (broadcaster != null)
                    broadcaster.Broadcast(e.SensorData);
            }
        }

        #endregion

    }
}
