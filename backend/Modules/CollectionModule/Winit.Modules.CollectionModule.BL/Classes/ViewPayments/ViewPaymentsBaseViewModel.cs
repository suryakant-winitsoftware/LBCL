using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.CollectionModule.BL.Classes.Cashier
{
    public abstract class ViewPaymentsBaseViewModel : IViewPaymentsViewModel
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IAppConfig _appConfig;
        public List<Winit.Modules.Store.Model.Classes.Store> CustomerCode { get; set; }
        public List<AccCollectionAllotment> _viewDetailsList { get; set; } = new List<AccCollectionAllotment>();
        public List<IAccCollection> CollectionTabDetails { get; set; } = new List<IAccCollection>();
        public Dictionary<string, string> storeData { get; set; }
        public List<FilterCriteria> FilterCriterias { get; set; } 
        public List<SortCriteria> SortCriterias { get; set; } 
        public List<IAccCollection> Payments { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; } = 0;


        protected ViewPaymentsBaseViewModel(IServiceProvider serviceProvider, IAppConfig appConfig)
        {
            _serviceProvider = serviceProvider;
            _appConfig = appConfig;
            storeData = new Dictionary<string, string>();
            FilterCriterias = new List<FilterCriteria>();
            SortCriterias = new List<SortCriteria>();
            Payments = new List<IAccCollection>();
            CustomerCode = new List<Winit.Modules.Store.Model.Classes.Store>();
        }

        public async Task PopulateViewModel()
        {
            try
            {
                var data = await GetReceiptDetails_Data();
                await CollectionTabs();
                Payments.Clear();
                if (data != null)
                {
                    Payments.AddRange(data);
                }
            }
            catch(Exception ex)
            {

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
                throw ;
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
                        FilterCriterias.Add(new FilterCriteria(keyValue.Key, values, FilterType.In));
                    }
                    else
                    {
                        if (keyValue.Key == "FromDate" || keyValue.Key == "ToDate")
                        {
                            switch (keyValue.Key)
                            {
                                case "FromDate":
                                    fromDateValue = ConvertIntoFormat(keyValue.Value);
                                    break;
                                case "ToDate":
                                    toDateValue = ConvertIntoFormat(keyValue.Value);
                                    break;
                            }
                        }
                        else
                        {
                            FilterCriterias.Add(new FilterCriteria(keyValue.Key, keyValue.Value, FilterType.Like));
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
                FilterCriterias.Add(new FilterCriteria(ColumnName, dateValues, FilterType.Between));
            }
            await PopulateViewModel();
        }
        public async Task OnSortApply(SortCriteria sortCriteria)
        {
            try
            {
                SortCriterias.Clear();
                SortCriterias.Add(sortCriteria);
                await PopulateViewModel();
            }
            catch(Exception ex)
            {

            }
        }

        #region abstract methods
        public abstract Task<List<IAccCollection>> GetReceiptDetails_Data();
        public abstract Task ViewReceiptDetails(string UID);
        public abstract Task GetCustomerCodeName();
        public abstract Task CollectionTabs();
        #endregion
    }
}
