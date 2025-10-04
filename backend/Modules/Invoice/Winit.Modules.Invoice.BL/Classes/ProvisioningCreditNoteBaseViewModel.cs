using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Invoice.BL.Interfaces;
using Winit.Modules.Invoice.Model.Classes;
using Winit.Modules.Invoice.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common;
using Winit.UIModels.Common;
using WINITMobile.Data;

namespace Winit.Modules.Invoice.BL.Classes
{
    public class ProvisioningCreditNoteBaseViewModel : IProvisioningCreditNoteViewModel
    {
        //Dependecy injection
        private readonly IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private readonly IAppUser _appUser;
        private readonly IAppSetting _appSetting;
        private readonly IDataManager _dataManager;
        private readonly IAppConfig _appConfigs;
        private readonly ApiService _apiService;
        private readonly IAlertService _alertService;
        public List<FilterCriteria> CreditNoteFilterCriterias { get; set; }
        public List<SortCriteria> SortCriterias { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public List<IProvisioningCreditNoteView> DisplayCreditNoteList { set; get; } = new List<IProvisioningCreditNoteView>();

        public ProvisioningCreditNoteBaseViewModel(IServiceProvider serviceProvider,
                IFilterHelper filter,
                ISortHelper sorter,
                IListHelper listHelper,
                IAppUser appUser,
                IAppSetting appSetting,
                IDataManager dataManager,
                IAppConfig appConfigs, ApiService apiService, IAlertService alertService) 
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _appUser = appUser;
            _dataManager = dataManager;
            _appConfigs = appConfigs;
            _apiService = apiService;
            _alertService = alertService;
            DisplayCreditNoteList = new();
            CreditNoteFilterCriterias = new List<FilterCriteria>();

        }
        #region UI Logic
        public async Task LoadDataAsync(bool status = false)
        {
            // Fetch data and apply the filter based on status
            DisplayCreditNoteList = await FetchCreditNoteDataAsync(status);
        }
       public async  Task ApproveSelectedAsync()
       {
            if (await UpdateInvoiceApproveStatus())
            {
                await LoadDataAsync();
                await _alertService.ShowErrorAlert("Success", "Selected items approved successfully!");
            }
            else
            {
                await _alertService.ShowErrorAlert("failed", "Failed to approve");
            }
       }

        public async Task OnFilterApply(List<UIModels.Common.Filter.FilterModel> ColumnsForFilter, Dictionary<string, string> filterCriteria, string selectedTab)
        {
            List<FilterCriteria> filterCriterias = new List<FilterCriteria>();
            DateTime? startDate = null;
            DateTime? endDate = null;

            // Loop through the filter criteria dictionary
            foreach (var keyValue in filterCriteria)
            {
                if (!string.IsNullOrEmpty(keyValue.Value))
                {
                    // Handle filtering based on the OrgUID (Channel Partner)
                    if (keyValue.Key == "OrgUID")
                    {
                        // Implement your logic here if needed
                    }
                    // Handle filtering based on the invoice_number (Invoice No)
                    else if (keyValue.Key == "invoice_number")
                    {
                        filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Equal));
                    }
                    // Handle filtering based on the Start Date and End Date
                    else if (keyValue.Key == "Start Date")
                    {
                        startDate = DateTime.Parse(keyValue.Value);
                    }
                    else if (keyValue.Key == "End Date")
                    {
                        endDate = DateTime.Parse(keyValue.Value);
                    }
                }
            }

            // If both dates are present, apply BETWEEN filter
            if (startDate.HasValue && endDate.HasValue)
            {
                filterCriterias.Add(new FilterCriteria("invoice_date", new { Start = startDate.Value, End = endDate.Value }, FilterType.Between));
            }
            else if (startDate.HasValue)
            {
                filterCriterias.Add(new FilterCriteria("invoice_date", startDate.Value, FilterType.GreaterThanOrEqual));
            }
            else if (endDate.HasValue)
            {
                filterCriterias.Add(new FilterCriteria("invoice_date", endDate.Value, FilterType.LessThanOrEqual));
            }

            // Apply the filter criteria and load the data based on selected tab
            await ApplyFilter(filterCriterias, selectedTab);
        }

        public async Task ApplyFilter(List<Shared.Models.Enums.FilterCriteria> filterCriterias, string selectectab)
        {
            CreditNoteFilterCriterias.Clear();
            CreditNoteFilterCriterias.AddRange(filterCriterias);
            await LoadDataAsync();
        }
        #endregion

        #region ApiCallling 
        public async Task<List<IProvisioningCreditNoteView>> FetchCreditNoteDataAsync(bool status)
        {
            DateTime endDate = DateTime.Now.AddDays(4);
            DateTime startDate = DateTime.Now.AddDays(-120);
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.PageSize = PageSize;
            pagingRequest.PageNumber = PageNumber;
            pagingRequest.FilterCriterias = CreditNoteFilterCriterias;
            pagingRequest.SortCriterias = new List<SortCriteria>
             {
                 new SortCriteria("InvoiceProvisioning",SortDirection.Desc)
             };
            pagingRequest.SortCriterias = this.SortCriterias;
            pagingRequest.IsCountRequired = true;

            Winit.Shared.Models.Common.ApiResponse<PagedResponse<IProvisioningCreditNoteView>> apiResponse = await _apiService.FetchDataAsync<PagedResponse<IProvisioningCreditNoteView>>(
                    $"{_appConfigs.ApiBaseUrl}Invoice/GetInvoiceApproveSatsusDetails?Status={status}",
                    HttpMethod.Post, pagingRequest);


            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                DisplayCreditNoteList = apiResponse!.Data!.PagedData!.ToList();
                return DisplayCreditNoteList;
            }
            return [];
        }


        public async Task<bool> UpdateInvoiceApproveStatus()
        {

            List<IProvisioningCreditNoteView> selectedItems = DisplayCreditNoteList.OfType<IProvisioningCreditNoteView>()
                                          .Where(item => item.IsSelected)
                                          .ToList();
            

            if (selectedItems.Count > 0)
            {
                selectedItems.ForEach(e => {
                    e.Status = true;
                    e.ApprovedByEmpUID = _appUser.Emp.UID;
                    e.ApprovedOn = DateTime.Now;
                    });
                try
                {
                    ApiResponse<string> apiResponse = await _apiService.FetchDataAsync<string>(
                    $"{_appConfigs.ApiBaseUrl}Invoice/UpdateInvoiceProvisioning",
                    HttpMethod.Put, selectedItems);
                    if (apiResponse.IsSuccess)
                    {
                        foreach (var item in selectedItems)
                        {
                            item.Status = true;
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                       
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            else
            {
                // Show message if no items are selected
                await _alertService.ShowErrorAlert("Alert", "Please select items to approve");
            }
            return false;
        }

        # endregion
    }
}
