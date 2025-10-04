using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.ShareOfShelf.BL.Classes
{
    public class ShareOfShelfWebViewModel : ShareOfShelfBaseViewModel
    {
        public ShareOfShelfWebViewModel(IServiceProvider serviceProvider, IFilterHelper filter, ISortHelper sorter, IListHelper listHelper, IAppUser appUser, IAppSetting appSetting, IDataManager dataManager, IAppConfig appConfig) : base(serviceProvider, filter, sorter, listHelper, appUser, appSetting, dataManager, appConfig)
        {
        }
    }
}
