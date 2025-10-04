using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Setting.BL.Classes;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Tax.Model.UIInterfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.Services;

namespace Winit.Modules.SKU.BL.Classes;

public abstract class MaintainStandardPriceListBaseViewModel : IMaintainStandardPriceListBaseViewModel
{
    public ISKUAttributeLevel sKUAttributeLevel { get; set; }

    public List<ISelectionItem> AttributeTypeSelectionItems { get; set; }
    public List<ISelectionItem> AttributeNameSelectionItems { get; set; }
    public List<ISelectionItem> ProductDivisionSelectionItems { get; set; }
    IAppSetting _appSetting { get; }
    protected MaintainStandardPriceListBaseViewModel(IServiceProvider serviceProvider,
        IAppUser appUser,
        IAlertService alertService, ILoadingService loadingService, IAppSetting appSetting)
    {
        _serviceProvider = serviceProvider;
        _appUser = appUser;
        _alertService = alertService;
        _loadingService = loadingService;

        SKUPriceList = new();
        SerchedSKUPriceList = new();
        AttributeTypeSelectionItems = [];
        AttributeNameSelectionItems = [];
        ProductDivisionSelectionItems = [];
        _appSetting = appSetting;

        DefaultFilter = new FilterCriteria(nameof(ISKUPrice.SKUPriceListUID), appSetting.PriceApplicationModel, Winit.Shared.Models.Enums.FilterType.Equal);
        PagingRequest.PageNumber = _currentPage;
        PagingRequest.PageSize = _pageSize;

    }
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppUser _appUser;
    private readonly IAlertService _alertService;
    ILoadingService _loadingService;
    public int _currentPage { get; set; } = 1;
    public int _pageSize { get; set; } = 20;
    public int _totalItems { get; set; } = 0;
    public string CodeOrName { get; set; } = "";
    public List<ISKUPrice> SKUPriceList { get; set; }
    public Winit.Modules.SKU.Model.Interfaces.ISKUPriceList FRPrice { get; set; }
    public List<ISKUPrice> SerchedSKUPriceList { get; set; }

    public List<ISKUPrice> sKU { get; set; }
    public bool isLoad { get; set; } = false;
    protected Winit.Shared.Models.Common.PagingRequest PagingRequest { get; set; } = new()
    {
        IsCountRequired = true,
        FilterCriterias = [],
        SortCriterias = [],
    };
    protected FilterCriteria DefaultFilter { get; set; }
    public bool IsPriceListsNeededByOrgUID { get; set; }


    public async Task PopulateViewModel()
    {
        if (IsPriceListsNeededByOrgUID)
        {
            await GetStandardPriceListDetails();
        }
        ProductDivisionSelectionItems.Clear();
        ProductDivisionSelectionItems.AddRange(_appUser.ProductDivisionSelectionItems?.Select(e => (e as SelectionItem).DeepCopy()));
        await GetSKUPriceDetails();
        isLoad = true;
    }

    public async Task OnFilterApply(Dictionary<string, string> filterCriterias)
    {
        _loadingService.ShowLoading();
        PagingRequest.FilterCriterias?.Clear();
        PagingRequest.PageNumber = _currentPage = 1;
        foreach (var keyValue in filterCriterias)
        {
            if (!string.IsNullOrEmpty(keyValue.Value))
            {
                if (keyValue.Key == "AttributeType")
                {
                    ISelectionItem? selectionItem = sKUAttributeLevel.SKUGroupTypes.Find(
                       (e => e.UID == keyValue.Value));
                    PagingRequest.FilterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", new List<string> { selectionItem.UID }, FilterType.Equal));

                }
                else if (keyValue.Key == "AttributeValue")
                {
                    var selectedUids = keyValue.Value.Split(",");
                    var selectedLabels = AttributeNameSelectionItems.Where(e => selectedUids.Contains(e.UID))
                         .Select(_ => _.UID);
                    PagingRequest.FilterCriterias!.Add(new FilterCriteria(@$"{keyValue.Key}", selectedLabels, FilterType.In));
                }
                else if (keyValue.Key == "IsActive" && keyValue.Value == "True")
                {
                    PagingRequest.FilterCriterias!.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Equal));
                }
                else if (keyValue.Key == "DivisionUID")
                {
                    PagingRequest.FilterCriterias!.Add(new FilterCriteria("DivisionUIDs", keyValue.Value.Trim().Split(","), FilterType.In));
                }
                else if (keyValue.Key == nameof(ISKUPrice.ValidFrom))
                {
                    PagingRequest.FilterCriterias!.Add(new FilterCriteria(keyValue.Key, CommonFunctions.GetDate(keyValue.Value), FilterType.GreaterThanOrEqual));
                }
                else if (keyValue.Key == nameof(ISKUPrice.ValidUpto))
                {
                    PagingRequest.FilterCriterias!.Add(new FilterCriteria(keyValue.Key, CommonFunctions.GetDate(keyValue.Value), FilterType.LessThanOrEqual));
                }
                else
                {
                    PagingRequest.FilterCriterias!.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Like));
                }
            }
        }
        await GetSKUPriceDetails();
        _loadingService.HideLoading();
    }

    public async Task OnPageChange(int pageNo)
    {
        _loadingService.ShowLoading();
        PagingRequest.PageNumber = _currentPage = pageNo;
        await GetSKUPriceDetails();
        _loadingService.HideLoading();
    }

    public async void OnSort(SortCriteria criteria)
    {
        PagingRequest.SortCriterias = [criteria];
        await GetSKUPriceDetails();
    }
    public async Task<ISKUAttributeLevel> GetAttributeType()
    {
        return await GetSKUAttributeType();
    }
    public async Task OnAttributeTypeSelect(string code)
    {
        AttributeNameSelectionItems.Clear();
        foreach (ISelectionItem item in sKUAttributeLevel.SKUGroups[code])
        {
            item.IsSelected = false; // Reset selection state
            AttributeNameSelectionItems.Add(item);
        }

    }

    #region Abstract Methods
    protected abstract Task GetStandardPriceListDetails();
    protected abstract Task GetSKUPriceDetails();
    public abstract Task<ISKUAttributeLevel> GetSKUAttributeType();
    #endregion
}
