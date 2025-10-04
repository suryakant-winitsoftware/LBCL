using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL;
using Winit.Modules.Common.BL.Classes;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Classes;
using Winit.Modules.Common.Model.Interfaces;
//using Winit.Modules.SKU.BL.Interfaces;
//using Winit.Modules.SKU.DL.Interfaces;
//using Winit.Modules.SKU.Model.Classes;
//using Winit.Modules.SKU.Model.Interfaces;
//using Winit.Modules.SKU.Model.UIInterfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.Modules.WHStock.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.Route.BL.Interfaces;

namespace Winit.Modules.WHStock.BL.Classes
{
    public abstract class LoadRequestView : LoadRequestBaseViewModel
    {
        public LoadRequestView(IServiceProvider serviceProvider,
    IFilterHelper filter,
    ISortHelper sorter,
    IListHelper listHelper,
    IAppUser appUser,
   IWHStockBL iWHStockBL,
    ISKUBL sKUBL,
    IAppConfig appConfigs,
    IRouteBL iRouteBL,
    Base.BL.ApiService apiService,
    Winit.Modules.Setting.BL.Interfaces.IAppSetting appSetting
    ) : base(serviceProvider, filter, sorter, listHelper, appUser, iWHStockBL, sKUBL,appConfigs, iRouteBL, apiService, appSetting)
        { }

    
}
}
