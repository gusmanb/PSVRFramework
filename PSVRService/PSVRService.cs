using PSVRFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace PSVRService
{
    public partial class PSVRService : ServiceBase
    {

        PSVRServer server;

        public PSVRService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (server == null)
                server = new PSVRServer(IPAddress.Parse("127.0.0.1"), 9354);            
        }

        protected override void OnStop()
        {
            if (server != null)
            {
                server.Device?.Controller.Shutdown();
                server.Dispose();
            }
        }
    }
}
