using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIComponents.Web.Location.Services
{
    public interface ILocationMasterService
    {
        event Action<Action<object>> OnShowLocationEvent;
        bool EventAsigned { get; set; }
        Task ShowLocationMaster(Action<object> action);
    }
}
