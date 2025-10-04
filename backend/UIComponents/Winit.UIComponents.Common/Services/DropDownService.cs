using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIComponents.Common.Services
{
    public class DropDownService : IDropDownService
    {
        public event Action<DropDownOptions> OnShowDropDown;
        public event Func<DropDownOptions,Task> OnShowMobilePopUpDropDown;

        public async Task ShowDropDown(DropDownOptions options)
        {
            if (OnShowDropDown != null) {
                OnShowDropDown.Invoke(options);
            }
        }

        public async Task ShowMobilePopUpDropDown(DropDownOptions options)
        {
            if (OnShowMobilePopUpDropDown != null)
            {
               await OnShowMobilePopUpDropDown.Invoke(options);
            }
        }
    }
}
