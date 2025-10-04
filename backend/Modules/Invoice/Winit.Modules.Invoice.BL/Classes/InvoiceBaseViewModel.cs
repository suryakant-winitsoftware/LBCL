using Newtonsoft.Json;
using System.Reflection;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Invoice.BL.Interfaces;
using Winit.Modules.Invoice.Model.Classes;
using Winit.Modules.Invoice.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Invoice.BL.Classes;

public class InvoiceBaseViewModel : IInvoiceViewModel
{
    public List<FilterCriteria> FilterCriterias { get; set; }
    public List<SortCriteria> SortCriterias { get; set; }
    public List<IInvoiceHeaderView> InvoiceHeaderViews { get; set; }
    public IInvoiceMaster InvoiceMaster { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public List<ISelectionItem> ChannelPartnerSelectionItem { get; set; }
    public List<IOutstandingInvoiceReport> OutstandingInvoiceReportDataList { get; set; }

    //DI
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppUser _appUser;
    private readonly IAppSetting _appSetting;
    private readonly IAppConfig _appConfigs;
    private readonly IInvoiceDataHelper _inoviceDataHelper;
    private Winit.Modules.Base.BL.ApiService _apiService;

    public InvoiceBaseViewModel(IServiceProvider serviceProvider,
        IAppUser appUser,
        IAppSetting appSetting,
        IAppConfig appConfigs,
        IInvoiceDataHelper invoiceDataHelper, Winit.Modules.Base.BL.ApiService apiService)
    {
        _serviceProvider = serviceProvider;
        _appUser = appUser;
        _appSetting = appSetting;
        _appConfigs = appConfigs;
        _inoviceDataHelper = invoiceDataHelper;
        _apiService = apiService;
        InvoiceHeaderViews = [];
        FilterCriterias = [];
        SortCriterias = [];
        InvoiceMaster = new InvoiceMaster();
        ChannelPartnerSelectionItem = [];
        OutstandingInvoiceReportDataList = new List<IOutstandingInvoiceReport>();
        FilterCriterias.Add(new FilterCriteria("JobPositionUid", _appUser.SelectedJobPosition.UID, FilterType.Equal));
    }

    public async Task PopulateViewModel()
    {

        //FilterCriteria? filterCriteria = FilterCriterias.Find(e => "OrgUID".Equals(e.Name, StringComparison.OrdinalIgnoreCase));
        //if (filterCriteria == null)
        //{
        //    FilterCriterias.Add(new FilterCriteria("orguid", _appUser.SelectedJobPosition.OrgUID, FilterType.Equal));
        //}
        await getInvoices();
    }
    private async Task getInvoices()
    {
        PagedResponse<IInvoiceHeaderView>? data = await _inoviceDataHelper
           .GetAllInvoices(SortCriterias, PageNumber, PageSize, FilterCriterias, true);
        InvoiceHeaderViews.Clear();
        if (data is not null)
        {
            TotalCount = data.TotalCount;
            InvoiceHeaderViews.AddRange(data.PagedData);
        }
    }
    public async Task PageIndexChanged(int index)
    {
        PageNumber = index;
        await PopulateViewModel();
    }

    public async Task OnSortClick(SortCriteria sortCriteria)
    {
        SortCriterias.Clear();
        SortCriterias.Add(sortCriteria);
        await PopulateViewModel();
    }

    public async Task ApplyFilter(List<FilterCriteria> filters)
    {
        FilterCriterias.Clear();
        FilterCriteria? filterCriteria = FilterCriterias.Find(e => "JobPositionUid".Equals(e.Name, StringComparison.OrdinalIgnoreCase));
        if (filterCriteria == null)
        {
            FilterCriterias.Add(new FilterCriteria("JobPositionUid", _appUser.SelectedJobPosition.UID, FilterType.Equal));
        }
        FilterCriterias.AddRange(filters);
        await PopulateViewModel();
    }

    public async Task LoadInvoiceMasterByInoviceNo(string invoiceUid)
    {
        var data = await _inoviceDataHelper.GetInvoiceMasterByInvoiceUID(invoiceUid);
        if (data != null)
        {
            InvoiceMaster = data;
        }
    }

    public async Task LoadChannelPartner()
    {
        var data = await _inoviceDataHelper.GetChannelPartner(_appUser.SelectedJobPosition.UID);
        if (data != null && data.Any())
        {
            ChannelPartnerSelectionItem.AddRange(
            CommonFunctions.ConvertToSelectionItems(data, e => e.UID, e => e.Code, e => $"[{e.Code}] {e.Name}"));
        }
    }

    public async Task GetOutstandingInvoiceReportData()
    {
        OutstandingInvoiceReportDataList = await GetOutstandingInvoiceReportDataFromApiAsync();
    }

    async private Task<List<IOutstandingInvoiceReport>> GetOutstandingInvoiceReportDataFromApiAsync()
    {
        try
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.IsCountRequired = true;
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}Invoice/GetOutstandingInvoiceReportData",
            HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                ApiResponse<PagedResponse<Winit.Modules.Invoice.Model.Classes.OutstandingInvoiceReport>> pagedResponse =
                    JsonConvert
                        .DeserializeObject<
                            ApiResponse<PagedResponse<Winit.Modules.Invoice.Model.Classes.OutstandingInvoiceReport>>>(
                            apiResponse.Data);
                return pagedResponse.Data.PagedData
                    .OfType<Winit.Modules.Invoice.Model.Interfaces.IOutstandingInvoiceReport>().ToList();
            }
        }
        catch (Exception ex)
        {
        }
        return null;
    }
}
