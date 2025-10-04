using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Base.BL;
using Winit.Modules.Provisioning.BL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Provisioning.Model.Interfaces;
using Winit.Modules.Provisioning.Model.Classes;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
using Nest;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Modules.BroadClassification.Model.Interfaces;
using System.Globalization;
using System.util;

namespace Winit.Modules.Provisioning.BL.Classes
{
    public abstract class ProvisioningHeaderViewBaseViewModel : IProvisioningHeaderViewViewModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItemsCount { get; set; }
        public List<FilterCriteria> FilterCriterias { get; set; } = new List<FilterCriteria>();
        public List<ISelectionItem> ChannelPartnerList { get; set; }
        public IProvisionHeaderView ProvisionHeaderViewDetails { get; set; }
        public List<IProvisionApproval> SummaryViewDetails { get; set; }
        public List<IProvisionApproval> DetailViewDetails { get; set; }
        public List<IProvisionApproval> ProvisionRequestHistoryDetails { get; set; }
        //Filter Details
        public List<ISelectionItem> BranchDetails { get; set; }
        public List<ISelectionItem> SalesOfficeDetails { get; set; }
        public List<ISelectionItem> BroadClassificationDetails { get; set; }
        public List<ISelectionItem> ChannelPartnerDetails { get; set; }
        public List<ISelectionItem> ProvisionTypeDetails { get; set; }
        public string TabName { get; set; }
        public bool OnFilterApplied { get; set; }

        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private List<string> _propertiesToSearch = new List<string>();
        private Winit.Modules.Common.BL.Interfaces.IAppUser _appUser;

        public ProvisioningHeaderViewBaseViewModel(IServiceProvider serviceProvider,
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
            BranchDetails = new List<ISelectionItem>();
            SalesOfficeDetails = new List<ISelectionItem>();
            BroadClassificationDetails = new List<ISelectionItem>();
            ChannelPartnerDetails = new List<ISelectionItem>();
            ProvisionTypeDetails = new List<ISelectionItem>();
            ChannelPartnerList = new List<ISelectionItem>();
            ProvisionHeaderViewDetails = new ProvisionHeaderView();
            SummaryViewDetails = new List<IProvisionApproval>();
            DetailViewDetails = new List<IProvisionApproval>();
            ProvisionRequestHistoryDetails = new List<IProvisionApproval>();
        }
        public async virtual Task PopulateHeaderViewModel()
        {
            await GetChannelPartnersList();
        }
        public async virtual Task GetProvisioningHeaderViewData(string UID)
        {
            ProvisionHeaderViewDetails = await GetprovisionHeaderViewDetailsByUID(UID);
        }

        private async Task GetChannelPartnersList()
        {
            ChannelPartnerList.Clear();
            var newChannelpartners = await GetChannelPartnersListForProvisioning();
            if (newChannelpartners != null && newChannelpartners.Any())
            {
                ChannelPartnerList.AddRange(CommonFunctions.ConvertToSelectionItems(newChannelpartners, e => e.UID, e => e.Code, e => e.Name, e => $"[{e.Code}] {e.Name}"));
            }
        }
        public abstract Task<IProvisionHeaderView> GetprovisionHeaderViewDetailsByUID(string UID);
        public abstract Task<List<IStore>> GetChannelPartnersListForProvisioning();



