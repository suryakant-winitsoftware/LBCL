using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Route.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.WHStock.BL.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.WHStock.BL.Classes
{
    public class WebLoadRequestViewModel
    {
        public WebLoadRequestViewModel(IServiceProvider serviceProvider,

      IFilterHelper filter,
      ISortHelper sorter,
      IListHelper listHelper,
      IAppUser appUser,
      IWHStockBL iWHStockBL,
        ISKUBL sKUBL,
      IAppConfig appConfigs,
      IRouteBL iRouteBL,
      Base.BL.ApiService apiService,
      Winit.Modules.Setting.BL.Interfaces.IAppSetting appSetting)
        {

            
        }
    }
}
