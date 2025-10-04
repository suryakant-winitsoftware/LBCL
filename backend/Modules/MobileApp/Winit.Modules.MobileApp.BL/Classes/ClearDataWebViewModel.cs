using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL;
using Winit.Modules.Emp.Model.Classes;
using Winit.Modules.Mobile.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;

namespace Winit.Modules.Mobile.BL.Classes
{
    public class ClearDataWebViewModel : ClearDataBaseViewModel
    {
       
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private Winit.Modules.Common.BL.Interfaces.IAppUser _appUser;
        private readonly ILanguageService _languageService;
        private IStringLocalizer<LanguageKeys> _localizer;
        public ClearDataWebViewModel(IServiceProvider serviceProvider,
              IFilterHelper filter,
              ISortHelper sorter,
              IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService, Winit.Modules.Common.BL.Interfaces.IAppUser appUser, IStringLocalizer<LanguageKeys> Localizer,
            ILanguageService languageService)
          : base(serviceProvider, filter, sorter, listHelper, appConfigs, apiService,appUser)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _appUser = appUser;
            _localizer = Localizer;
            _languageService = languageService;
            ClearDataLists = new List<IMobileAppAction>();
            ActionSelectionItems = new List<ISelectionItem>();
            EmpSalesRepSelectionItems = new List<ISelectionItem>();
            MobileAppActionFilterCriterials = new List<FilterCriteria>();
        }
        protected void LoadResources(object sender, string culture)
        {
            CultureInfo cultureInfo = new CultureInfo(culture);
            ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys).Assembly);
            _localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
        }
        public override async Task PopulateViewModel()
        {
            LoadResources(null, _languageService.SelectedCulture);
            await base.PopulateViewModel();          
        }
        public override async Task PopulateViewModelForDD()
        {
            await base.PopulateViewModelForDD();
            List<ISelectionItem> salesRepSelectionItems = Winit.Shared.CommonUtilities.Common.CommonFunctions.ConvertToSelectionItems
                <Winit.Modules.Mobile.Model.Interfaces.IUser>(SalesRep, new List<string> { "UID", "Code", "Name" });
            EmpSalesRepSelectionItems.Clear();
            EmpSalesRepSelectionItems.AddRange(salesRepSelectionItems);
            ActionSelectionItems = ConvertActionToSelectionItem();
        }
        #region Business Logics      
        private List<ISelectionItem> ConvertActionToSelectionItem()
        {
            List<ISelectionItem> selectionItems = new List<ISelectionItem>();
            SelectionItem noActionItem = new SelectionItem
            {
                Code =  "NO_ACTION",
                Label = @_localizer["no_action"],
                UID = "NO_ACTION"
            };
            selectionItems.Add(noActionItem);
            SelectionItem clearDataItem = new SelectionItem
            {
                Code = "Clear Data after Upload",
                Label = @_localizer["clear_data_after_upload"],
                UID = "Clear Data after Upload"
            };
            selectionItems.Add(clearDataItem);
            return selectionItems;
        }
        #endregion
        #region Database or Services Methods
        public override async Task<List<Winit.Modules.Mobile.Model.Interfaces.IMobileAppAction>> GetClearData()
        {
            return await GetClearDataFromAPIAsync();
        }
        public override async Task<bool> SaveClearData_Data(List<IMobileAppAction> mobileAppAction)
        {
            return await CreateUpdateOrgDataFromAPIAsync(mobileAppAction);
        }
        public override async Task<List<Winit.Modules.Mobile.Model.Interfaces.IUser>> GetSalesRepDropdownData()
        {
            return await GetSalesRepDropdownDataFromAPIAsync();
        }
        public override async Task<List<ISelectionItem>> GetSalesmanData(string OrgUID)
        {
            return await GetSalesmanDataFromAPIAsync(OrgUID);
        }
        #endregion
        #region Api Calling Methods
        private async Task<List<ISelectionItem>> GetSalesmanDataFromAPIAsync(string OrgUID, bool getDataByLoginId = true)
        {
            try

            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Dropdown/GetEmpDropDown?orgUID={OrgUID}&getDataByLoginId={getDataByLoginId}",
                    HttpMethod.Post);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<List<SelectionItem>> Response = JsonConvert.DeserializeObject<ApiResponse<List<SelectionItem>>>(apiResponse.Data);
                    return Response.Data.ToList<ISelectionItem>();
                }
            }
            catch (Exception)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
        protected async Task<List<Winit.Modules.Mobile.Model.Interfaces.IMobileAppAction>> GetClearDataFromAPIAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.FilterCriterias = MobileAppActionFilterCriterials;
                pagingRequest.SortCriterias = this.SortCriterias;
                pagingRequest.PageNumber = PageNumber;
                pagingRequest.PageSize = PageSize;
                pagingRequest.IsCountRequired = true;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}MobileAppAction/GetClearDataDetails",
                    HttpMethod.Post, pagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                   ApiResponse<PagedResponse<Winit.Modules.Mobile.Model.Classes.MobileAppAction>> selectionORGs = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Mobile.Model.Classes.MobileAppAction>>>(apiResponse.Data);
                    if (selectionORGs.Data.PagedData != null)
                    {
                        TotalItemsCount = selectionORGs.Data.TotalCount;
                        return selectionORGs.Data.PagedData.OfType<Winit.Modules.Mobile.Model.Interfaces.IMobileAppAction>().ToList();
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }
        protected async Task<List<Winit.Modules.Mobile.Model.Interfaces.IUser>> GetSalesRepDropdownDataFromAPIAsync()
        {
            try
            {

                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.FilterCriterias = new List<FilterCriteria>();
                ApiResponse<IEnumerable<Winit.Modules.Mobile.Model.Classes.User>> apiResponse =
                   await _apiService.FetchDataAsync<IEnumerable<Winit.Modules.Mobile.Model.Classes.User>>(
                   $"{_appConfigs.ApiBaseUrl}MobileAppAction/GetUserDDL?OrgUID=WINIT",
                   HttpMethod.Post);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data.OfType<IUser>().ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }
        private async Task<bool> CreateUpdateOrgDataFromAPIAsync(List<IMobileAppAction> mobileAppAction)
        {
            try
            {
                ApiResponse<string> apiResponse = null;
                string jsonBody = JsonConvert.SerializeObject(mobileAppAction);
                apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}MobileAppAction/PerformCUD", HttpMethod.Post, mobileAppAction);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    if (apiResponse.IsSuccess != null && apiResponse.IsSuccess)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }
        #endregion
    }
}
