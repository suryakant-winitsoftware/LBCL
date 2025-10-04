using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.ServiceAndCallRegistration.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Modules.ServiceAndCallRegistration.Model.Interfaces;
using Winit.Modules.ServiceAndCallRegistration.Model.Classes;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using System.Runtime.InteropServices.Marshalling;
using Winit.Modules.SKU.Model.Constants;
using Winit.Shared.CommonUtilities.Common;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Shared.Models.Enums;
using Winit.Modules.ListHeader.Model.Interfaces;

namespace Winit.Modules.ServiceAndCallRegistration.BL.Classes
{
    public abstract class ServiceAndCallRegistrationBaseViewModel : IServiceAndCallRegistrationViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private Winit.Modules.Common.BL.Interfaces.IAppUser _appUser;
        private List<string> _propertiesToSearch = new List<string>();
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        public List<SortCriteria> SortCriterias { get; set; }
        public List<FilterCriteria> CallRegistrationFilterCriteria { get; set; }
        public List<ISelectionItem> CustTypeSelectionList { get; set; }
        public List<ISelectionItem> ProductCategorySelectionList { get; set; }
        public List<ISelectionItem> BrandCodeSelectionList { get; set; }
        public List<ISelectionItem> ServiceTypeCodeSelectionList { get; set; }
        public List<ISelectionItem> WarrentyStatusSelectionList { get; set; }
        public ICallRegistration CallRegistrationDetails { get; set; }
        public IServiceRequestStatus ServiceStatus { get; set; }
        public ICallRegistrationResponce CallRegistrationResponce { get; set; }
        public IServiceRequestStatusResponce serviceRequestStatusResponce { get; set; }
        public List<IFileSys>? FileSys { get; set; }
        public List<ICallRegistration> CallRegisteredDataList { get; set; }
        public ICallRegistration CallRegisteredItemDetails { get; set; }
        public ServiceAndCallRegistrationBaseViewModel(IServiceProvider serviceProvider,
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
            CustTypeSelectionList = new List<ISelectionItem>();
            ProductCategorySelectionList = new List<ISelectionItem>();
            BrandCodeSelectionList = new List<ISelectionItem>();
            ServiceTypeCodeSelectionList = new List<ISelectionItem>();
            WarrentyStatusSelectionList = new List<ISelectionItem>();
            CallRegistrationDetails = new CallRegistration();
            ServiceStatus = new ServiceRequestStatus();
            CallRegistrationResponce = new CallRegistrationResponce();
            serviceRequestStatusResponce = new ServiceRequestStatusResponce();
            FileSys = new List<IFileSys>();
            CallRegisteredDataList = new List<ICallRegistration>();
            CallRegisteredItemDetails = new CallRegistration();
            SortCriterias = new List<SortCriteria>();
            CallRegistrationFilterCriteria = new List<FilterCriteria>();
        }

        public async virtual Task PopulateDropDowns()
        {
            List<string> ServiceCallRegistrationDropDownsItems = new List<string>() {  SKUGroupTypeContants.ServiceRegistrationCustomerType , SKUGroupTypeContants.ProductCategoryName,
                                                    SKUGroupTypeContants.BrandCode , SKUGroupTypeContants.Servicetype,
                                                    SKUGroupTypeContants.WarrantyStatus };
            List<IListItem> listItems = await GetServiceAndCallRegistrationDropDownsItems(ServiceCallRegistrationDropDownsItems);

            CustTypeSelectionList.Clear();
            var CustTypeSelectionitems = listItems.Where(item => item.ListHeaderUID == SKUGroupTypeContants.ServiceRegistrationCustomerType);
            CustTypeSelectionList.AddRange(CommonFunctions.ConvertToSelectionItems(CustTypeSelectionitems.ToList(), e => e.UID, e => e.Code, e => e.Name));

            ProductCategorySelectionList.Clear();
            var ProductCategorySelectionItems = listItems.Where(item => item.ListHeaderUID == SKUGroupTypeContants.ProductCategoryName);
            ProductCategorySelectionList.AddRange(CommonFunctions.ConvertToSelectionItems(ProductCategorySelectionItems.ToList(), e => e.UID, e => e.Code, e => e.Name));

            BrandCodeSelectionList.Clear();
            var BrandCodeSelectionItems = listItems.Where(item => item.ListHeaderUID == SKUGroupTypeContants.BrandCode);
            BrandCodeSelectionList.AddRange(CommonFunctions.ConvertToSelectionItems(BrandCodeSelectionItems.ToList(), e => e.UID, e => e.Code, e => e.Name));

            ServiceTypeCodeSelectionList.Clear();
            var ServiceTypeCodeSelectionItems = listItems.Where(item => item.ListHeaderUID == SKUGroupTypeContants.Servicetype);
            ServiceTypeCodeSelectionList.AddRange(CommonFunctions.ConvertToSelectionItems(ServiceTypeCodeSelectionItems.ToList(), e => e.UID, e => e.Code, e => e.Name));

            WarrentyStatusSelectionList.Clear();
            var WarrentyStatusSelectionItems = listItems.Where(item => item.ListHeaderUID == SKUGroupTypeContants.WarrantyStatus);
            WarrentyStatusSelectionList.AddRange(CommonFunctions.ConvertToSelectionItems(WarrentyStatusSelectionItems.ToList(), e => e.UID, e => e.Code, e => e.Name));


        }

