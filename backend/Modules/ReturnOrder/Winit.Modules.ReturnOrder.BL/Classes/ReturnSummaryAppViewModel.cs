using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.ReturnOrder.BL.Interfaces;

namespace Winit.Modules.ReturnOrder.BL.Classes
{
    public class ReturnSummaryAppViewModel : ReturnSummaryBaseViewModel
    {
        private readonly IReturnOrderBL _returnOrderBL;
        public ReturnSummaryAppViewModel
        (IReturnOrderBL returnOrderBL,IAppUser appUser):base(appUser)
        {
            _returnOrderBL = returnOrderBL;
        }
        #region Concrete Methods
        protected override async Task<bool> UpdateReturnOrderStatus_Data(List<string> returnOrderUIDs, string status)
        {
            return await UpdateReturnOrderStatusByUIDs(returnOrderUIDs, status);
        }
        protected override async Task<List<Model.Interfaces.IReturnSummaryItemView>> GetReturnOrderSummaryItemViews_Data
            (DateTime startDate, DateTime endDate, string? storeUID = null)
        {
            return await GetReturnSummaryItemViews(startDate, endDate, storeUID);
        }
        #endregion

        #region BL calling Methods
        private async Task<bool> UpdateReturnOrderStatusByUIDs(List<string> returnOrderUIDs, string status)
        {
            return await _returnOrderBL.UpdateReturnOrderStatus(returnOrderUIDs, status) > 0;
        }
        private async Task<List<Model.Interfaces.IReturnSummaryItemView>> GetReturnSummaryItemViews(DateTime startDate,
            DateTime endDate, string? storeUID = null)
        {
            return await _returnOrderBL.GetReturnSummaryItemView(startDate, endDate, storeUID);
        }

        public override Task ApplyFilter()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
