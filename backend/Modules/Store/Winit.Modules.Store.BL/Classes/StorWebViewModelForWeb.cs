using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Address.Model.Classes;
using Winit.Modules.Address.Model.Interfaces;
using Winit.Modules.AwayPeriod.Model.Interfaces;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Contact.Model.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.ListHeader.Model.Classes;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Route.Model.Classes;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Constants;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.LanguageResources.Web;
using Winit.UIModels.Web.Store;

namespace Winit.Modules.Store.BL.Classes
{  
    public class StorWebViewModelForWeb : StorBaseViewModelForWeb
    {
        protected Winit.UIComponents.Common.Services.ILoadingService _loadingService;
        protected IAppUser _iAppUser { get; set; }
        private ApiService _apiService { get; set; }
        private Winit.Shared.Models.Common.IAppConfig _appConfigs { get; set; }
        private CommonFunctions _commonFunctions { get; set; }
        private NavigationManager _navigationManager { get; set; }
        private Common.Model.Interfaces.IDataManager _dataManager { get; set; }
        private IAlertService _alertService { get; set; }
        private Winit.UIComponents.SnackBar.IToast _toast { get; set; }
        private IStringLocalizer<LanguageKeys> _localizer;

        public StorWebViewModelForWeb(ApiService apiService, Winit.Shared.Models.Common.IAppConfig appConfigs, CommonFunctions commonFunctions,
            NavigationManager navigationManager, Winit.Modules.Common.Model.Interfaces.IDataManager dataManager, IAlertService alertService, IAppUser iAppUser, IStringLocalizer<LanguageKeys> Localizer,
            Winit.UIComponents.Common.Services.ILoadingService loadingService, UIComponents.SnackBar.IToast toast) : base(commonFunctions, navigationManager, dataManager, alertService, iAppUser, loadingService)
        {
            this._apiService = apiService;
            this._appConfigs = appConfigs;
            this._commonFunctions = commonFunctions;
            this._navigationManager = navigationManager;
            this._dataManager = dataManager;
            this._alertService = alertService;
            this._iAppUser = iAppUser;
            _localizer = Localizer;
            this._loadingService = loadingService;
            _toast = toast;
        }
        public List<Modules.Location.Model.Classes.Location> Locations { get; set; } = new();
        public List<Modules.Location.Model.Classes.LocationType> LocationType { get; set; } = new();
        public List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> fileSys { get; set; } = new();
        protected override async Task GetDropDownLists()
        {
            //await GetLocationTypeDetails();
            await GetSalesOrganisation();
            await getListItemsAsync();
            await GetRoute();
        }


        #region API Calling Methodes

