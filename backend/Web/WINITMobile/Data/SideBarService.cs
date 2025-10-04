using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WINITMobile.Data
{
    public class SideBarService
    {
        public Action RefreshSideBar { get; set; }
        //public Action<Winit.UIComponents.Mobile.Enums.SwipeDirection> OnSwipeDirection { get; set; }
        public Action OnCheckOutClick { get; set; }
        public Action OnOpenRouteDD { get; set; }
    }
}
