using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.ErrorHandling.BL.Interfaces;
using Winit.Modules.ErrorHandling.Model.Classes;
using Winit.Modules.ErrorHandling.Model.Interfaces;

namespace Winit.Modules.ErrorHandling.BL.Classes
{
    public abstract class ErrorDescriptionBaseViewModel : IErrorDescriptionViewModel
    {
        public IErrorDescriptionDetails ErrorDescriptionDetails { get; set; }
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private List<string> _propertiesToSearch = new List<string>();
        public IErrorDetailModel ErrorDetailModel { get; set; }


        public ErrorDescriptionBaseViewModel(IServiceProvider serviceProvider,
               IFilterHelper filter,
               ISortHelper sorter,
               IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService
             )
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            ErrorDescriptionDetails = new ErrorDescriptionDetails();
            // Property set for Search
            _propertiesToSearch.Add("Code");
            _propertiesToSearch.Add("Name");
        }
        public async virtual Task PopulateErrorDescriptionDetailsByUID(string error_code)
        {
            Winit.Modules.ErrorHandling.Model.Interfaces.IErrorDescriptionDetails? data = await GetErrorDescriptionDatailsData(error_code);
            if (data != null)
            {
                ErrorDescriptionDetails = data;
            }
        }
        public abstract Task<Winit.Modules.ErrorHandling.Model.Interfaces.IErrorDescriptionDetails>? GetErrorDescriptionDatailsData(string error_code);
    }
}
