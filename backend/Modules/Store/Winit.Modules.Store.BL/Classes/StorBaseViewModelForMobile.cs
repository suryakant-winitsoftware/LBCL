using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Store.BL.Interfaces;

namespace Winit.Modules.Store.BL.Classes
{
    public class StorBaseViewModelForMobile: IStorBaseViewModelForMobile
    {
        public StorBaseViewModelForMobile( IAppUser appUser)
        {
            _appUser= appUser;
        }
        IAppUser _appUser { get; set; }


    }
}
