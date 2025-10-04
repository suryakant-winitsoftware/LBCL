using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Location.Model.Interfaces;

namespace Winit.Modules.Location.Model.Classes
{
    public class LocationTypeAndValue :ILocationTypeAndValue
    {
        public string LocationType { get; set; }
        public string LocationValue { get; set; }
    }
}
