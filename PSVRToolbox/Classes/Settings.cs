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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace PSVRToolbox
{
    public class Settings
    {
        static Settings instance;
        public static Settings Instance
        {
            get
            {
                if (instance == null)
                    ReadSettings();

                return instance;
            }

            set { instance = value; }
        }

        public static void ReadSettings()
        {
            try
            {
                string file = Path.Combine(Application.StartupPath, "settings.json");

                if (File.Exists(file))
                    instance = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(file));
                else
                    instance = new Settings();
            }
            catch { instance = new Settings(); }
        }

        public static void SaveSettings()
        {
            try
            {
                string file = Path.Combine(Application.StartupPath, "settings.json");

                File.WriteAllText(file, JsonConvert.SerializeObject(instance));
            }
            catch { }
        }

        public bool ControlModifier { get; set; }
        public bool ShiftModifier { get; set; }
        public bool AltModifier { get; set; }
        public Keys HeadSetOn { get; set; } = Keys.None;
        public Keys HeadSetOff { get; set; } = Keys.None;
        public Keys EnableVRAndTracking { get; set; } = Keys.None;
        public Keys EnableVR { get; set; } = Keys.None;
        public Keys EnableTheater { get; set; } = Keys.None;
        public Keys Recenter { get; set; } = Keys.None;
        public Keys Shutdown { get; set; } = Keys.None;
        public bool EnableUDPBroadcast { get; set; }
        public string UDPBroadcastAddress { get; set; } = "255.255.255.255";
        public int UDPBroadcastPort { get; set; } = 9090;
        public int OpenTrackPort { get; set; } = 4242;
        public bool StartMinimized { get; set; } = true;
        public byte ScreenSize { get; set; } = 0x29;
        public byte ScreenDistance { get; set; } = 0x32;
        public byte Brightness { get; set; } = 0x20;
        public byte MicVol { get; set; } = 0;
        public byte LedAIntensity { get; set; } = 0x48;
        public byte LedBIntensity { get; set; } = 0x48;
        public byte LedCIntensity { get; set; } = 0x48;
        public byte LedDIntensity { get; set; } = 0x48;
        public byte LedEIntensity { get; set; } = 0x48;
        public byte LedFIntensity { get; set; } = 0x48;
        public byte LedGIntensity { get; set; } = 0x48;
        public byte LedHIntensity { get; set; } = 0x48;
        public byte LedIIntensity { get; set; } = 0x48;

    }
}