        #region DROP DOWN APIS
        protected async Task GetLocationTypeDetails()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.IsCountRequired = true;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}LocationType/SelectAllLocationTypeDetails",
                HttpMethod.Post, pagingRequest);
                if (apiResponse.Data != null)
                {
                    var data = _commonFunctions.GetDataFromResponse(apiResponse.Data);
                    PagedResponse<Modules.Location.Model.Classes.LocationType> pagedResponse = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Location.Model.Classes.LocationType>>(data);
                    if (pagedResponse != null)
                    {

                        if (pagedResponse.TotalCount > 0)
                        {
                            LocationType = pagedResponse.PagedData.ToList();
                        }
                    }

                }
            }
            catch (Exception ex)
            {

            }
        }
        protected async Task GetLocationDetails()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.IsCountRequired = true;

                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}Location/SelectAllLocationDetails",
                HttpMethod.Post, pagingRequest);
                if (apiResponse.Data != null)
                {
                    var data = _commonFunctions.GetDataFromResponse(apiResponse.Data);
                    PagedResponse<Modules.Location.Model.Classes.Location> pagedResponse = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Location.Model.Classes.Location>>(data);
                    if (pagedResponse != null)
                    {
                        try
                        {
                            if (pagedResponse.TotalCount > 0)
                            {
                                Locations = pagedResponse.PagedData.ToList();
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        protected async Task getListItemsAsync()
        {
            try
            {
                ListItemRequest request = new()
                {
                    Codes = new()
                {
                    "RouteType",
                    "CustomerType",
                    "Designation",
                    "PriceType",
                    "BlockedBy",
                    "BDM",
                    "DocumentType",
                    "CustomerChain",
                    "CustomerGroup",
                    "CustomerClassifciation ",
                    "PaymentMethod",
                    "PaymentTerm",
                    "PaymentType",
                    "InvoiceFrequency",
                    "InvoiceFormat",
                    "InvoiceDeliveryMethod"
                },
                    isCountRequired = true
                };
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                 $"{_appConfigs.ApiBaseUrl}ListItemHeader/GetListItemsByCodes",
                 HttpMethod.Post, request);
                if (apiResponse.Data != null)
                {
                    var data = _commonFunctions.GetDataFromResponse(apiResponse.Data);
                    PagedResponse<Winit.Modules.ListHeader.Model.Classes.ListItem> pagedResponse = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.ListHeader.Model.Classes.ListItem>>(data);
                    if (pagedResponse != null)
                    {
                        try
                        {
                            if (pagedResponse.TotalCount > 0)
                            {
                                ListItems = pagedResponse.PagedData.OrderBy(p => p.SerialNo).ToList();
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                }
            }
            catch (Exception ex)
            {

            }
        }
        private async Task GetSalesOrganisation()
        {
            SalesOrgList = new();
            PagingRequest pagingRequest = new()
            {
                FilterCriterias = new()
                {
                    new Shared.Models.Enums.FilterCriteria("OrgTypeUID","DC",Shared.Models.Enums.FilterType.Equal),
                },
                IsCountRequired = true,
            };

            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}Org/GetOrgDetails",
            HttpMethod.Post, pagingRequest);

            if (apiResponse.IsSuccess)
            {
                var data = _commonFunctions.GetDataFromResponse(apiResponse.Data);
                PagedResponse<Winit.Modules.Org.Model.Classes.Org> pagedResponse = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Org.Model.Classes.Org>>(data);
                if (pagedResponse != null)
                {
                    try
                    {
                        if (pagedResponse.TotalCount > 0)
                        {

                            foreach (var item in pagedResponse.PagedData)
                            {
                                SalesOrgList.Add(new SelectionItem() { Code = item.Code, UID = item.UID, Label = item.Name });
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }

        private async Task GetRoute()
        {
            Route = new();
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
             $"{_appConfigs.ApiBaseUrl}Route/GetRoutesByStoreUID?OrgUID={_iAppUser.SelectedJobPosition.OrgUID}&StoreUID={StoreUID}",
             HttpMethod.Get);
            if (apiResponse.Data != null)
            {
                ApiResponse<List<Modules.Route.Model.Classes.Route>>? pagedResponse = JsonConvert.DeserializeObject<ApiResponse<List<Modules.Route.Model.Classes.Route>>>(apiResponse.Data);
                if (pagedResponse != null && pagedResponse.Data != null)
                {
                    try
                    {
                        foreach (var item in pagedResponse.Data)
                        {
                            Route.Add(new SelectionItem()
                            {
                                UID = item.UID,
                                Code = item.Code,
                                Label = item.Name,
                                IsSelected = false,
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        // isNewStore = true;
                        // GetStatus(ex.Message);
                    }
                }

            }
        }
        #endregion

        #region Store
        public async Task CreateStoreImage(List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> files)
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
              $"{_appConfigs.ApiBaseUrl}FileSys/CreateFileSysForBulk", HttpMethod.Post, files);
            if (apiResponse != null && apiResponse.IsSuccess)
            {
                //ApiResponse<string>? response = JsonConvert.DeserializeObject<PagedResponse<string>>(apiResponse.Data);
            }
        }
        protected override async Task GetStoreImages()
        {
            PagingRequest request = new()
            {
                FilterCriterias = new()
                {
                    new FilterCriteria("LinkedItemUID",StoreUID,FilterType.Equal),
                }
            };
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
              $"{_appConfigs.ApiBaseUrl}FileSys/SelectAllFileSysDetails", HttpMethod.Post, request);
            if (apiResponse != null && apiResponse.IsSuccess)
            {
                PagedResponse<FileSys.Model.Classes.FileSys>? response = (JsonConvert.DeserializeObject<PagedResponse<FileSys.Model.Classes.FileSys>>(_commonFunctions.GetDataFromResponse(apiResponse.Data)));
                FileSysList = response?.PagedData?.ToList<IFileSys>();
            }
        }
        protected override async Task GetCustomerInfo()
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
              $"{_appConfigs.ApiBaseUrl}Store/SelectStoreByUID?UID=" + StoreUID, HttpMethod.Get);
            if (apiResponse.Data != null)
            {
                CustomerInformation = JsonConvert.DeserializeObject<Winit.Modules.Store.Model.Classes.Store>(_commonFunctions.GetDataFromResponse(apiResponse.Data));
                if (CustomerInformation != null)
                {
                    var data = GetLocationLabelByPrimaryUID(CustomerInformation.LocationUID);
                    CustomerInformation.LocationLabel = data.Item1;
                    CustomerInformation.LocationJson= data.Item2;
                }
            }
        }
        public async Task<int> SaveCustomerInfo()
        {
            int retval = 0;
            if (IsNewStore)
            {
                CustomerInformation.UID = StoreUID;
                CustomerInformation.CreatedTime = DateTime.Now;
                CustomerInformation.ModifiedTime = DateTime.Now;
                CustomerInformation.ServerAddTime = DateTime.Now;
                CustomerInformation.ServerModifiedTime = DateTime.Now;
                CustomerInformation.CreatedByEmpUID = _iAppUser?.Emp?.UID;
                CustomerInformation.CompanyUID = _iAppUser?.Emp?.CompanyUID;
                CustomerInformation.CreatedByJobPositionUID = _iAppUser.SelectedJobPosition.UID;
                CustomerInformation.CreatedBy = _iAppUser.Emp.UID;
                CustomerInformation.ModifiedBy = _iAppUser.Emp.UID;
                CustomerInformation.FranchiseeOrgUID = _iAppUser.SelectedJobPosition.OrgUID;
                CustomerInformation.Type = "FRC";
            }
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
             $"{_appConfigs.ApiBaseUrl}store/CreateStore",
             HttpMethod.Post, CustomerInformation);

            if (apiResponse.StatusCode == 200)
            {
                IsDisabled = false;
                IsNewStore = false;
                retval = CommonFunctions.GetIntValue(_commonFunctions.GetDataFromResponse(apiResponse.Data));
                _toast.Add(@_localizer["success"], @_localizer["saved_successfully"], UIComponents.SnackBar.Enum.Severity.Success);
                //await _alertService.ShowSuccessAlert("Success", "Saved Successfully");
            }
            else
            {
                _toast.Add("Error", $"[{apiResponse.StatusCode}]{apiResponse.ErrorMessage}", UIComponents.SnackBar.Enum.Severity.Error);
            }
            return retval;
        }
        public async Task<int> UpdateCustomerInfo()
        {
            int retval = 0;
            CustomerInformation.ModifiedTime = DateTime.Now;
            CustomerInformation.ServerModifiedTime = DateTime.Now;
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
             $"{_appConfigs.ApiBaseUrl}Store/UpdateStore", HttpMethod.Put, CustomerInformation);
            if (apiResponse.StatusCode == 200)
            {
                retval = CommonFunctions.GetIntValue(_commonFunctions.GetDataFromResponse(apiResponse.Data));
                _toast.Add(@_localizer["success"], @_localizer["updated_successfully"], UIComponents.SnackBar.Enum.Severity.Success);
            }
            else
            {
                _toast.Add(@_localizer["error"], $"[{apiResponse.StatusCode}]{apiResponse.ErrorMessage}", UIComponents.SnackBar.Enum.Severity.Error);
            }
            return retval;
        }
        #endregion

        #region Contact
        public async Task GetContactsByStoreUID()
        {
            PagingRequest paging = new PagingRequest();
            paging.FilterCriterias = new();
            paging.FilterCriterias.Add(new FilterCriteria("LinkedItemUID", StoreUID, FilterType.Equal));
            paging.FilterCriterias.Add(new FilterCriteria("LinkedItemType", "store", FilterType.Equal));
            paging.IsCountRequired = true;
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                  $"{_appConfigs.ApiBaseUrl}Contact/SelectAllContactDetails",
              HttpMethod.Post, paging);
            if (apiResponse.Data != null)
            {
                var data = _commonFunctions.GetDataFromResponse(apiResponse.Data);
                PagedResponse<Winit.Modules.Contact.Model.Classes.Contact> pagedResponse = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Contact.Model.Classes.Contact>>(data);
                if (pagedResponse != null)
                {
                    if (pagedResponse.TotalCount > 0)
                    {
                        var contact = pagedResponse.PagedData.FirstOrDefault();
                        if (contact != null)
                        {
                            Contact = pagedResponse.PagedData.FirstOrDefault();
                        }
                    }
                }
            }
        }
        public async Task<int> SaveOrUpdateContacts()
        {
            int retval = 0;
            bool isVal = true;
            string message = string.Empty;
            if (string.IsNullOrEmpty(Contact?.Mobile))
            {
                message += @_localizer["mobile_number"]+", " ;
                isVal = false;
            }
            if (string.IsNullOrEmpty(Contact?.Mobile2))
            {
                message += @_localizer["landline_number"] +", ";
                isVal = false;
            }
            if (string.IsNullOrEmpty(Contact?.PhoneExtension))
            {
                message += @_localizer["phone_extension"]+", ";
                isVal = false;
            }
            if (string.IsNullOrEmpty(Contact?.Email))
            {
                message += @_localizer["email"] +", ";
                isVal = false;
            }
            if (string.IsNullOrEmpty(Contact?.Fax))
            {
                message += @_localizer["fax_number"]+ ", ";
                isVal = false;
            }

            if (isVal)
            {
                if (string.IsNullOrEmpty(Contact.UID))
                {
                    Contact.UID = Guid.NewGuid().ToString();
                    Contact.LinkedItemUID = StoreUID; ;
                    Contact.LinkedItemType = "store"; 
                    Contact.CreatedTime = DateTime.Now;
                    Contact.ModifiedTime = DateTime.Now;
                    Contact.ServerAddTime = DateTime.Now;
                    Contact.ServerModifiedTime = DateTime.Now;
                    Contact.CreatedBy = _iAppUser.Emp.CreatedBy;
                    Contact.ModifiedBy = _iAppUser.Emp.CreatedBy;
                    ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                  $"{_appConfigs.ApiBaseUrl}Contact/CreateContactDetails",
                  HttpMethod.Post, Contact);
                    if (apiResponse.StatusCode == 200)
                    {
                        retval = CommonFunctions.GetIntValue(_commonFunctions.GetDataFromResponse(apiResponse.Data));
                        _toast.Add(@_localizer["success"], @_localizer["saved_successfully"], UIComponents.SnackBar.Enum.Severity.Success);
                    }
                    else
                    {
                        Contact.UID = string.Empty;
                        _toast.Add(@_localizer["error"], $"[{apiResponse.StatusCode}]{apiResponse.ErrorMessage}", UIComponents.SnackBar.Enum.Severity.Error);
                    }
                }
                else
                {
                    ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                     $"{_appConfigs.ApiBaseUrl}Contact/UpdateContactDetails",
                  HttpMethod.Put, Contact);
                    if (apiResponse.StatusCode == 200)
                    {
                        retval = CommonFunctions.GetIntValue(_commonFunctions.GetDataFromResponse(apiResponse.Data));
                        _toast.Add(@_localizer["success"], @_localizer["updated_successfully"], UIComponents.SnackBar.Enum.Severity.Success);
                    }
                    else
                    {
                        _toast.Add(@_localizer["error"], $"[{apiResponse.StatusCode}]{apiResponse.ErrorMessage}", UIComponents.SnackBar.Enum.Severity.Error);
                    }
                }
            }
            else
            {
                _toast.Add(@_localizer["error"], message, UIComponents.SnackBar.Enum.Severity.Error);
            }
            return retval;
        }
        #endregion

        #region Contact Person
        public async Task GetContactsPersonDetails()
        {
            PagingRequest paging = new PagingRequest();
            paging.FilterCriterias = new();
            paging.FilterCriterias.Add(new FilterCriteria("LinkedItemUID", StoreUID, FilterType.Equal));
            paging.FilterCriterias.Add(new FilterCriteria("LinkedItemType", "person", FilterType.Equal));

            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                  $"{_appConfigs.ApiBaseUrl}contact/SelectAllContactDetails",
              HttpMethod.Post, paging);
            if (apiResponse.Data != null)
            {
                var data = _commonFunctions.GetDataFromResponse(apiResponse.Data);
                PagedResponse<Contact.Model.Classes.Contact>? pagedResponse = JsonConvert.DeserializeObject<PagedResponse<Contact.Model.Classes.Contact>>(data);
                if (pagedResponse != null && pagedResponse.PagedData != null && pagedResponse.PagedData.Any())
                {
                    ContactPersonList = pagedResponse.PagedData.ToList<IContact>();
                    if (pagedResponse.TotalCount >= 0)
                    {

                    }
                }

            }
        }

        public async Task<int> SaveContactPerson(Contact.Model.Interfaces.IContact? ContactPerson)
        {
            int retVal = 0;
            ContactPerson.UID = Guid.NewGuid().ToString();
            ContactPerson.LinkedItemUID = StoreUID;
            ContactPerson.LinkedItemType = "person";
            ContactPerson.CreatedBy = _iAppUser.Emp.UID;
            ContactPerson.ModifiedBy = _iAppUser.Emp.UID;
            ContactPerson.CreatedTime = DateTime.Now;
            ContactPerson.ModifiedTime = DateTime.Now;
            ContactPerson.ServerAddTime = DateTime.Now;
            ContactPerson.ServerModifiedTime = DateTime.Now;
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
          $"{_appConfigs.ApiBaseUrl}Contact/CreateContactDetails",
          HttpMethod.Post, ContactPerson);
            if (apiResponse.StatusCode == 200)
            {
                retVal = CommonFunctions.GetIntValue(apiResponse.Data);
                ContactPersonList.Add(ContactPerson);
                _toast.Add(@_localizer["success"], @_localizer["saved_successfully"], UIComponents.SnackBar.Enum.Severity.Success);
                ContactPerson = new Contact.Model.Classes.Contact();
            }
            else
            {
                _toast.Add(@_localizer["error"], apiResponse.StatusCode + apiResponse.ErrorMessage, UIComponents.SnackBar.Enum.Severity.Success);
            }
            return retVal;
        }
        public async Task<int> UpdateContactPerson(Contact.Model.Interfaces.IContact? ContactPerson)
        {
            int retval = 0;
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}Contact/UpdateContactDetails",
             HttpMethod.Put, ContactPerson);

            if (apiResponse.StatusCode == 200)
            {
                retval = CommonFunctions.GetIntValue(_commonFunctions.GetDataFromResponse(apiResponse.Data));
            }
            else
            {
                _toast.Add(@_localizer["error"], apiResponse.StatusCode + apiResponse.ErrorMessage, UIComponents.SnackBar.Enum.Severity.Success);
            }
            return retval;
        }

        #endregion

        #region Address
        protected override async Task<IAddress> GetAddress()
        {
            IAddress address = new Address.Model.Classes.Address();
            PagingRequest paging = new PagingRequest();
            paging.FilterCriterias = new List<FilterCriteria>()
            {
                new FilterCriteria("LinkedItemUID",StoreUID,FilterType.Equal)
            };
            paging.IsCountRequired = true;
            List<AddressModel> addresses = new List<AddressModel>();
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
              $"{_appConfigs.ApiBaseUrl}Address/SelectAllAddressDetails",
              HttpMethod.Post, paging);
            if (apiResponse.Data != null)
            {
                string data = _commonFunctions.GetDataFromResponse(apiResponse.Data);
                if (!string.IsNullOrEmpty(data))
                {
                    PagedResponse<Address.Model.Classes.Address> pagedResponse = JsonConvert.DeserializeObject<PagedResponse<Address.Model.Classes.Address>>(data);
                    if (pagedResponse != null && pagedResponse.PagedData != null)
                    {
                        if (pagedResponse?.PagedData?.FirstOrDefault() != null)
                        {
                            Addresses = pagedResponse?.PagedData?.ToList<IAddress>();
                        }
                    }
                }

            }
            return address;

        }

        public async Task<int> SaveAddress(IAddress address, string type)
        {
            int retval = 0;
            address.UID = Guid.NewGuid().ToString();
            address.LinkedItemUID = StoreUID;
            address.Type = type.ToLower();
            address.LinkedItemType =StoreConstants.Store;
            address.CreatedBy = _iAppUser.Emp.UID;
            address.ModifiedBy = _iAppUser.Emp.UID;
            address.CreatedTime = DateTime.Now;
            address.ModifiedTime = DateTime.Now;
            address.ServerAddTime = DateTime.Now;
            address.ServerModifiedTime = DateTime.Now;

            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
             $"{_appConfigs.ApiBaseUrl}Address/CreateAddressDetails",
             HttpMethod.Post, address);
            if (apiResponse.StatusCode == 200)
            {
                retval = CommonFunctions.GetIntValue(_commonFunctions.GetDataFromResponse(apiResponse.Data));
                if (retval > 0)
                {
                    Addresses.Add(address);
                }
                _toast.Add(@_localizer["success"], type + @_localizer["saved_successfully"], UIComponents.SnackBar.Enum.Severity.Success);
            }
            else
            {
                _toast.Add(@_localizer["error"], $"[{apiResponse.StatusCode}]{apiResponse.ErrorMessage}", UIComponents.SnackBar.Enum.Severity.Success);
            }
            return retval;
        }
        public async Task<int> UpdateAddress(IAddress address, string type)
        {
            int retval = 0;
            address.ModifiedTime = DateTime.Now;
            address.ModifiedBy = _iAppUser.Emp.UID;
            address.ServerModifiedTime = DateTime.Now;

            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
           $"{_appConfigs.ApiBaseUrl}Address/UpdateAddressDetails",
            HttpMethod.Put, address);
            if (apiResponse.StatusCode == 200)
            {

                retval = CommonFunctions.GetIntValue(_commonFunctions.GetDataFromResponse(apiResponse.Data));
                _toast.Add(@_localizer["success"], type + @_localizer["updated_successfully"], UIComponents.SnackBar.Enum.Severity.Success);
            }
            else
            {
                _toast.Add(@_localizer["error"], $"[{apiResponse.StatusCode}]{apiResponse.ErrorMessage}", UIComponents.SnackBar.Enum.Severity.Success);

            }
            return retval;
        }
        #endregion

        #region Away Period
        public async Task UpdateAwayPeriod()
        {
            AwayPeriod.ModifiedTime = DateTime.Now;
            AwayPeriod.ServerModifiedTime = DateTime.Now;
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
           $"{_appConfigs.ApiBaseUrl}AwayPeriod/UpdateAwayPeriodDetails",
             HttpMethod.Put, AwayPeriod);

            if (apiResponse.StatusCode == 200)
            {
                //isEdited = false;
                _toast.Add(@_localizer["success"], @_localizer["updated_successfully"], UIComponents.SnackBar.Enum.Severity.Success);
                AwayPeriod = new AwayPeriod.Model.Classes.AwayPeriod();
            }
            else
            {
                _toast.Add(@_localizer["error"], $"[{apiResponse.StatusCode}] {apiResponse.ErrorMessage}", UIComponents.SnackBar.Enum.Severity.Success);
            }
        }

        public async Task saveAwayPeriod()
        {
            AwayPeriod.LinkedItemUID = StoreUID;
            AwayPeriod.UID = Guid.NewGuid().ToString();
            AwayPeriod.CreatedBy = _iAppUser.Emp.UID;
            AwayPeriod.ModifiedBy = _iAppUser.Emp.UID;
            AwayPeriod.CreatedTime = DateTime.Now;
            AwayPeriod.ModifiedTime = DateTime.Now;
            AwayPeriod.ServerAddTime = DateTime.Now;
            AwayPeriod.ServerModifiedTime = DateTime.Now;

            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}AwayPeriod/CreateAwayPeriodDetails",
            HttpMethod.Post, AwayPeriod);

            if (apiResponse.StatusCode == 200)
            {
                _toast.Add(@_localizer["success"], @_localizer["saved_successfully"], UIComponents.SnackBar.Enum.Severity.Success);

                AwayPeriodList.Add(AwayPeriod);
                AwayPeriod = new AwayPeriod.Model.Classes.AwayPeriod();
            }
            else
            {
                _toast.Add(@_localizer["error"], $"[{apiResponse.StatusCode}] {apiResponse.ErrorMessage}", UIComponents.SnackBar.Enum.Severity.Success);
            }


        }
        public async Task GetAwayPeriodDetails()
        {
            PagingRequest paging = new PagingRequest();
            paging.FilterCriterias = new List<FilterCriteria>()
            {
                 new FilterCriteria("LinkedItemUID",StoreUID,FilterType.Equal),
            };
            paging.IsCountRequired = true;

            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
              $"{_appConfigs.ApiBaseUrl}AwayPeriod/GetAwayPeriodDetails",
              HttpMethod.Post, paging);
            if (apiResponse.Data != null)
            {
                var data = _commonFunctions.GetDataFromResponse(apiResponse.Data);
                PagedResponse<AwayPeriod.Model.Classes.AwayPeriod>? pagedResponse = JsonConvert.DeserializeObject<PagedResponse<AwayPeriod.Model.Classes.AwayPeriod>>(data);
                if (pagedResponse != null && pagedResponse.PagedData != null)
                {
                    AwayPeriodList = pagedResponse.PagedData.ToList<IAwayPeriod>();
                }

            }

        }
        #endregion

        #region Organisation Configuration
        public async Task<int> SaveOrUpdateOrgConfig()
        {
            _loadingService.ShowLoading();
            int retVal = 0;
            PrepareOrgConfigurationToSaveOrUpdate();
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}StoreOrgConfiguration/CreateStoreOrgConfiguration",
            HttpMethod.Post, OrgConfiguration);
            if (apiResponse.StatusCode == 200)
            {
                _StoreCredit.OrgLabel = _OrganisationConfiguration.SalesOrgLabel;
                _StoreCredit.PaymentTypeLabel = _OrganisationConfiguration.PaymentTypeLabel;
                _StoreCredit.PaymentTermLabel = _OrganisationConfiguration.PaymentTermLabel;
                StoreCreditList?.Add(_StoreCredit);
                _OrganisationConfiguration = CreateOrganisationConfigurationModelInstance();
                retVal = CommonFunctions.GetIntValue(_commonFunctions.GetDataFromResponse(apiResponse.Data));
                _toast.Add(@_localizer["success"], @_localizer["saved_successfully"], UIComponents.SnackBar.Enum.Severity.Success);
            }
            else
            {
                _toast.Add(@_localizer["error"], $"[{apiResponse.StatusCode}]{apiResponse.ErrorMessage}", UIComponents.SnackBar.Enum.Severity.Success);
            }
            _loadingService.HideLoading();

            return retVal;

        }
        public async Task GetOrgConfigurationByStoreUID()
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}StoreOrgConfiguration/SelectStoreOrgConfigurationByStoreUID?StoreUID={StoreUID}",
            HttpMethod.Get);
            if (apiResponse.IsSuccess && apiResponse.Data != null)
            {
                ApiResponse<OrgConfigurationUIModel>? pagedResponse = JsonConvert.DeserializeObject<ApiResponse<OrgConfigurationUIModel>>(apiResponse.Data);
                if (pagedResponse != null)
                {
                    try
                    {
                        //OrgConfiguration = pagedResponse.Data;
                        StoreCreditList = pagedResponse?.Data?.StoreCredit;
                        StoreAttributesList = pagedResponse?.Data?.StoreAttributes;
                        if (StoreCreditList != null && StoreCreditList.Count > 0)
                        {
                            //SalesOrgList
                            foreach (StoreCredit item in StoreCreditList)
                            {
                                if (string.IsNullOrEmpty(item.OrgUID))
                                {
                                    item.OrgLabel = "NA";
                                }
                                else
                                {
                                    foreach (ISelectionItem selectionItem in SalesOrgList)
                                    {
                                        if (selectionItem.UID == item.OrgUID)
                                        {
                                            item.OrgLabel = selectionItem.Label;
                                            //break;
                                        }
                                    }
                                }
                                foreach (ListItem listItems in ListItems)
                                {
                                    if (!string.IsNullOrEmpty(item.PaymentType))
                                    {
                                        if (listItems.Code == item.PaymentType)
                                        {
                                            item.PaymentTypeLabel = listItems.Name;
                                        }
                                    }
                                    else
                                    {
                                        item.PaymentTypeLabel = "NA";
                                    }
                                    if (!string.IsNullOrEmpty(item.PaymentTermUID))
                                    {
                                        if (listItems.Code == item.PaymentTermUID)
                                        {
                                            item.PaymentTermLabel = listItems.Name;
                                        }
                                    }
                                    else
                                    {
                                        item.PaymentTermLabel = "NA";
                                    }


                                    //if (string.IsNullOrEmpty(item.PaymentTermUID))
                                    //{
                                    //    item.PaymentTermLabel = "NA";
                                    //}
                                    //else
                                    //{
                                    //    item.PaymentTermLabel = listItems.Name;
                                    //}
                                }
                                //item.OrgLabel = string.IsNullOrEmpty(item.OrgUID) ? "NA" : SalesOrgList.Find(p => p.Code == item.OrgUID)?.Label;
                                //item.PaymentTypeLabel = string.IsNullOrEmpty(item.PaymentType) ? "NA" : ListItems.Find(p => p.Code == item.PaymentType)?.Name;
                                //item.PaymentTermLabel = string.IsNullOrEmpty(item.PaymentTermUID) ? "NA" : ListItems.Find(p => p.Code == item.PaymentTermUID)?.Name;
                            }
                        }
                        //SetEditMode();
                    }
                    catch (Exception ex)
                    {
                        await _alertService.ShowErrorAlert(@_localizer["error"], $"[{apiResponse.StatusCode}] {apiResponse.ErrorMessage}");
                    }
                }
            }
            else
            {
                await _alertService.ShowErrorAlert(@_localizer["error"], $"[{apiResponse.StatusCode}]{apiResponse.ErrorMessage}");
            }
        }

        #endregion

        #region Additional Info
        public async Task GetAdditionalInfo()
        {
            PagingRequest paging = new PagingRequest()
            {
                FilterCriterias = new List<FilterCriteria>()
                {
                    new FilterCriteria("StoreUID", StoreUID, FilterType.Equal)
                }
            };
            paging.IsCountRequired = true;

            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
             $"{_appConfigs.ApiBaseUrl}StoreAdditionalInfo/SelectAllStoreAdditionalInfo",
              HttpMethod.Post, paging);

            if (apiResponse != null && apiResponse.IsSuccess)
            {
                PagedResponse<StoreAdditionalInfo>? pagedResponse = JsonConvert.DeserializeObject<PagedResponse<StoreAdditionalInfo>>(_commonFunctions.GetDataFromResponse(apiResponse.Data));
                if (pagedResponse != null)
                {
                    try
                    {
                        if (pagedResponse.TotalCount > 0)
                        {
                            _StoreAdditionalInfo = pagedResponse?.PagedData?.FirstOrDefault<IStoreAdditionalInfo>();
                        }
                    }
                    catch (Exception ex)
                    {
                        await _alertService.ShowErrorAlert("Error", $"[{apiResponse.StatusCode}] {apiResponse.ErrorMessage}");
                    }
                }
            }
            else
            {
                _toast.Add(@_localizer["error"], $"[{apiResponse.StatusCode}] {apiResponse.ErrorMessage}", UIComponents.SnackBar.Enum.Severity.Success);
            }

        }
        public async Task SaveorUpdateStoreAdditionalinfo()
        {
            bool isnew = string.IsNullOrEmpty(_StoreAdditionalInfo?.UID);
            HttpMethod httpMethod = HttpMethod.Put;
            string endpointMethod = "UpdateStoreAdditionalInfo";
            if (isnew)
            {
                _StoreAdditionalInfo.StoreUID = StoreUID;
                _StoreAdditionalInfo.UID = Guid.NewGuid().ToString();
                _StoreAdditionalInfo.CreatedTime = DateTime.Now;
                _StoreAdditionalInfo.CreatedBy = _iAppUser.Emp.UID;
                endpointMethod = "CreateStoreAdditionalInfo";
                httpMethod = HttpMethod.Post;
            }
            _StoreAdditionalInfo.ModifiedTime = DateTime.Now;
            _StoreAdditionalInfo.ModifiedBy = _iAppUser.Emp.UID;


            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
              $"{_appConfigs.ApiBaseUrl}StoreAdditionalInfo/{endpointMethod}",
              httpMethod, _StoreAdditionalInfo);

            if (apiResponse.StatusCode == 200)
            {
                //isEdited = false;
                _toast.Add(@_localizer["success"], isnew ? @_localizer["saved"] : @_localizer["updated"] + @_localizer["successfully"], UIComponents.SnackBar.Enum.Severity.Success);
            }
            else
            {
                _toast.Add(@_localizer["error"], $"[{apiResponse.StatusCode}]  {apiResponse.ErrorMessage}", UIComponents.SnackBar.Enum.Severity.Success);
            }
        }
        #endregion

        #endregion
    }
}
