using Nest;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.DashBoard.Model.Classes;
using Winit.Modules.DashBoard.Model.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Constants;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.Shared.Models.Models.ReportController;

namespace Winit.Modules.DashBoard.BL.Classes;

public class DashboardWebViewmodel : DashboardBaseViewmodel
{
    ApiService _apiService;
    IAppUser _user;
    IDataManager _dataManager;

    public DashboardWebViewmodel(ApiService apiService, IAppConfig appConfig, IAppSetting setting, IAppUser user, IDataManager dataManager)
    {
        _setting = setting;
        _apiService = apiService;
        _appConfig = appConfig;
        _user = user;
        _dataManager = dataManager;

        _categoryWiseTopChhannelPartnersRequest = new()
        {
            LastYear = Year - 1,
            CurrentYear = Year,
            Count = 10,
            Types = [],
            Groups = []
        };
    }

    private int Month
    {
        get
        {
            var calender = _user.CalenderPeriods.Find(p => p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now);
            if (calender != null)
            {
                return calender.PeriodNum;
            }

            return DateTime.Now.Month;
        }
    }

    private int Year
    {
        get
        {
            var calender = _user.CalenderPeriods.Find(p => p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now);
            if (calender != null)
            {
                return calender.PeriodYear;
            }

            return DateTime.Now.Year;
        }
    }

    protected override async Task PopulateReports()
    {
        await Task.WhenAll(
            GetSalesPerformance(0,
                Year, 10),
            GetCategorySalesPerformance(Month, Year, 10),
            GetTopChanelPartners(Year - 1, Year, 10),
            GetGrowthVsDeGrowth(Year - 1, Year, 10),
            GetTargetVsAchievement(Month, Year, 10)
        );
        PopulateDropDowns();
    }
    protected void PopulateDropDowns()
    {
        var sKUGroup = (List<ISKUGroup>)_dataManager.GetData(Winit.Shared.Models.Constants.CommonMasterDataConstants.SKUGroup);
        var sKUGroupType = (List<ISKUGroupType>)_dataManager.GetData(Winit.Shared.Models.Constants.CommonMasterDataConstants.SKUGroupType);
        if (sKUGroup != null && sKUGroupType != null)
        {
            string categoryUID = sKUGroupType.Find(p => p.Code == SKUGroupTypeContants.Category)?.UID ?? string.Empty;
            string typeUID = sKUGroupType.Find(p => p.Code == SKUGroupTypeContants.Product_Type)?.UID ?? string.Empty;
            string starRatingUID = sKUGroupType.Find(p => p.Code == SKUGroupTypeContants.StarRating)?.UID ?? string.Empty;
            string tonnageUID = sKUGroupType.Find(p => p.Code == SKUGroupTypeContants.TONAGE)?.UID ?? string.Empty;
            string itemSeriesUID = sKUGroupType.Find(p => p.Code == SKUGroupTypeContants.Item_Series)?.UID ?? string.Empty;
            string capacityUID = sKUGroupType.Find(p => p.Code == SKUGroupTypeContants.Capacity)?.UID ?? string.Empty;
            string groupUID = sKUGroupType.Find(p => p.Code == SKUGroupTypeContants.ProductGroup)?.UID ?? string.Empty;

            ProductGroupDDL.Clear();
            ProductTypeDDL.Clear();

            sKUGroup.ForEach(item =>
            {
                var selectionItem = new SelectionItem()
                {
                    UID = item.UID,
                    Label = item.Name,
                    Code = item.Code,
                };
                if (item.SKUGroupTypeUID.Equals(groupUID))
                {
                    ProductGroupDDL.Add(selectionItem);
                }
                else if (item.SKUGroupTypeUID.Equals(typeUID))
                {
                    ProductTypeDDL.Add(selectionItem);
                }
            });
        }
    }
    public async Task GetSalesPerformanceMTD(bool isYTD)
    {
        await GetSalesPerformance(isYTD ? 0 : Month, Year, 10);
    }
    public async Task GetSalesPerformance(int month, int year, int count)
    {
        ApiResponse<List<ISalesPerformance>> resp =
            await _apiService.FetchDataAsync<List<ISalesPerformance>>(
                $"{_appConfig.ApiBaseUrl}DashBoard/GetSalesPerformance?month={Month}&year={year}&count={count}",
                HttpMethod.Get);
        if (resp != null)
        {
            if (resp.IsSuccess && resp.Data != null)
            {
                SalesPerformances.Clear();
                resp.Data.ForEach(p =>
                {
                    SalesPerformances.Add(new PieDataItem() { Name = p.StoreName, Value = p.NetAmount });
                });
            }
        }
    }

