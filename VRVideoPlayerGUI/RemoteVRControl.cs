using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace VRVideoPlayerGUI
{
    public static class RemoteVRControl
    {
        private static void SendCommand(RemoteCommand Command)
        {
            string ser = JsonConvert.SerializeObject(Command);
            byte[] data = Encoding.UTF8.GetBytes(ser);


            var ep = new IPEndPoint(IPAddress.Parse("255.255.255.255"), 14598);
            var client = new UdpClient();
            client.EnableBroadcast = true;
            client.Send(data, data.Length, ep);
            client.Close();
        }

        public static void PowerOnHeadset()
        {
            RemoteCommand cmd = new RemoteCommand { Command = "HeadsetOn" };
            SendCommand(cmd);
        }

        public static void SwitchToVR()
        {
            RemoteCommand cmd = new RemoteCommand { Command = "EnableVRMode" };
            SendCommand(cmd);
        }

        public static void SwitchToCinematic()
        {
            RemoteCommand cmd = new RemoteCommand { Command = "EnableCinematicMode" };
            SendCommand(cmd);
        }
    }

    public class RemoteCommand
    {
        public string Command { get; set; }
    }

    public class CinematicSettingsCommand : RemoteCommand
    {
        public byte? Distance { get; set; }
        public byte? Size { get; set; }
        public byte? Brightness { get; set; }
    }

    public class LEDSettingsCommand : RemoteCommand
    {
        public byte? LedA { get; set; }
        public byte? LedB { get; set; }
        public byte? LedC { get; set; }
        public byte? LedD { get; set; }
        public byte? LedE { get; set; }
        public byte? LedF { get; set; }
        public byte? LedG { get; set; }
        public byte? LedH { get; set; }
        public byte? LedI { get; set; }
    }
}
