using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIComponents.Web.Location.Services
{
    public interface ILocationData
    {
        bool ShowLocationData { get; set; }
        Action<object> Action { get; set; }
    }
}
