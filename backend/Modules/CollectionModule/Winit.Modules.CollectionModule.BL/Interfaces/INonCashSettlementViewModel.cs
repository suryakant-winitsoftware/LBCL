using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Bank.Model.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.CollectionModule.BL.Interfaces
{
    public interface INonCashSettlementViewModel
    {
        public Dictionary<string, string> storeData { get; set; }
        public List<FilterCriteria> filterCriterias { get; set; }
        public AccCustomer[] CustomerCode { get; set; }
        public IBank[] BankNames { get; set; }
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
        public List<IAccCollection> CollectionTabDetails { get; set; }
        public int settlementCount { get; set; }

        Task PopulateUI();
        Task GetCustomerCodeName();
        Task ShowAllTabssRecords(int Count);
        Task OnFilterApply(Dictionary<string, string> keyValuePairs, string pageName);
        public int _currentPage { get; set; }
        public int _pageSize { get; set; }
        public Task PageIndexChanged(int pageNumber);
        public int pendingCount { get; set; }
        public int rejectedCount { get; set; }
        public int bouncedCount { get; set; }
        public int approvedCount { get; set; }
    }
}
