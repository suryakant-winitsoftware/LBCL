using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.SalesOrder.BL.UIInterfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;

namespace Winit.Modules.SalesOrder.BL.UIClasses;

public class SalesOrderViewModelFactory : ISalesOrderViewModelFactory
{
    private readonly IServiceProvider serviceProvider;
    private readonly IFilterHelper filter;
    private readonly ISortHelper sorter;
    private readonly UIInterfaces.ISalesOrderAmountCalculator amountCalculator;
    private readonly IListHelper listHelper;
    private readonly IAppUser appUser;
    private readonly IAppSetting appSetting;
    private readonly ISKUBL sKUBL;
    private readonly IDataManager dataManager;
    private readonly ISKUPriceBL sKUPriceBL;
    private readonly Winit.Modules.SalesOrder.BL.Interfaces.ISalesOrderBL salesOrderBL;
    private readonly IOrderLevelCalculator orderLevelCalculator;
    private readonly ICashDiscountCalculator cashDiscountCalculator;
    private readonly Winit.Shared.Models.Common.IAppConfig appConfigs;
    private readonly Winit.Modules.Base.BL.ApiService apiService;
    private readonly Winit.Modules.Promotion.BL.Interfaces.IPromotionBL promotionBL;
    private readonly Org.BL.Interfaces.IOrgBL orgBL;
    private readonly SalesOrder.BL.UIInterfaces.ISalesOrderDataHelper salesOrderDataHelper;

    public SalesOrderViewModelFactory(
        IServiceProvider serviceProvider,
        IFilterHelper filter,
        ISortHelper sorter,
        UIInterfaces.ISalesOrderAmountCalculator amountCalculator,
        IListHelper listHelper,
        IAppUser appUser,
        IAppSetting appSetting,
        ISKUBL sKUBL,
        IDataManager dataManager,
        ISKUPriceBL sKUPriceBL,
        Winit.Modules.SalesOrder.BL.Interfaces.ISalesOrderBL salesOrderBL,
        IOrderLevelCalculator orderLevelCalculator,
        ICashDiscountCalculator cashDiscountCalculator,
        Winit.Modules.Promotion.BL.Interfaces.IPromotionBL promotionBL,
        Org.BL.Interfaces.IOrgBL orgBL, ISalesOrderDataHelper salesOrderDataHelper)
    {
        // Initialize your dependencies here
        this.serviceProvider = serviceProvider;
        this.filter = filter;
        this.sorter = sorter;
        this.amountCalculator = amountCalculator;
        this.listHelper = listHelper;
        this.appUser = appUser;
        this.appSetting = appSetting;
        this.sKUBL = sKUBL;
        this.dataManager = dataManager;
        this.sKUPriceBL = sKUPriceBL;
        this.salesOrderBL = salesOrderBL;
        this.orderLevelCalculator = orderLevelCalculator;
        this.cashDiscountCalculator = cashDiscountCalculator;
        this.promotionBL = promotionBL;
        this.orgBL = orgBL;
        this.salesOrderDataHelper = salesOrderDataHelper;
    }
    public SalesOrderViewModelFactory(
        IServiceProvider serviceProvider,
        IFilterHelper filter,
        ISortHelper sorter,
        UIInterfaces.ISalesOrderAmountCalculator amountCalculator,
        IListHelper listHelper,
        IAppUser appUser,
        IAppSetting appSetting,
        IDataManager dataManager,
        IOrderLevelCalculator orderLevelCalculator,
        ICashDiscountCalculator cashDiscountCalculator,
        Winit.Shared.Models.Common.IAppConfig appConfigs,
        Winit.Modules.Base.BL.ApiService apiService, ISalesOrderDataHelper salesOrderDataHelper)
    {
        // Initialize your dependencies here
        this.serviceProvider = serviceProvider;
        this.filter = filter;
        this.sorter = sorter;
        this.amountCalculator = amountCalculator;
        this.listHelper = listHelper;
        this.appUser = appUser;
        this.appSetting = appSetting;
        this.dataManager = dataManager;
        this.orderLevelCalculator = orderLevelCalculator;
        this.cashDiscountCalculator = cashDiscountCalculator;
        this.apiService = apiService;
        this.appConfigs = appConfigs;
        this.salesOrderDataHelper = salesOrderDataHelper;
    }

    public ISalesOrderViewModel CreateSalesOrderViewModel(string SalesType)
    {
        switch (SalesType)
        {
            case Winit.Shared.Models.Constants.OrderType.Presales:
                return new PreSalesOrderViewModel(
                   serviceProvider, filter, sorter, amountCalculator, listHelper,
                   appUser, appSetting, dataManager,
                   orderLevelCalculator, cashDiscountCalculator, appConfigs, salesOrderDataHelper);
            case Winit.Shared.Models.Constants.OrderType.Vansales:
                return new VanSalesOrderViewModel(
                serviceProvider, filter, sorter, amountCalculator, listHelper,
                appUser, appSetting, dataManager,
                orderLevelCalculator, cashDiscountCalculator, appConfigs, salesOrderDataHelper);
            case Winit.Shared.Models.Constants.OrderType.Cashsales:
                return new CashSalesOrderViewModel(
                serviceProvider, filter, sorter, amountCalculator, listHelper,
                appUser, appSetting, dataManager,
                orderLevelCalculator, cashDiscountCalculator, appConfigs, salesOrderDataHelper);
            default:
                return new VanSalesOrderViewModel(
                serviceProvider, filter, sorter, amountCalculator, listHelper,
                appUser, appSetting, dataManager,
                orderLevelCalculator, cashDiscountCalculator, appConfigs, salesOrderDataHelper);
        }
    }
}
