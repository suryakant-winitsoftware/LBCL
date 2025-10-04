using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Provisioning.BL.Interfaces;
using Winit.Modules.Provisioning.Model.Classes;
using Winit.Modules.Provisioning.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Provisioning.BL.Classes
{
    public abstract class ProvisioningItemViewBaseViewModel : IProvisioningItemViewViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private List<string> _propertiesToSearch = new List<string>();
        private Winit.Modules.Common.BL.Interfaces.IAppUser _appUser;
        public List<IProvisionItemView> ProvisionItemDataList { get; set; }
        public IProvisionItemView ProvisionItemViewDetails { get; set; }
        public ProvisioningItemViewBaseViewModel(IServiceProvider serviceProvider,
           IFilterHelper filter,
        ISortHelper sorter,
            IAppUser appUser,
           IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService
         )
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _appUser = appUser;
            _propertiesToSearch.Add("Code");
            _propertiesToSearch.Add("Name");
            ProvisionItemDataList = new List<IProvisionItemView>();
            ProvisionItemViewDetails = new ProvisionItemView();
        }
      
        public virtual async Task GetProvisionItemViewData(string? UID)
        {
            ProvisionItemDataList = await GetProvisionItemDataList(UID);
        }
       

        public async Task GetProvisioningItemDetailsByUID(string provisionItemUID)
        {
            ProvisionItemViewDetails = await GetProvisioningItemDetailsByUid(provisionItemUID);
        }

        public abstract Task<IProvisionItemView> GetProvisioningItemDetailsByUid(string provisionItemUID);
        public abstract Task<List<IProvisionItemView>> GetProvisionItemDataList(string UID);
    }
}