        //Provision Approval
        public async Task DataSource(string Status, string View)
        {
            try
            {
                TabName = Status;
                await PopulateProvisionApproval(View);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task PopulateProvisionFilter()
        {
            try
            {
                BranchDetails.AddRange(CommonFunctions.ConvertToSelectionItems(await GetBranch(), new List<string> { "UID", "Code", "Name" }));
                SalesOfficeDetails.AddRange(CommonFunctions.ConvertToSelectionItems(await GetSalesOffice(), new List<string> { "UID", "Code", "Name" }));
                BroadClassificationDetails.AddRange(CommonFunctions.ConvertToSelectionItems(await GetBroadClassification(), new List<string> { "UID", "Code", "Name" }));
                ChannelPartnerDetails.AddRange(CommonFunctions.ConvertToSelectionItems(await GetChannelPartnerDetails(), new List<string> { "UID", "Code", "Name" }));
                ProvisionTypeDetails.AddRange(CommonFunctions.ConvertToSelectionItems(await GetListItemsByCodes(new List<string> { "PROVISION_TYPE" }), new List<string> { "UID", "Code", "Name" }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string ConvertIntoFormat(string value)
        {
            try
            {
                string dateValueString = value;
                DateTime dateValue;

                if (DateTime.TryParse(dateValueString, out dateValue))
                {
                    return dateValue.ToString("yyyy-MM-dd");
                    // Use the formattedDate as needed
                }
                return value;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task OnFilterApply(Dictionary<string, string> keyValuePairs, string View)
        {
            try
            {
                string ColumnName = string.Empty;
                string? startDate = null;
                string? endDate = null;
                FilterCriterias.Clear();
                foreach (var keyValue in keyValuePairs)
                {
                    if (!string.IsNullOrEmpty(keyValue.Value))
                    {
                        if (keyValue.Value.Contains(","))
                        {
                            string[] values = keyValue.Value.Split(',');
                            FilterCriterias.Add(new FilterCriteria(keyValue.Key, values, FilterType.In));
                        }
                        if (keyValue.Key != "PD.invoice_date" && keyValue.Key != "InvoiceToDate" && keyValue.Key != "InvoiceDate")
                        {
                            FilterCriterias.Add(new FilterCriteria(keyValue.Key, keyValue.Value, FilterType.Equal));
                        }
                        if (keyValue.Key == "PD.invoice_date" || keyValue.Key == "InvoiceDate")
                        {
                            ColumnName = keyValue.Key;
                            startDate = ConvertIntoFormat(keyValue.Value);
                        }
                        if (keyValue.Key == "InvoiceToDate")
                        {
                            endDate = ConvertIntoFormat(keyValue.Value);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
                {
                    string[] dateValues = { startDate, endDate };
                    FilterCriterias.Add(new FilterCriteria(ColumnName, dateValues, FilterType.Between));
                }
                OnFilterApplied = FilterCriterias.Count > 0 ?  true : false;
                await PopulateProvisionApproval(View);
            }
            catch (Exception ex)
            {

            }
        }
        public async Task PageIndexChanged(int pageNumber, string View)
        {
            PageNumber = pageNumber;
            await PopulateProvisionApproval(View);
        }
        public async Task PopulateProvisionApproval(string View)
        {
            try
            {
                if (View == ProvisionConstants.SummaryView)
                {
                    await GetProvisionSummaryViewDetails();
                }
                if (View == ProvisionConstants.DetailView)
                {
                    await GetProvisionDetailViewDetails();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task PopulateProvisionRequestHistory(List<string> ProvisionIds)
        {
            try
            {
                ProvisionRequestHistoryDetails.Clear();
                ProvisionRequestHistoryDetails.AddRange(await GetProvisionRequestHistoryDetailsByProvisionIds(ProvisionIds));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task GetProvisionSummaryViewDetails()
        {
            try
            {
                SummaryViewDetails.Clear();
                if (FilterCriterias.Count > 0 && ! OnFilterApplied)
                {
                    FilterCriterias.RemoveAll(e => e.Name == "PD.status" || e.Name == "PD.scheme_type");
                }
                if (!string.IsNullOrEmpty(TabName) && TabName != ProvisionConstants.Requested)
                {
                    FilterCriterias.Add(new FilterCriteria("PD.status", TabName, FilterType.Equal));
                    FilterCriterias.Add(new FilterCriteria("PD.scheme_type", ProvisionConstants.P1, FilterType.NotEqual));
                    FilterCriterias.Add(new FilterCriteria("PD.scheme_type", ProvisionConstants.CD, FilterType.NotEqual));
                }
                SummaryViewDetails.AddRange(TabName == ProvisionConstants.Requested ? await GetSummaryProvisionRequestHistoryDetails() ?? new() : await GetSummaryDetails() ?? new());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task GetProvisionDetailViewDetails()
        {
            try
            {
                DetailViewDetails.Clear();
                if (FilterCriterias.Count > 0)
                {
                    FilterCriterias.RemoveAll(e => e.Name == "Status" || e.Name == "ProvisionType");
                }
                if (!string.IsNullOrEmpty(TabName))
                {
                    if (TabName == ProvisionConstants.Pending || TabName == ProvisionConstants.Requested)
                    {
                        FilterCriterias.Add(new FilterCriteria("Status", TabName, FilterType.Equal));
                        FilterCriterias.Add(new FilterCriteria("ProvisionType", ProvisionConstants.P1, FilterType.NotEqual));
                        FilterCriterias.Add(new FilterCriteria("ProvisionType", ProvisionConstants.CD, FilterType.NotEqual));
                    }
                    else if (TabName == ProvisionConstants.PendingP1 || TabName == ProvisionConstants.PendingCD)
                    {
                        FilterCriterias.Add(new FilterCriteria("Status", ProvisionConstants.Pending, FilterType.Equal));
                        FilterCriterias.Add(new FilterCriteria("ProvisionType", TabName == ProvisionConstants.PendingP1 ? ProvisionConstants.P1 : ProvisionConstants.CD, FilterType.Equal));
                    }
                    else
                    {
                        FilterCriterias.Add(new FilterCriteria("Status", TabName, FilterType.Equal));
                    }
                }
                DetailViewDetails.AddRange(await GetDetailViewDetails() ?? new());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<bool> UpdateStatus(List<IProvisionApproval> provisionApprovals, string View)
        {
            try
            {
                return await UpdateProvisionStatus(provisionApprovals, View);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> InsertProvisionRequestHistoryDetails(List<IProvisionApproval> provisionApproval)
        {
            try
            {
                return await InsertProvisionRequestDetails(provisionApproval);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public abstract Task<List<IProvisionApproval>> GetSummaryDetails();
        public abstract Task<List<IProvisionApproval>> GetSummaryProvisionRequestHistoryDetails();
        public abstract Task<List<IProvisionApproval>> GetProvisionRequestHistoryDetailsByProvisionIds(List<string> ProvisionIds);
        public abstract Task<List<IProvisionApproval>> GetDetailViewDetails();

        //Filter Details
        protected abstract Task<List<IBranch>> GetBranch();
        protected abstract Task<bool> InsertProvisionRequestDetails(List<IProvisionApproval> provisionApproval);
        protected abstract Task<List<ISalesOffice>> GetSalesOffice();
        protected abstract Task<List<IBroadClassificationHeader>> GetBroadClassification();
        protected abstract Task<List<IStore>> GetChannelPartnerDetails();
        protected abstract Task<List<IListItem>?> GetListItemsByCodes(List<string> codes);
        protected abstract Task<bool> UpdateProvisionStatus(List<IProvisionApproval> provisionApprovals, string View);
    }
}
