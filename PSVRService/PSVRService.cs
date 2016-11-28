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
