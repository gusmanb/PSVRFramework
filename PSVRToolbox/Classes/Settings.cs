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
        public Keys Recalibrate { get; set; } = Keys.None;
        public Keys EnableVR { get; set; } = Keys.None;
        public Keys EnableTheater { get; set; } = Keys.None;
        public Keys Recenter { get; set; } = Keys.None;
        public Keys Shutdown { get; set; } = Keys.None;
        public bool Standalone { get; set; } = false;
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
