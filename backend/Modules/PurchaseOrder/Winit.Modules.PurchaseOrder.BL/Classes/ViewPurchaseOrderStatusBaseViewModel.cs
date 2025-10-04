using Nest;
using Winit.Modules.Base.BL;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.PurchaseOrder.BL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Constatnts;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PurchaseOrder.BL.Classes;

public class ViewPurchaseOrderStatusBaseViewModel : IViewPurchaseOrderStatusViewModel
{
    //Dependecy injection
    private readonly IServiceProvider _serviceProvider;
    private readonly IFilterHelper _filter;
    private readonly ISortHelper _sorter;
    private readonly IListHelper _listHelper;
    private readonly IAppUser _appUser;
    private readonly IAppSetting _appSetting;
    private readonly IDataManager _dataManager;
    private readonly IAppConfig _appConfigs;
    private readonly ApiService _apiService;
    public List<FilterCriteria> FilterCriterias { get; set; }
    public List<SortCriteria> SortCriterias { get; set; }
    public List<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderHeaderItem> DisplayHeaderList { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public List<ISelectionItem> ChannelPartnerSelectionItems { get; set; }
    public string Status { get; set; }
    public List<ISelectionItem> OracleOrderStatusSelectionItems { get; set; }

    private SortCriteria DefaultSortCriteria = new("OrderDate", SortDirection.Desc);

    public ViewPurchaseOrderStatusBaseViewModel(IServiceProvider serviceProvider,
        IFilterHelper filter,
        ISortHelper sorter,
        IListHelper listHelper,
        IAppUser appUser,
        IAppSetting appSetting,
        IDataManager dataManager,
        IAppConfig appConfigs, ApiService apiService)
    {
        _serviceProvider = serviceProvider;
        _filter = filter;
        _sorter = sorter;
        _listHelper = listHelper;
        _appUser = appUser;
        _appSetting = appSetting;
        _dataManager = dataManager;
        _appConfigs = appConfigs;
        _apiService = apiService;

        FilterCriterias = [];
        SortCriterias = [];
        DisplayHeaderList = [];
        ChannelPartnerSelectionItems = [];
        OracleOrderStatusSelectionItems = [];
        PrepareOracleOrderStatusSelectionItems();
    }

    public async Task PopulateViewModel()
    {
        try
        {
            DisplayHeaderList.Clear();
            FilterCriteria? jobPositionFilterCriteria = FilterCriterias.Find(e => e.Name == "JobPositionUid");
            if (jobPositionFilterCriteria == null)
            {
                FilterCriterias.Add(new FilterCriteria("JobPositionUid", _appUser.Role.IsPrincipalRole ? _appUser.SelectedJobPosition.UID : null,
                FilterType.Equal));
            }


            PagingRequest pagingRequest = new()
            {
                PageSize = PageSize,
                PageNumber = PageNumber,
                IsCountRequired = true,
                FilterCriterias = FilterCriterias,
                SortCriterias = (SortCriterias == null || !SortCriterias.Any()) ? [DefaultSortCriteria] : SortCriterias
            };
            FilterCriteria? filterCriteria = FilterCriterias.Find(e => e.Name == "status");
            if (filterCriteria != null)
            {
                FilterCriterias.Remove(filterCriteria);
            }
            if (!string.IsNullOrEmpty(Status))
            {
                FilterCriterias.Add(new FilterCriteria
                (
                "status",
                Status,
                FilterType.Equal
                ));
            }

            if (!_appUser.Role.IsPrincipalRole)
            {
                FilterCriteria? orgfilterCriteria = FilterCriterias.Find(e => e.Name == "OrgUID");
                if (orgfilterCriteria == null)
                {
                    FilterCriterias.Add(new FilterCriteria
                    (
                    "OrgUID",
                    _appUser.SelectedJobPosition.OrgUID,
                    FilterType.Equal
                    ));
                }
            }

            if (!string.IsNullOrEmpty(Status) && filterCriteria != null)
            {
                filterCriteria.Value = Status;
            }
            FilterCriterias.RemoveAll(e => e.Name == "DivisionUID");
            FilterCriterias.RemoveAll(e => e.Name == "ReportingEmpUID");
            if (_appUser.Role.Code == "ASM" &&
                !FilterCriterias.Exists(e => e.Name == "status" && e.Value == PurchaseOrderStatusConst.Draft))
            {
                FilterCriterias.Add(new FilterCriteria("DivisionUID", _appUser.AsmDivisions, FilterType.In));
            }
            if (_appUser.Role.Code == "ASM" &&
                !FilterCriterias.Exists(e => e.Name == "ReportingEmpUID") &&
                !FilterCriterias.Exists(e => e.Name == "status" && e.Value == PurchaseOrderStatusConst.Draft))
            {
                FilterCriterias.Add(new FilterCriteria("ReportingEmpUID", _appUser.Emp.UID, FilterType.Equal));
            }
            ApiResponse<PagedResponse<IPurchaseOrderHeaderItem>> apiResponse = await _apiService
                .FetchDataAsync<PagedResponse<IPurchaseOrderHeaderItem>>(
                $"{_appConfigs.ApiBaseUrl}PurchaseOrder/GetPurchaseOrderHeaders",
                HttpMethod.Post,
                pagingRequest
                );

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null &&
                apiResponse.Data.PagedData != null)
            {
                if (apiResponse.Data.PagedData is not null)
                {
                    for (int i = 0; i < apiResponse.Data.PagedData.Count(); i++)
                    {
                        apiResponse.Data.PagedData.ToList()[i].SerialNumber = ((PageNumber - 1) * PageSize) + i + 1;
                    }
                    if (apiResponse.Data.TotalCount >= 0)
                    {
                        TotalCount = apiResponse.Data.TotalCount;
                    }
                    DisplayHeaderList.AddRange(apiResponse.Data.PagedData.ToList<IPurchaseOrderHeaderItem>());
                }
            }
            else
            {
                TotalCount = 0;
            }
        }
        catch (Exception)
        {
            TotalCount = 0;
            throw;
        }
    }

