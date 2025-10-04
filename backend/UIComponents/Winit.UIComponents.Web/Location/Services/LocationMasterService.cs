using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIComponents.Web.Location.Services
{
    public class LocationMasterService : ILocationMasterService
    {
        Winit.UIComponents.Web.Location.Services.ILocationData LocationMasterServices { get; set; }
        public bool EventAsigned { get; set; }
        public event Action<Action<object>> OnShowLocationEvent;
        public LocationMasterService(Winit.UIComponents.Web.Location.Services.ILocationData locationMasterService) 
        {
            LocationMasterServices=locationMasterService;
        }
        
        public async Task ShowLocationMaster(Action<object> action)
        {
            LocationMasterServices.ShowLocationData = true;
            LocationMasterServices.Action = action;
            OnShowLocationEvent.Invoke(action);
        }
    }
}
