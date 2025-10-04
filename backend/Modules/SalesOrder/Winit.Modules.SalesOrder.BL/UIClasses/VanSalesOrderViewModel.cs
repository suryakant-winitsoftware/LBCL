using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.SalesOrder.BL.UIInterfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SalesOrder.BL.UIClasses;

public class VanSalesOrderViewModel : SalesOrderWebViewModel
{
    public VanSalesOrderViewModel(IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            UIInterfaces.ISalesOrderAmountCalculator amountCalculator,
            IListHelper listHelper,
            IAppUser appUser,
            IAppSetting appSetting,
            IDataManager dataManager,
            IOrderLevelCalculator orderLevelCalculator,
            ICashDiscountCalculator cashDiscountCalculator, Winit.Shared.Models.Common.IAppConfig appConfigs,
            ISalesOrderDataHelper salesOrderDataHelper)
        : base(serviceProvider, filter, sorter, amountCalculator, listHelper, appUser, appSetting,
               dataManager, orderLevelCalculator, cashDiscountCalculator, appConfigs, salesOrderDataHelper)
    {
        _appConfigs = appConfigs;
        RouteSelectionItems = [];
        StoreSelectionItems = [];
        StoreItemViews = [];
        OrderType = Shared.Models.Constants.OrderType.Vansales;
    }
    public override async Task<bool> SaveSalesOrder(string statusType = SalesOrderStatus.DRAFT)
    {
        if (statusType == SalesOrderStatus.DRAFT)
        {
            return await base.SaveSalesOrder(statusType);
        }
        else
        {
            var storeHistory = await _salesOrderDataHelper.GetStoreHistoryAPI(RouteUID, CommonFunctions.GetDateOnlyInFormatForSqlite(ExpectedDeliveryDate), StoreUID);
            if (storeHistory != null)
            {
                BeatHistoryUID = storeHistory.BeatHistoryUID;
                StoreHistoryUID = storeHistory.UID;
                return await base.SaveSalesOrder(statusType);
            }
            else
            {
                throw new CustomException(ExceptionStatus.Failed, "Store History not found");
            }
        }
    }

}
