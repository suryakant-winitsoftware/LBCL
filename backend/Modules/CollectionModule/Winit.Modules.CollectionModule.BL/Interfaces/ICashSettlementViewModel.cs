using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.CollectionModule.BL.Interfaces
{
    public interface ICashSettlementViewModel
    {
        public Dictionary<string, string> storeData { get; set; }
        public List<FilterCriteria> filterCriterias { get; set; }
        public PagedResponse<AccElement> pageResponse { get; set; }
        public AccCustomer[] CustomerCode { get; set; }
        public PagedResponse<AccElement> pagedResponse1 { get; set; }
        public PagedResponse<AccElement> pagedResponse2 { get; set; }
        Task GetCustomerCodeName();
        Task GetCashierDetails();
        public List<IAccCollection> CollectionTabDetails { get; set; }
        Task<Winit.Shared.Models.Common.ApiResponse<string>> Clicked(string Status);
        Task<Winit.Shared.Models.Common.ApiResponse<string>> SettleRecords(List<string> Multiple);
        Task OnFilterApply(Dictionary<string, string> keyValuePairs, string pageName);
        Task<Winit.Shared.Models.Common.ApiResponse<string>> ReceiptReverseByCash(string ReceiptNumber, string Amount, string ChequeNo, string ReasonforCancelation);
        Task<Winit.Shared.Models.Common.ApiResponse<string>> ReceiptVOIDByCash(string ReceiptNumber, string Amount, string ChequeNo, string ReasonforCancelation);
        public int _currentPage { get; set; }
        public int _pageSize { get; set; }
        public Task PageIndexChanged(int pageNumber);
    }
}