    public async Task OnSorting(SortCriteria sortCriteria)
    {
        SortCriterias.Clear();
        SortCriterias.Add(sortCriteria);
        await PopulateViewModel();
    }

    public async Task PageIndexChanged(int pageNumber)
    {
        PageNumber = pageNumber;
        await PopulateViewModel();
    }

    public async Task<Dictionary<string, int>> GetTabItemsCount(List<FilterCriteria> filterCriterias)
    {
        try
        {
            FilterCriteria? jobPositionFilterCriteria = FilterCriterias.Find(e => e.Name == "JobPositionUid");
            if (jobPositionFilterCriteria == null)
            {
                FilterCriterias.Add(new FilterCriteria("JobPositionUid", _appUser.Role.IsPrincipalRole ? _appUser.SelectedJobPosition.UID : null,
                FilterType.Equal));
            }


            PagingRequest pagingRequest = new()
            {
                PageSize = PageSize,
                PageNumber = PageNumber,
                IsCountRequired = true,
                FilterCriterias = FilterCriterias,
                SortCriterias = (SortCriterias == null || !SortCriterias.Any()) ? [DefaultSortCriteria] : SortCriterias
            };

            if (!_appUser.Role.IsPrincipalRole)
            {
                FilterCriteria? orgfilterCriteria = FilterCriterias.Find(e => e.Name == "OrgUID");
                if (orgfilterCriteria == null)
                {
                    FilterCriterias.Add(new FilterCriteria
                    (
                    "OrgUID",
                    _appUser.SelectedJobPosition.OrgUID,
                    FilterType.Equal
                    ));
                }
            }
            FilterCriterias.RemoveAll(e => e.Name == "DivisionUID");
            FilterCriterias.RemoveAll(e => e.Name == "ReportingEmpUID");
            if (_appUser.Role.Code == "ASM" &&
                !FilterCriterias.Exists(e => e.Name == "status" && e.Value == PurchaseOrderStatusConst.Draft))
            {
                FilterCriterias.Add(new FilterCriteria("DivisionUID", _appUser.AsmDivisions, FilterType.In));
            }
            if (_appUser.Role.Code == "ASM" &&
                !FilterCriterias.Exists(e => e.Name == "ReportingEmpUID") &&
                !FilterCriterias.Exists(e => e.Name == "status" && e.Value == PurchaseOrderStatusConst.Draft))
            {
                FilterCriterias.Add(new FilterCriteria("ReportingEmpUID", _appUser.Emp.UID, FilterType.Equal));
            }
            FilterCriterias.RemoveAll(e => e.Name == "status");

            ApiResponse<Dictionary<string, int>> apiResponse =
                await _apiService.FetchDataAsync<Dictionary<string, int>>(
                $"{_appConfigs.ApiBaseUrl}PurchaseOrder/GetPurchaseOrderSatatusCounts",
                HttpMethod.Post,
                filterCriterias
                );
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                return apiResponse.Data;
            }
            return [];
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task<List<IStore>?> GetChannelPartner()
    {
        ApiResponse<List<IStore>> apiResponse = await _apiService.FetchDataAsync<List<IStore>>(
        $"{_appConfigs.ApiBaseUrl}Store/GetChannelPartner?jobPositionUid={_appUser.SelectedJobPosition.UID}",
        HttpMethod.Get);

        return apiResponse != null && apiResponse.Data != null ? apiResponse.Data : null;
    }

    public async Task LoadChannelPartner()
    {
        var data = await GetChannelPartner();
        if (data != null && data.Any())
        {
            ChannelPartnerSelectionItems.AddRange(
            CommonFunctions.ConvertToSelectionItems(data, e => e.UID, e => e.Code, e => $"[{e.Code}] {e.Name}"));
        }
    }


    public async Task ApplyFilter(List<FilterCriteria> filterCriterias)
    {
        FilterCriterias.Clear();
        FilterCriterias.AddRange(filterCriterias);
        await PopulateViewModel();
    }
    private void PrepareOracleOrderStatusSelectionItems()
    {
        OracleOrderStatusSelectionItems.Clear();
        OracleOrderStatusSelectionItems.Add(new SelectionItem
        {
            UID = PurchaseOrderErpStatusConst.InProcess,
            Code = PurchaseOrderErpStatusConst.InProcess,
            Label = PurchaseOrderErpStatusConst.InProcess
        });
        OracleOrderStatusSelectionItems.Add(new SelectionItem
        {
            UID = PurchaseOrderErpStatusConst.Completed,
            Code = PurchaseOrderErpStatusConst.Completed,
            Label = PurchaseOrderErpStatusConst.Completed
        });
        OracleOrderStatusSelectionItems.Add(new SelectionItem
        {
            UID = PurchaseOrderErpStatusConst.PartialCompleted,
            Code = PurchaseOrderErpStatusConst.PartialCompleted,
            Label = PurchaseOrderErpStatusConst.PartialCompleted
        });
        OracleOrderStatusSelectionItems.Add(new SelectionItem
        {
            UID = PurchaseOrderErpStatusConst.Cancelled,
            Code = PurchaseOrderErpStatusConst.Cancelled,
            Label = PurchaseOrderErpStatusConst.Cancelled
        });
    }
}
