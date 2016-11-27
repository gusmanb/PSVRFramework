using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using PSVRFramework;
using System.Text.RegularExpressions;
using PSVRToolbox.Classes;
using System.Net;

namespace PSVRToolbox
{

    public partial class MainForm : Form
    {

        bool exit = false;
        IKeyboardMouseEvents hookedEvents;

        object locker = new object();

        int startupSize = 0;

        //TapDetector tapper;

        DeviceController controller;
        
        public MainForm()
        {
            InitializeComponent();

            //tapper = new TapDetector(0.07f);
            //tapper.Tapped += Tapper_Tapped;

            try
            {
                if (CurrentOS.IsWindows)
                {
                    hookedEvents = Hook.GlobalEvents();
                    hookedEvents.KeyDown += HookedEvents_KeyDown;
                }
            }
            catch { }
            
            controller = new DeviceController(Settings.Instance.Standalone ? null : System.Net.IPAddress.Parse("127.0.0.1"), 9354);
            controller.DeviceStatusChanged += Controller_DeviceStatusChanged;

        }
        
        async void Recenter()
        {
            await controller.ResetPose();

            var current = Settings.Instance;

            byte fake = 0;
            if (current.ScreenSize < 50)
                fake = (byte)(current.ScreenSize + 1);
            else
                fake = (byte)(current.ScreenSize - 1);

            await controller.ApplyCinematicSettings(current.ScreenDistance, fake, current.Brightness, current.MicVol);
            await controller.ApplyCinematicSettings(current.ScreenDistance, current.ScreenSize, current.Brightness, current.MicVol);
        }

        //private void Tapper_Tapped(object sender, EventArgs e)
        //{
        //    PSVRController.Recenter();
        //}

        #region Button handlers

        private void button1_Click(object sender, EventArgs e)
        {
            controller.HeadsetOn();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            controller.HeadsetOff();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            controller.RecalibrateDevice();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            controller.EnableVRMode();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            controller.EnableCinematicMode();
        }

        private async void button6_Click(object sender, EventArgs e)
        {
            Recenter();
            
        }

        private void button7_Click(object sender, EventArgs e)
        {
            controller.Shutdown();

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
        }

        private void button15_Click(object sender, EventArgs e)
        {
            Settings set = Settings.Instance;
            set.ScreenDistance = (byte)trkDistance.Value;
            set.ScreenSize = (byte)trkSize.Value;
            set.Brightness = (byte)trkBrightness.Value;
            Settings.SaveSettings();
            var current = Settings.Instance;
            controller.ApplyCinematicSettings(current.ScreenDistance, current.ScreenSize, current.Brightness, current.MicVol);
        }

        private void button17_Click(object sender, EventArgs e)
        {
            LedMask mask = LedMask.None;

            switch (cbLeds.SelectedIndex)
            {
                case 0:
                    Settings.Instance.LedAIntensity = (byte)trkLedIntensity.Value;
                    mask = LedMask.LedA;
                    break;
                case 1:
                    Settings.Instance.LedBIntensity = (byte)trkLedIntensity.Value;
                    mask = LedMask.LedB;
                    break;
                case 2:
                    Settings.Instance.LedCIntensity = (byte)trkLedIntensity.Value;
                    mask = LedMask.LedC;
                    break;
                case 3:
                    Settings.Instance.LedDIntensity = (byte)trkLedIntensity.Value;
                    mask = LedMask.LedD;
                    break;
                case 4:
                    Settings.Instance.LedEIntensity = (byte)trkLedIntensity.Value;
                    mask = LedMask.LedE;
                    break;
                case 5:
                    Settings.Instance.LedFIntensity = (byte)trkLedIntensity.Value;
                    mask = LedMask.LedF;
                    break;
                case 6:
                    Settings.Instance.LedGIntensity = (byte)trkLedIntensity.Value;
                    mask = LedMask.LedG;
                    break;
                case 7:
                    Settings.Instance.LedHIntensity = (byte)trkLedIntensity.Value;
                    mask = LedMask.LedH;
                    break;
                case 8:
                    Settings.Instance.LedIIntensity = (byte)trkLedIntensity.Value;
                    mask = LedMask.LedI;
                    break;
                case 9:
                    Settings.Instance.LedAIntensity =
                    Settings.Instance.LedBIntensity =
                    Settings.Instance.LedCIntensity =
                    Settings.Instance.LedDIntensity =
                    Settings.Instance.LedEIntensity =
                    Settings.Instance.LedFIntensity =
                    Settings.Instance.LedGIntensity =
                    Settings.Instance.LedHIntensity =
                    Settings.Instance.LedIIntensity = (byte)trkLedIntensity.Value;

                    mask = LedMask.All;
                    break;
            }

            if (mask != LedMask.None)
            {
                Settings.SaveSettings();
                controller.ApplyLedSettings(new byte[] { Settings.Instance.LedAIntensity,
                    Settings.Instance.LedBIntensity,
                    Settings.Instance.LedCIntensity,
                    Settings.Instance.LedDIntensity,
                    Settings.Instance.LedEIntensity,
                    Settings.Instance.LedFIntensity,
                    Settings.Instance.LedGIntensity,
                    Settings.Instance.LedHIntensity,
                    Settings.Instance.LedIIntensity});
            }
        }

        
        private void btnDebug_Click(object sender, EventArgs e)
        {
            if (btnDebug.Text == "<")
            {
                Width = startupSize;
                btnDebug.Text = ">";
            }
            else
            {
                Width = grpLeds.Width + startupSize + 16;
                btnDebug.Text = "<";
            }
        }