    public async Task GetCategorySalesPerformance(int month, int year, int count)
    {
        ApiResponse<List<ICategoryPerformance>> resp =
            await _apiService.FetchDataAsync<List<ICategoryPerformance>>(
                $"{_appConfig.ApiBaseUrl}DashBoard/GetCategorySalesPerformance?month={month}&year={year}&count={count}",
                HttpMethod.Get);
        if (resp != null)
        {
            if (resp.IsSuccess && resp.Data != null)
            {
                Categories.Clear();
                CategorySalesPerformanceVolumeData.Clear();
                CategorySalesPerformanceVolumeLabel.Clear();
                resp.Data.ForEach(p =>
                {
                    Categories.Add(p);
                    CategorySalesPerformanceVolumeLabel.Add(p.CategoryName);
                    CategorySalesPerformanceVolumeData.Add(CommonFunctions.GetIntValue(p.TotalVolume));
                });
            }
        }
    }
    public async Task GetTargetVsAchievement(int month, int year, int count)
    {
        ApiResponse<List<IDistributorPerformance>> resp =
            await _apiService.FetchDataAsync<List<IDistributorPerformance>>(
                $"{_appConfig.ApiBaseUrl}DashBoard/GetTargetVsAchievement?Month={month}&Year={year}&count={count}",
                HttpMethod.Get);
        if (resp != null)
        {
            if (resp.IsSuccess && resp.Data != null)
            {
                DistributorPerformance.Clear();
                DistributorPerformance.AddRange(resp.Data);
            }
        }
    }

    public async Task GetGrowthVsDeGrowth(int LastYear, int CurrentYear, int count)
    {
        ApiResponse<List<IGrowthWiseChannelPartner>> resp =
            await _apiService.FetchDataAsync<List<IGrowthWiseChannelPartner>>(
                $"{_appConfig.ApiBaseUrl}DashBoard/GetGrowthVsDeGrowth?LastYear={LastYear}&CurrentYear={CurrentYear}&count={count}",
                HttpMethod.Get);

        SetGrowthvsDegrowthData(resp != null && resp.IsSuccess && resp.Data != null ? resp.Data : []);
    }
    public async Task GetTopChanelPartners(int LastYear, int CurrentYear, int count)
    {
        ApiResponse<List<IGrowthWiseChannelPartner>> resp =
            await _apiService.FetchDataAsync<List<IGrowthWiseChannelPartner>>(
                $"{_appConfig.ApiBaseUrl}DashBoard/GetTopChanelPartners?LastYear={LastYear}&CurrentYear={CurrentYear}&count={count}",
                HttpMethod.Get);

        SetTopChanelPartners(resp != null && resp.IsSuccess && resp.Data != null ? resp.Data : []);
    }
    public async Task GetTopChanelPartnersByCategoryAndGroup()
    {

        ApiResponse<List<IGrowthWiseChannelPartner>> resp =
            await _apiService.FetchDataAsync<List<IGrowthWiseChannelPartner>>(
                $"{_appConfig.ApiBaseUrl}DashBoard/GetTopChanelPartnersByCategoryAndGroup",
                HttpMethod.Post, _categoryWiseTopChhannelPartnersRequest);

        SetTopChanelPartners(resp != null && resp.IsSuccess && resp.Data != null ? resp.Data : []);
    }
}