        public async Task PopulateCallRegistrations()
        {
            var data = await PopulateCallRegistrationsAsync();
            CallRegisteredDataList.Clear();
            if (data != null)
            {
                CallRegisteredDataList.AddRange(data);
            }
        }
        public async Task<ICallRegistrationResponce> SaveCallRegistrationDetails()
        {
            CallRegistrationDetails.OrgUID = _appUser.SelectedJobPosition.OrgUID;
            CallRegistrationResponce = await SaveCallRegistrationDetailsAPI(CallRegistrationDetails);
            if (CallRegistrationResponce != null && CallRegistrationResponce.CallID != null)
            {
                CallRegistrationDetails.ServiceCallNo = CallRegistrationResponce.CallID;
                if (await SaveCallRegistrationDetailsToDB(CallRegistrationDetails))
                {
                    return CallRegistrationResponce;
                }
                else
                {
                    CallRegistrationResponce.Errors.Add("Data Saving in Local DB failed");
                    return CallRegistrationResponce;
                }
            }
            else
            {
                return CallRegistrationResponce;
            }
        }



        public async Task<IServiceRequestStatusResponce> GetServiceStatusBasedOnNumber(IServiceRequestStatus serviceNumber)
        {
            return await GetServiceStatusBasedOnNumberAPI(serviceNumber);
        }
        public async Task PopulateCallRegistrationItemDetailsByUID(string serviceCallNumber)
        {
            CallRegisteredItemDetails = await PopulateCallRegistrationItemDetailsByUIDAsync(serviceCallNumber);
        }
        public async Task ApplyFilterForDealer(List<FilterCriteria> filterCriterias)
        {
            CallRegistrationFilterCriteria.Clear();
            CallRegistrationFilterCriteria.AddRange(filterCriterias);
            CallRegisteredDataList = await PopulateCallRegistrationsAsync();
        }
        public async Task ApplySort(SortCriteria sortCriteria)
        {
            SortCriterias.Clear();
            SortCriterias.Add(sortCriteria);
            CallRegisteredDataList = await PopulateCallRegistrationsAsync();
        }
        public async Task PageIndexChanged(int pageNumber)
        {
            PageNumber = pageNumber;
            CallRegisteredDataList = await PopulateCallRegistrationsAsync();
        }
        public abstract Task<List<Winit.Modules.SKU.Model.Interfaces.ISKUGroup>> GetProductCategorySelectionList(string skuGroupTypeUid);
        public abstract Task<List<Winit.Modules.ListHeader.Model.Interfaces.IListItem>> GetCustTypeFromListItem(string listHeaderCode);
        public abstract Task<ICallRegistrationResponce> SaveCallRegistrationDetailsAPI(ICallRegistration callRegistrationDetails);
        public abstract Task<IServiceRequestStatusResponce> GetServiceStatusBasedOnNumberAPI(IServiceRequestStatus serviceNumber);
        public abstract Task<List<ICallRegistration>> PopulateCallRegistrationsAsync();
        public abstract Task<ICallRegistration> PopulateCallRegistrationItemDetailsByUIDAsync(string serviceCallNumber);
        public abstract Task<bool> SaveCallRegistrationDetailsToDB(ICallRegistration callRegistrationDetails);
        public abstract Task<List<IListItem>> GetServiceAndCallRegistrationDropDownsItems(List<string> serviceCallRegistrationDropDownsItems);

    }
}
