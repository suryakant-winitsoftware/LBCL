using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Models.Configuration
{
    public class RabbitMqConfiguration
    {
        public string HostName { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }

}
