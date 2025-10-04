using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Winit.Modules.Auth.Model.Classes
{
    public class AppDco
    {
        public string AppName { get; set; }
        public int AppId { get; set; }
        public bool IsActive { get; set; }

    }
}
