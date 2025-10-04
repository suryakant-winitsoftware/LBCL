using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;


namespace Winit.Modules.Org.BL.Classes
{
    public class OrgViewModel : OrgBaseViewModel
    {
        public OrgViewModel(IServiceProvider serviceProvider,
               IFilterHelper filter,
               ISortHelper sorter,
               IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService)
           : base(serviceProvider, filter, sorter, listHelper, appConfigs, apiService)
        {
        }
    }
}
