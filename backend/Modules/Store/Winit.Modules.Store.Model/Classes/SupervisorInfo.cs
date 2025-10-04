using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.Model.Classes
{
    public class SupervisorInfo : ISupervisorInfo
    {
        public int Sn { get; set; }

        public string Name { get; set; }
        public string Age { get; set; }
        public string Qualification { get; set; }
    }
}
