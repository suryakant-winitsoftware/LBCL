using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIComponents.Web.Location.Services
{
    public class LocationData: ILocationData
    {
        public bool ShowLocationData { get; set; }
        public Action<object> Action { get; set; }
    }
}
