using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Bank.Model.Interfaces;
using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.CollectionModule.BL.Classes.NonCashSettlement
{
    public abstract class NonCashSettlementBaseViewModel : INonCashSettlementViewModel
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IAppConfig _appConfig;
        protected readonly IAppUser _appUser;
        public int _currentPage { get; set; }
        public int _pageSize { get; set; }
        public AccCustomer[] CustomerCode { get; set; } = new AccCustomer[1];
        public IBank[] BankNames { get; set; } = new IBank[1];
        public Dictionary<string, string> storeData { get; set; }
        public List<FilterCriteria> filterCriterias { get; set; }
        public Winit.Shared.Models.Common.ApiResponse<PagedResponse<AccCollectionPaymentMode>> Pendingresponse { get; set; }
        public Winit.Shared.Models.Common.ApiResponse<PagedResponse<AccCollectionPaymentMode>> Settledresponse { get; set; }
        public Winit.Shared.Models.Common.ApiResponse<PagedResponse<AccCollectionPaymentMode>> Approvedresponse { get; set; }
        public Winit.Shared.Models.Common.ApiResponse<PagedResponse<AccCollectionPaymentMode>> Bouncedresponse { get; set; }
        public Winit.Shared.Models.Common.ApiResponse<PagedResponse<AccCollectionPaymentMode>> Rejectedresponse { get; set; }
        public List<AccCollectionPaymentMode> _elemPending { get; set; } 
        public List<AccCollectionPaymentMode> _elemSettled { get; set; } 
        public List<AccCollectionPaymentMode> _elemApproved { get; set; } 
        public List<AccCollectionPaymentMode> _elemRejected { get; set; } 
        public List<AccCollectionPaymentMode> _elemBounced { get; set; } 
        public AccCollectionPaymentMode[] elem { get; set; } 
        public AccCollectionPaymentMode[] elemen { get; set; } 
        public AccCollectionPaymentMode[] elemt { get; set; } 
        public AccCollectionPaymentMode[] elemtRej { get; set; } 
        public AccCollectionPaymentMode[] elemtBounc { get; set; }
        public List<IAccCollection> CollectionTabDetails { get; set; } = new List<IAccCollection>();
        public int pendingCount { get; set; } 
        public int settlementCount { get; set; } 
        public int rejectedCount { get; set; } 
        public int bouncedCount { get; set; } 
        public int approvedCount { get; set; } 

        public NonCashSettlementBaseViewModel(IServiceProvider serviceProvider, IAppConfig appConfig,IAppUser appUser)
        {
            _serviceProvider = serviceProvider;
            _appConfig = appConfig;
            _appUser = appUser;
            storeData = new Dictionary<string, string>();
            CustomerCode = new AccCustomer[1];
            filterCriterias = new List<FilterCriteria>();
            elem = new AccCollectionPaymentMode[0];
            elemen = new AccCollectionPaymentMode[0];
            elemt = new AccCollectionPaymentMode[0];
            elemtRej = new AccCollectionPaymentMode[0];
            elemtBounc = new AccCollectionPaymentMode[0];
            _elemPending = new List<AccCollectionPaymentMode>();
            _elemSettled  = new List<AccCollectionPaymentMode>();
            _elemApproved = new List<AccCollectionPaymentMode>();
            _elemRejected = new List<AccCollectionPaymentMode>();
            _elemBounced  = new List<AccCollectionPaymentMode>();
            _currentPage = 1;
            _pageSize = 10;
            pendingCount = 0;
            settlementCount = 0;
            rejectedCount = 0;
            bouncedCount = 0;
            approvedCount = 0;
        }

        public async Task PopulateUI()
        {
            try
            {
                await GetCustomerCodeName();
            }
            catch(Exception ex)
            {

            }
        }

        public async Task OnFilterApply(Dictionary<string, string> keyValuePairs, string pageName)
        {
            string fromDateValue = "";
            string toDateValue = "";

            foreach (var keyValue in keyValuePairs)
            {
                if (!string.IsNullOrEmpty(keyValue.Value))
                {
                    if (keyValue.Value.Contains(","))
                    {
                        string[] values = keyValue.Value.Split(',');
                        filterCriterias.Add(new FilterCriteria(keyValue.Key, values, FilterType.In));
                    }
                    else
                    {
                        if (keyValue.Key == "FromDate" || keyValue.Key == "ToDate")
                        {
                            switch (keyValue.Key)
                            {
                                case "FromDate":
                                    fromDateValue = keyValue.Value;
                                    break;
                                case "ToDate":
                                    toDateValue = keyValue.Value;
                                    break;
                            }
                        }
                        else
                        {
                            filterCriterias.Add(new FilterCriteria(keyValue.Key, keyValue.Value, FilterType.Like));
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(fromDateValue) && !string.IsNullOrEmpty(toDateValue))
            {
                // Create FilterCriteria for Between
                string[] dateValues = { fromDateValue, toDateValue };
                string ColumnName = "";
                switch (pageName)
                {
                    case "NonCashSettlement":
                        ColumnName = "ChequeDate";
                        break;
                    case "CashSettlement":
                        ColumnName = "";
                        break;
                    case "ViewPayments":
                        ColumnName = "CollectedDate";
                        break;
                }
                filterCriterias.Add(new FilterCriteria(ColumnName, dateValues, FilterType.Between));
            }
            //await ShowAllTabssRecords(0);
        }
        public async Task PageIndexChanged(int pageNumber)
        {
            _currentPage = pageNumber;
            await ShowAllTabssRecords();
        }

        public abstract Task GetCustomerCodeName();
        public abstract Task ShowAllTabssRecords(int Count = 0);
    }
}