        #endregion

        #region Misc. event handlers

        private void Controller_DeviceStatusChanged(object sender, StatusEventArgs e)
        {
            if (e.Connected)
            {
                BeginInvoke((Action)(() =>
                {
                    lblStatus.Text = "VR set found";
                    grpFunctions.Enabled = true;
                    grpCinematic.Enabled = true;
                    grpLeds.Enabled = true;

                }));
            }
            else
            {
                BeginInvoke((Action)(() =>
                {
                    grpFunctions.Enabled = false;
                    grpCinematic.Enabled = false;
                    grpLeds.Enabled = false;
                    lblStatus.Text = "Waiting for PS VR...";
                }));
            }
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            var set = Settings.Instance;

            startupSize = this.Width;

            if (set.StartMinimized)
            {
                BeginInvoke(new MethodInvoker(delegate
                {
                    if (CurrentOS.IsLinux)
                        this.WindowState = FormWindowState.Minimized;
                    else
                        Hide();
                }));
            }
            
            trkDistance.Value = set.ScreenDistance;
            trkSize.Value = set.ScreenSize;
            trkBrightness.Value = set.Brightness;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (!exit)
            {
                if (CurrentOS.IsLinux)
                    this.WindowState = FormWindowState.Minimized;
                else
                    this.Hide();

                e.Cancel = true;
                return;
            }

            controller.Dispose();

            try
            {
                if (hookedEvents != null)
                {
                    hookedEvents.KeyDown -= HookedEvents_KeyDown;
                    hookedEvents.Dispose();
                }
            }
            catch { }

            trayIcon.Visible = false;

            if (CurrentOS.IsLinux)
                Environment.Exit(0);
        }

        private void HookedEvents_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                var settings = Settings.Instance;

                if (settings.ControlModifier != e.Control || settings.ShiftModifier != e.Shift || settings.AltModifier != e.Alt)
                    return;

                if (e.KeyCode == settings.HeadSetOn)
                {
                    controller.HeadsetOn();
                    e.Handled = true;
                }
                else if (e.KeyCode == settings.HeadSetOff)
                {
                    controller.HeadsetOff();
                    e.Handled = true;
                }
                else if (e.KeyCode == settings.Recalibrate)
                {
                    controller.RecalibrateDevice();
                    e.Handled = true;
                }
                else if (e.KeyCode == settings.EnableVR)
                {
                    controller.EnableVRMode();
                    e.Handled = true;
                }
                else if (e.KeyCode == settings.EnableTheater)
                {
                    controller.EnableCinematicMode();
                    e.Handled = true;
                }
                else if (e.KeyCode == settings.Recenter)
                {
                    Recenter();
                    e.Handled = true;
                }
                else if (e.KeyCode == settings.Shutdown)
                {
                    controller.Shutdown();
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


        private void cbLeds_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cbLeds.SelectedIndex)
            {
                case 0:
                    trkLedIntensity.Value = Settings.Instance.LedAIntensity;
                    break;
                case 1:
                    trkLedIntensity.Value = Settings.Instance.LedBIntensity;
                    break;
                case 2:
                    trkLedIntensity.Value = Settings.Instance.LedCIntensity;
                    break;
                case 3:
                    trkLedIntensity.Value = Settings.Instance.LedDIntensity;
                    break;
                case 4:
                    trkLedIntensity.Value = Settings.Instance.LedEIntensity;
                    break;
                case 5:
                    trkLedIntensity.Value = Settings.Instance.LedFIntensity;
                    break;
                case 6:
                    trkLedIntensity.Value = Settings.Instance.LedGIntensity;
                    break;
                case 7:
                    trkLedIntensity.Value = Settings.Instance.LedHIntensity;
                    break;
                case 8:
                    trkLedIntensity.Value = Settings.Instance.LedIIntensity;
                    break;
            }
        }
        
        #endregion

    }
}
