using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.ExpiryCheck.BL.Interfaces;
using Winit.Modules.ExpiryCheck.Model.Classes;
using Winit.Modules.ExpiryCheck.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.ExpiryCheck.BL.Classes
{
    public class ExpiryCheckBaseViewModel : IExpiryCheckViewModel
    {
        // Injection
        private IServiceProvider _serviceProvider;
        protected readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;

        private readonly IListHelper _listHelper;
        protected readonly IAppUser _appUser;
        private readonly IAppSetting _appSetting;
        protected readonly IDataManager _dataManager;
        private readonly IAppConfig _appConfig;
        public IExpiryCheckExecution expiryCheckHeader { get; set;  }
        public List<ExpiryCheckItem> availableProducts { get; set; }
        public ExpiryCheckBaseViewModel(IServiceProvider serviceProvider,
                IFilterHelper filter,
                ISortHelper sorter,
                IListHelper listHelper,
                IAppUser appUser,
                IAppSetting appSetting,
                IDataManager dataManager,
                IAppConfig appConfig)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;

            _listHelper = listHelper;
            _appUser = appUser;
            _appSetting = appSetting;
            _dataManager = dataManager;
            _appConfig = appConfig;
            expiryCheckHeader = new ExpiryCheckExecution();
            availableProducts = new List<ExpiryCheckItem>();
        }

        public virtual async Task PopulateViewModel()
        {

        }
        
        
        public virtual async Task<string> OnSubmitExpiryCheck()
        {
            return default;
        }
    }
}
