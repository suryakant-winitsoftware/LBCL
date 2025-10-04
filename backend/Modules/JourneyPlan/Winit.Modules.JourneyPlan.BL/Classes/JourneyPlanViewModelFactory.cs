using System;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.JourneyPlan.BL.Interfaces;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.JourneyPlan.BL.Classes
{
    public class JourneyPlanViewModelFactory : IJourneyPlanViewModelFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private readonly IAppUser _appUser;
        private readonly IAppConfig _appConfigs;
        private readonly IBeatHistoryBL _beatHistoryBL;
        private readonly IStoreBL _storeBL;
        private readonly Winit.Modules.Base.BL.ApiService _apiService;


        // Property to hold the journey plan view model
        public IJourneyPlanViewModel _viewmodelJp { get;  set; }
        public JourneyPlanViewModelFactory(
            IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper,
            IAppUser appUser,
            IAppConfig appConfigs,
            Winit.Modules.Base.BL.ApiService apiService,
            IBeatHistoryBL beatHistoryBL,
            IStoreBL storeBL)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _appUser = appUser;
            _appConfigs = appConfigs;
            _apiService = apiService;
            _beatHistoryBL = beatHistoryBL;
            _storeBL = storeBL;
        }

        public IJourneyPlanViewModel CreateJourneyPlanViewModel(string screenType)
        {
            switch (screenType)
            {
                case Winit.Shared.Models.Constants.ScreenType.JourneyPlan:
                    _viewmodelJp = new JourneyPlanViewModel(
                        _serviceProvider, _filter, _sorter, _listHelper,
                        _appUser, _appConfigs, _apiService, _beatHistoryBL, _storeBL);
                    break;
                case Winit.Shared.Models.Constants.ScreenType.MyCustomer:
                    _viewmodelJp = new MyCustomerViewModel(
                        _serviceProvider, _filter, _sorter, _listHelper,
                        _appUser, _appConfigs, _apiService, _beatHistoryBL, _storeBL);
                    break;
                default:
                    _viewmodelJp = new JourneyPlanAppViewModel(
                        _serviceProvider, _filter, _sorter, _listHelper,
                        _appUser, _appConfigs, _apiService, _beatHistoryBL, _storeBL);
                    break;
            }
            return _viewmodelJp;
        }

        public void Dispose()
        {
            if (_viewmodelJp != null)
            {
                (_viewmodelJp as IDisposable)?.Dispose();
                _viewmodelJp = null;
            }
        }
    }
}
