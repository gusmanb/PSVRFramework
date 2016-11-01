using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace PSVRToolbox
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            var set = Settings.Instance;

            chkAlt.Checked = set.AltModifier;
            chkBroadcast.Checked = set.EnableUDPBroadcast;
            chkControl.Checked = set.ControlModifier;
            chkMinimized.Checked = set.StartMinimized;
            chkShift.Checked = set.ShiftModifier;
            chkStartup.Checked = Utils.IsStartupEnabled();
            chkOpenTrack.Checked = set.EnableOpenTrackSender;

            txtBroadcastAddress.Text = set.UDPBroadcastAddress;
            txtBroadcastPort.Text = set.UDPBroadcastPort.ToString();
            txtOTPort.Text = set.OpenTrackPort.ToString();

            string[] keyNames = Enum.GetNames(typeof(Keys)).OrderBy(s => s).ToArray();

            cbHeadsetOff.Items.AddRange(keyNames);
            cbHeadsetOn.Items.AddRange(keyNames);
            cbRecenter.Items.AddRange(keyNames);
            cbShutdown.Items.AddRange(keyNames);
            cbTheater.Items.AddRange(keyNames);
            cbTracking.Items.AddRange(keyNames);
            cbVR.Items.AddRange(keyNames);

            cbHeadsetOff.SelectedItem = set.HeadSetOff.ToString();
            cbHeadsetOn.SelectedItem = set.HeadSetOn.ToString();
            cbRecenter.SelectedItem = set.Recenter.ToString();
            cbShutdown.SelectedItem = set.Shutdown.ToString();
            cbTheater.SelectedItem = set.EnableTheater.ToString();
            cbTracking.SelectedItem = set.EnableVRAndTracking.ToString();
            cbVR.SelectedItem = set.EnableVR.ToString();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            int port;

            if (!int.TryParse(txtBroadcastPort.Text, out port))
            {
                MessageBox.Show("Invalid broadcast port");
                return;
            }

            IPAddress addr;

            if(!IPAddress.TryParse(txtBroadcastAddress.Text, out addr))
            {
                MessageBox.Show("Invalid broadcast address");
                return;
            }

            if (!txtBroadcastAddress.Text.EndsWith(".255"))
            {
                MessageBox.Show("Only broadcast addresses supported");
                return;
            }

            int portOT;

            if (!int.TryParse(txtOTPort.Text, out portOT))
            {
                MessageBox.Show("Invalid OpenTrack port");
                return;
            }

            var set = new Settings();
            
            set.UDPBroadcastPort = port;
            set.OpenTrackPort = portOT;
            set.EnableOpenTrackSender = chkOpenTrack.Checked;
            set.AltModifier = chkAlt.Checked;
            set.EnableUDPBroadcast = chkBroadcast.Checked;
            set.ControlModifier = chkControl.Checked;
            set.StartMinimized = chkMinimized.Checked;
            set.ShiftModifier = chkShift.Checked;

            if (chkStartup.Checked)
                Utils.EnableStartup();
            else
                Utils.DisableStartup();

            set.UDPBroadcastAddress = txtBroadcastAddress.Text;
            set.HeadSetOff = (Keys)Enum.Parse(typeof(Keys), cbHeadsetOff.SelectedItem.ToString());
            set.HeadSetOn = (Keys)Enum.Parse(typeof(Keys), cbHeadsetOn.SelectedItem.ToString());
            set.Recenter = (Keys)Enum.Parse(typeof(Keys), cbRecenter.SelectedItem.ToString());
            set.Shutdown = (Keys)Enum.Parse(typeof(Keys), cbShutdown.SelectedItem.ToString());
            set.EnableTheater = (Keys)Enum.Parse(typeof(Keys), cbTheater.SelectedItem.ToString());
            set.EnableVRAndTracking = (Keys)Enum.Parse(typeof(Keys), cbTracking.SelectedItem.ToString());
            set.EnableVR = (Keys)Enum.Parse(typeof(Keys), cbVR.SelectedItem.ToString());


            Settings.Instance = set;
            Settings.SaveSettings();

            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
