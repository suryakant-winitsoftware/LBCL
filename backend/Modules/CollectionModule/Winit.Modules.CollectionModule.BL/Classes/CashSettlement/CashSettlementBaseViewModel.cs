using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.CollectionModule.BL.Classes.CashSettlement
{
    public abstract class CashSettlementBaseViewModel : ICashSettlementViewModel
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IAppConfig _appConfig;
        protected readonly IAppUser _appUser;
        public AccCustomer[] CustomerCode { get; set; }
        public Dictionary<string, string> storeData { get; set; }
        public List<FilterCriteria> filterCriterias { get; set; }
        public PagedResponse<AccElement> pageResponse { get; set; }
        public PagedResponse<AccElement> pagedResponse1 { get; set; }
        public PagedResponse<AccElement> pagedResponse2 { get; set; }
        public int _currentPage { get; set; }
        public int _pageSize { get; set; }
        public List<IAccCollection> CollectionTabDetails { get; set; } = new List<IAccCollection>();

        public CashSettlementBaseViewModel(IServiceProvider serviceProvider, IAppConfig appConfig, IAppUser appUser)
        {
            _serviceProvider = serviceProvider;
            _appUser = appUser;
            _appConfig = appConfig;
            CustomerCode = new AccCustomer[1];
            storeData = new Dictionary<string, string>();
            filterCriterias = new List<FilterCriteria>();
            pageResponse = new PagedResponse<AccElement>();
            pagedResponse1 = new PagedResponse<AccElement>();
            pagedResponse2 = new PagedResponse<AccElement>();
            _currentPage = 1;
            _pageSize = 10;
        }

        public async Task PopulateUI()
        {
            try
            {
                await GetCustomerCodeName();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task PageIndexChanged(int pageNumber)
        {
            _currentPage = pageNumber;
            await GetCashierDetails();
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
                        ColumnName = "CollectedDate";
                        break;
                    case "ViewPayments":
                        ColumnName = "CollectedDate";
                        break;
                }
                filterCriterias.Add(new FilterCriteria(ColumnName, dateValues, FilterType.Between));
            }
            await GetCashierDetails();
        }

        public abstract Task GetCustomerCodeName();
        public abstract Task GetCashierDetails();
        public abstract Task<Winit.Shared.Models.Common.ApiResponse<string>> Clicked(string Status);
        public abstract Task<Winit.Shared.Models.Common.ApiResponse<string>> SettleRecords(List<string> Multiple);
        public abstract Task<Winit.Shared.Models.Common.ApiResponse<string>> ReceiptReverseByCash(string ReceiptNumber, string Amount, string ChequeNo, string ReasonforCancelation);
        public abstract Task<Winit.Shared.Models.Common.ApiResponse<string>> ReceiptVOIDByCash(string ReceiptNumber, string Amount, string ChequeNo, string ReasonforCancelation);

    }
}
