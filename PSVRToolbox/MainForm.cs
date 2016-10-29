using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PSVRFramework;

namespace PSVRToolbox
{
    
    public partial class MainForm : Form
    {
        PSVR vrSet;
        public MainForm()
        {
            InitializeComponent();
        }

        private void detectTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                vrSet = new PSVR();
                vrSet.SensorDataUpdate += VrSet_SensorDataUpdate;
                vrSet.SendCommand(PSVRCommand.GetHeadsetOn());
                vrSet.SendCommand(PSVRCommand.GetEnterVRMode());
                vrSet.SendCommand(PSVRCommand.GetExitVRMode());
                detectTimer.Enabled = false;
                lblStatus.Text = "VR set found";
                grpFunctions.Enabled = true;
            }
            catch { detectTimer.Enabled = true; }
        }

        private void VrSet_SensorDataUpdate(object sender, PSVRSensorEventArgs e)
        {
            //Nothing for now, just the data from the sensors
        }

        private void button1_Click(object sender, EventArgs e)
        {
            vrSet.SendCommand(PSVRCommand.GetHeadsetOn());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            vrSet.SendCommand(PSVRCommand.GetHeadsetOff());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            vrSet.SendCommand(PSVRCommand.GetEnableVRTracking());
            vrSet.SendCommand(PSVRCommand.GetEnterVRMode());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            vrSet.SendCommand(PSVRCommand.GetEnterVRMode());
        }

        private void button5_Click(object sender, EventArgs e)
        {
            vrSet.SendCommand(PSVRCommand.GetExitVRMode());
        }

        private void button6_Click(object sender, EventArgs e)
        {
            vrSet.SendCommand(PSVRCommand.GetEnterVRMode());
            vrSet.SendCommand(PSVRCommand.GetExitVRMode());
        }

        private void button7_Click(object sender, EventArgs e)
        {
            vrSet.SendCommand(PSVRCommand.GetHeadsetOff());
            vrSet.SendCommand(PSVRCommand.GetBoxOff());

            this.Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(vrSet != null)
                vrSet.Dispose();
        }
    }
}
