using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.SalesOrder.BL.UIInterfaces;
using Winit.Modules.Setting.BL.Interfaces;

namespace Winit.Modules.SalesOrder.BL.UIClasses;

public class PreSalesOrderViewModel : SalesOrderWebViewModel
{
    public PreSalesOrderViewModel(IServiceProvider serviceProvider,
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
        OrderType = Shared.Models.Constants.OrderType.Presales;
    }

    public override async Task<bool> UpdateSalesOrderStatus(string status)
    {
        Winit.Modules.SalesOrder.Model.Classes.SalesOrderStatusModel salesOrderStatusModel = new()
        {
            ModifiedBy = _appUser?.Emp?.UID,
            UID = SalesOrderUID,
            ModifiedTime = DateTime.Now,
            Status = status,
        };
        return await _salesOrderDataHelper.UpdateSalesOrderStatusApiAsync(salesOrderStatusModel);
    }

}
