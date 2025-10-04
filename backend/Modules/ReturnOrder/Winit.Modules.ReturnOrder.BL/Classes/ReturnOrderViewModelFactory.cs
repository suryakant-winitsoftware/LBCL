using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Classes;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.FileSys.BL.Classes;
using Winit.Modules.FileSys.BL.Interfaces;
using Winit.Modules.ListHeader.BL.Classes;
using Winit.Modules.ListHeader.BL.Interfaces;
using Winit.Modules.ReturnOrder.BL.Interfaces;
using Winit.Modules.ReturnOrder.Model.Constants;
using Winit.Modules.SalesOrder.BL.Interfaces;
using Winit.Modules.SalesOrder.BL.UIClasses;
using Winit.Modules.SalesOrder.BL.UIInterfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Shared.Models.Constants;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Mobile;

namespace Winit.Modules.ReturnOrder.BL.Classes;

public class ReturnOrderViewModelFactory : IReturnOrderViewModelFactory
{
    private readonly IServiceProvider serviceProvider;
    private readonly IFilterHelper filter;
    private readonly ISortHelper sorter;
    private readonly IListHelper listHelper;
    private readonly IAppUser appUser;
    private readonly ISKUBL sKUBL;
    private readonly ISKUPriceBL sKUPriceBL;
    private readonly Winit.Modules.SalesOrder.BL.Interfaces.ISalesOrderBL salesOrderBL;
    private readonly Winit.Modules.ReturnOrder.BL.Interfaces.IReturnOrderBL returnOrderBL;
    private readonly Interfaces.IReturnOrderAmountCalculator amountCalculator;
    private readonly IListHeaderBL listHeaderBL;
    private readonly IFileSysBL fileSysBL;
    private readonly Winit.Shared.Models.Common.IAppConfig appConfigs;
    private readonly Winit.Modules.Base.BL.ApiService apiService;
    private readonly IDataManager _dataManager;
    private readonly IStringLocalizer<LanguageKeys> localizer;


    public ReturnOrderViewModelFactory(
        IServiceProvider serviceProvider,
        IFilterHelper filter,
        ISortHelper sorter,
        Interfaces.IReturnOrderAmountCalculator amountCalculator,
        IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs,
        Winit.Modules.Base.BL.ApiService apiService,
        IAppUser appUser, ISKUBL sKUBL, IListHeaderBL listHeaderBL, ISKUPriceBL sKUPriceBL,
        IReturnOrderBL returnOrderBL, IFileSysBL fileSysBL, ISalesOrderBL salesOrderBL, IDataManager dataManager)
    {
        this.serviceProvider = serviceProvider;
        this.filter = filter;
        this.sorter = sorter;
        this.listHelper = listHelper;
        this.appUser = appUser;
        this.sKUBL = sKUBL;
        this.sKUPriceBL = sKUPriceBL;
        this.salesOrderBL = salesOrderBL;
        this.returnOrderBL = returnOrderBL;
        this.listHeaderBL = listHeaderBL;
        this.fileSysBL = fileSysBL;
        this.amountCalculator = amountCalculator;
        this.appConfigs = appConfigs;
        this.apiService = apiService;
        this._dataManager = dataManager;
    }
    public ReturnOrderViewModelFactory(
        IServiceProvider serviceProvider,
        IFilterHelper filter,
        ISortHelper sorter,
        Interfaces.IReturnOrderAmountCalculator amountCalculator,
        IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs,
        Winit.Modules.Base.BL.ApiService apiService,
        IAppUser appUser)
    {
        this.serviceProvider = serviceProvider;
        this.filter = filter;
        this.sorter = sorter;
        this.listHelper = listHelper;
        this.appUser = appUser;
        this.apiService = apiService;
        this.appConfigs = appConfigs;
    }
    public IReturnOrderViewModel CreateReturnOrderViewModel(string orderType, string source)
    {
        switch (orderType)
        {
            case ReturnOrderType.WithInvoice:
                if (source == SourceType.APP)
                    return new ReturnOrderWithInvoiceAppViewModel(serviceProvider, filter, sorter, amountCalculator, listHelper, appConfigs,
                        apiService, appUser, sKUBL, listHeaderBL,
                        sKUPriceBL, returnOrderBL, fileSysBL, salesOrderBL, localizer, _dataManager
                      );
                else
                {
                    return new ReturnOrderWithInvoiceWebViewModel(serviceProvider, filter, sorter, amountCalculator, listHelper, appConfigs,
                    apiService, appUser);
                }
            case ReturnOrderType.WithoutInvoice:
                if (source == SourceType.APP)
                    return new ReturnOrderAppViewModel(serviceProvider, filter, sorter, amountCalculator, listHelper, appConfigs,
                        apiService, appUser, sKUBL, listHeaderBL,
                        sKUPriceBL, returnOrderBL, fileSysBL, localizer, _dataManager
                      );
                else
                {
                    return new ReturnOrderWebViewModel(serviceProvider, filter, sorter, amountCalculator, listHelper, appConfigs,
                        apiService, appUser);
                }
            default:
                return null;
        }
    }
}
