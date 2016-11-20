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

namespace PSVRToolbox
{

    public partial class MainForm : Form
    {

        bool exit = false;
        IKeyboardMouseEvents hookedEvents;
        SensorBroadcaster broadcaster;
        RemoteCommandListener cmdListen;

        object locker = new object();

        int startupSize = 0;

        TapDetector tapper;

        public MainForm()
        {
            InitializeComponent();

            tapper = new TapDetector(0.05f);
            tapper.Tapped += Tapper_Tapped;

            try
            {
                if (CurrentOS.IsWindows)
                {
                    hookedEvents = Hook.GlobalEvents();
                    hookedEvents.KeyDown += HookedEvents_KeyDown;
                }
            }
            catch { }

            cmdListen = new RemoteCommandListener(14598);
        }
        
        private void Tapper_Tapped(object sender, EventArgs e)
        {
            PSVRController.Recenter();
        }

        #region Button handlers

        private void button1_Click(object sender, EventArgs e)
        {
            PSVRController.HeadsetOn();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            PSVRController.HeadsetOff();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                PSVRController.EnableVRTracking();
                await Task.Delay(1500);
                PSVRController.ApplyLedSettings();
            });
        }

        private void button4_Click(object sender, EventArgs e)
        {
            PSVRController.EnableVRMode();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            PSVRController.EnableCinematicMode();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            PSVRController.Recenter();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            PSVRController.Shutdown();

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
                lock (locker)
                {
                    broadcaster.Dispose();
                    broadcaster = null;
                }
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            Settings set = Settings.Instance;
            set.ScreenDistance = (byte)trkDistance.Value;
            set.ScreenSize = (byte)trkSize.Value;
            set.Brightness = (byte)trkBrightness.Value;
            Settings.SaveSettings();
            PSVRController.ApplyCinematicSettings();
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
                PSVRController.ApplyLedSettings();
            }
        }


        private void button11_Click(object sender, EventArgs e)
        {
            string line = hextReportId.Text.PadLeft(2, '0') + hextStatusCode.Text.PadLeft(2, '0') + "AA" + (hexData.Text.Length / 2).ToString("X2") + hexData.Text;
            shellControl1.WriteLine(line);
            shellControl1.FireCommandEntered(line);
            hextReportId.Text = "";
            hextStatusCode.Text = "";
            hexData.Text = "";
            shellControl1.WritePrompt();
        }


        private void btnDebug_Click(object sender, EventArgs e)
        {
            if (btnDebug.Text == "<")
            {
                Width = startupSize;
                btnDebug.Text = ">";
                shellControl1.ResetText();
            }
            else
            {
                Width = shellControl1.Width + startupSize + 16;
                btnDebug.Text = "<";
                shellControl1.ResetText();
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            hextReportId.Text = "";
            hextStatusCode.Text = "";
            hexData.Text = "";
        }

        #endregion

        #region Misc. event handlers
        
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

            if (set.EnableUDPBroadcast)
                broadcaster = new SensorBroadcaster(set.UDPBroadcastAddress, set.UDPBroadcastPort);

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

            PSVRController.DeviceDisconnected();

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
                    PSVRController.HeadsetOn();
                    e.Handled = true;
                }
                else if (e.KeyCode == settings.HeadSetOff)
                {
                    PSVRController.HeadsetOff();
                    e.Handled = true;
                }
                else if (e.KeyCode == settings.EnableVRAndTracking)
                {
                    PSVRController.EnableVRTracking();
                    e.Handled = true;
                }
                else if (e.KeyCode == settings.EnableVR)
                {
                    PSVRController.EnableVRMode();
                    e.Handled = true;
                }
                else if (e.KeyCode == settings.EnableTheater)
                {
                    PSVRController.EnableCinematicMode();
                    e.Handled = true;
                }
                else if (e.KeyCode == settings.Recenter)
                {
                    PSVRController.Recenter();
                    e.Handled = true;
                }
                else if (e.KeyCode == settings.Shutdown)
                {
                    PSVRController.Shutdown();
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

        private void shellControl1_CommandEntered(object sender, CommandEnteredEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(e.Command))
                    return;

                if (e.Command.Length % 2 != 0)
                {
                    shellControl1.WriteLine("HEX string does not respect byte boundary");
                    return;
                }

                byte[] data = StringToByteArray(e.Command);
                var res = PSVRController.Raw(data);

                if (!res)
                    shellControl1.WriteLine("Report NOT sent");
                else
                    shellControl1.WriteLine("Report sent");
            }
            catch { shellControl1.WriteLine("Malformed HEX string"); }
        }

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;

            byte[] bytes = new byte[NumberChars / 2];

            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = byte.Parse(hex.Substring(i, 2), System.Globalization.NumberStyles.AllowHexSpecifier);

            return bytes;
        }

        #endregion

        #region Device connected detection

        private void detectTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                detectTimer.Enabled = false;
                var vrSet = new PSVR(Settings.Instance.EnableUDPBroadcast);
                PSVRController.DeviceConnected(vrSet);
                vrSet.SensorDataUpdate += VrSet_SensorDataUpdate;
                vrSet.Removed += VrSet_Removed;
                vrSet.INReport += VrSet_CommandResponse;
                ThreadPool.QueueUserWorkItem((a) =>
                {
                    PSVRController.RequestDeviceInfo();
                    PSVRController.HeadsetOn();
                    PSVRController.EnableVRMode();
                    Thread.Sleep(1500);
                    PSVRController.EnableCinematicMode();
                    Thread.Sleep(1500);
                    PSVRController.ApplyCinematicSettings();

                });

                lblStatus.Text = "VR set found";
                grpFunctions.Enabled = true;
                grpCinematic.Enabled = true;
                grpLeds.Enabled = true;
            }
            catch { detectTimer.Enabled = true; }
        }

        private void VrSet_CommandResponse(object sender, PSVRINEventArgs e)
        {

            if (btnDebug.Text == "<")
            {
                this.BeginInvoke((Action)(() =>
                {
                    shellControl1.WriteLine("");
                    shellControl1.WriteLine("Received report, ID: " + e.Response.ReportID.ToString("X2") + ", Response code: " + e.Response.CommandStatus.ToString("X2"));
                    shellControl1.WriteLine("--Raw data--");

                    shellControl1.WriteLine(e.Response.ReportID.ToString("X2") +
                        e.Response.CommandStatus.ToString("X2") +
                        e.Response.DataStart.ToString("X2") +
                        e.Response.DataLength.ToString("X2") +
                        BitConverter.ToString(e.Response.Data).Replace("-", ""));

                    shellControl1.WritePrompt();

                }));
            }

            switch (e.Response.ReportID)
            {
                case 0x80:

                    PSVRReport.PSVRDeviceInfoReport dev = PSVRReport.PSVRDeviceInfoReport.ParseInfo(e.Response.Data);
                    this.BeginInvoke((Action)(() =>
                    {
                        lblSerial.Text = "Device serial: " + dev.SerialNumber;
                        lblFirmware.Text = "Firmware version: " + dev.MajorVersion + "." + dev.MinorVersion.ToString().PadLeft(2, '0');

                    }));
                    break;

                case 0xF0:

                    PSVRReport.PSVRDeviceStatusReport stat = PSVRReport.PSVRDeviceStatusReport.ParseStatus(e.Response.Data);
                    this.BeginInvoke((Action)(() =>
                    {
                        chkWorn.Checked = stat.Status.HasFlag(PSVRReport.PSVRDeviceStatusReport.PSVRStatusMask.Worn);
                        chkCinematic.Checked = stat.Status.HasFlag(PSVRReport.PSVRDeviceStatusReport.PSVRStatusMask.Cinematic);
                        chkHeadphones.Checked = stat.Status.HasFlag(PSVRReport.PSVRDeviceStatusReport.PSVRStatusMask.Headphones);
                        chkHMDOn.Checked = stat.Status.HasFlag(PSVRReport.PSVRDeviceStatusReport.PSVRStatusMask.HeadsetOn);
                        chkMute.Checked = stat.Status.HasFlag(PSVRReport.PSVRDeviceStatusReport.PSVRStatusMask.Mute);
                        trkVolume.Value = (int)stat.Volume;
                    }));
                    break;
#if DEBUG
                case 0xA0:

                    PSVRReport.PSVRUnsolicitedReport resp = PSVRReport.PSVRUnsolicitedReport.ParseResponse(e.Response.Data);

                    Console.WriteLine("Received OUT report response");
                    Console.WriteLine("Report ID: 0x" + resp.ReportID.ToString("X2"));
                    Console.WriteLine("Result code: " + resp.ResultCode);
                    Console.WriteLine("Response message: " + resp.Message);

                    break;

                default:

                    Console.WriteLine("Received response " + e.Response.ReportID.ToString("X2"));
                    Console.WriteLine("Status: " + e.Response.CommandStatus.ToString("X2"));
                    Console.WriteLine("Binary data: " + BitConverter.ToString(e.Response.Data).Replace("-", ""));

                    var str = Encoding.ASCII.GetString(e.Response.Data);

                    str = Regex.Replace(str, @"[^a-zA-Z0-9 -+!""·$%&/()=?*\^`\[\]{}#@~€]", string.Empty);

                    Console.WriteLine("ASCII data: " + str);
                    break;
#endif

            }
        }

        #endregion

        #region VR set events

        private void VrSet_Removed(object sender, EventArgs e)
        {
            var vrSet = (PSVR)sender;

            BeginInvoke((Action)(() =>
            {
                grpFunctions.Enabled = false;
                grpCinematic.Enabled = false;
                grpLeds.Enabled = false;
                if (vrSet != null)
                {
                    try
                    {
                        vrSet.Dispose();
                    }
                    catch { }

                    vrSet = null;
                }
                lblStatus.Text = "Waiting for PS VR...";
                lblSerial.Text = "Device serial: unknown";
                lblFirmware.Text = "Firmware version: unknown";
                detectTimer.Enabled = true;
            }));
        }
        
        private void VrSet_SensorDataUpdate(object sender, PSVRSensorEventArgs e)
        {
            tapper.Feed(e.SensorData.SensorRead1);

            lock (locker)
            {
                if (broadcaster != null)
                    broadcaster.Broadcast(e.SensorData);
            }
        }

        #endregion
        
    }
}
