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
            chkControl.Checked = set.ControlModifier;
            chkMinimized.Checked = set.StartMinimized;
            chkShift.Checked = set.ShiftModifier;
            chkStartup.Checked = Utils.IsStartupEnabled();
            chkStandalone.Checked = set.Standalone;

            string[] keyNames = Enum.GetNames(typeof(Keys)).OrderBy(s => s).ToArray();

            cbHeadsetOff.Items.AddRange(keyNames);
            cbHeadsetOn.Items.AddRange(keyNames);
            cbRecenter.Items.AddRange(keyNames);
            cbShutdown.Items.AddRange(keyNames);
            cbTheater.Items.AddRange(keyNames);
            cbRecal.Items.AddRange(keyNames);
            cbVR.Items.AddRange(keyNames);

            cbHeadsetOff.SelectedItem = set.HeadSetOff.ToString();
            cbHeadsetOn.SelectedItem = set.HeadSetOn.ToString();
            cbRecenter.SelectedItem = set.Recenter.ToString();
            cbShutdown.SelectedItem = set.Shutdown.ToString();
            cbTheater.SelectedItem = set.EnableTheater.ToString();
            cbRecal.SelectedItem = set.Recalibrate.ToString();
            cbVR.SelectedItem = set.EnableVR.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int port;

            var set = new Settings();
            
            set.AltModifier = chkAlt.Checked;
            set.ControlModifier = chkControl.Checked;
            set.StartMinimized = chkMinimized.Checked;
            set.ShiftModifier = chkShift.Checked;
            set.Standalone = chkStandalone.Checked;

            if (chkStartup.Checked)
                Utils.EnableStartup();
            else
                Utils.DisableStartup();
            
            set.HeadSetOff = (Keys)Enum.Parse(typeof(Keys), cbHeadsetOff.SelectedItem.ToString());
            set.HeadSetOn = (Keys)Enum.Parse(typeof(Keys), cbHeadsetOn.SelectedItem.ToString());
            set.Recenter = (Keys)Enum.Parse(typeof(Keys), cbRecenter.SelectedItem.ToString());
            set.Shutdown = (Keys)Enum.Parse(typeof(Keys), cbShutdown.SelectedItem.ToString());
            set.EnableTheater = (Keys)Enum.Parse(typeof(Keys), cbTheater.SelectedItem.ToString());
            set.Recalibrate = (Keys)Enum.Parse(typeof(Keys), cbRecal.SelectedItem.ToString());
            set.Recenter = (Keys)Enum.Parse(typeof(Keys), cbRecenter.SelectedItem.ToString());
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
