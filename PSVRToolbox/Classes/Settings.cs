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
        public bool EnableOpenTrackSender { get; set; }
        public int OpenTrackPort { get; set; } = 4242;
        public bool StartMinimized { get; set; } = true;
        public byte ScreenSize { get; set; } = 0x29;
        public byte ScreenDistance { get; set; } = 0x32;
        public byte Brightness { get; set; } = 0x20;
        public byte MicVol { get; set; } = 0;
        public bool UseLibUSB { get; set; }

    }
}
