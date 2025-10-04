using Winit.Modules.DashBoard.BL.Interfaces;
using Winit.Modules.DashBoard.DL.Interfaces;
using Winit.Modules.DashBoard.Model.Classes;
using Winit.Modules.DashBoard.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.DashBoard.BL.Classes;

public class DashBoardBL : IDashBoardBL
{
    private readonly IDashBoardDL _dashBoardDl;

    public DashBoardBL(IDashBoardDL dashBoardDl)
    {
        _dashBoardDl = dashBoardDl;
    }

    public async Task<List<ISalesPerformance>> GetSalesPerformance(int month, int year, int count)
    {
        return await _dashBoardDl.GetSalesPerformance(month, year, count);
    }

    public async Task<List<ICategoryPerformance>> GetCategorySalesPerformance(int month, int year, int count)
    {
        return await _dashBoardDl.GetCategorySalesPerformance(month, year, count);
    }

    public async Task<List<IGrowthWiseChannelPartner>> GetTopChanelPartners(int LastYear, int CurrentYear, int count)
    {
        return await _dashBoardDl.GetTopChanelPartners(LastYear, CurrentYear, count);
    }
    public async Task<List<IGrowthWiseChannelPartner>> GetTopChanelPartnersByCategoryAndGroup(CategoryWiseTopChhannelPartnersRequest request)
    {
        return await _dashBoardDl.GetTopChanelPartnersByCategoryAndGroup(request);
    }
    public async Task<List<IGrowthWiseChannelPartner>> GetGrowthVsDeGrowth(int LastYear, int CurrentYear, int count)
    {
        return await _dashBoardDl.GetGrowthVsDeGrowth(LastYear, CurrentYear, count);
    }
    public async Task<List<IDistributorPerformance>> GetTargetVsAchievement(int Year, int Month, int count)
    {
        return await _dashBoardDl.GetTargetVsAchievement(Year, Month, count);
    }
    public async Task<PagedResponse<IBranchSalesReport>> GetBranchSalesReport(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, bool isForExport)
    {
        return await _dashBoardDl.GetBranchSalesReport(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired,isForExport);
    }
    public async Task<List<IBranchSalesReportAsmview>> GetAsmViewByBranchCode(string branchCode)
    {
        return await _dashBoardDl.GetAsmViewByBranchCode(branchCode);
    }
    public async Task<List<IBranchSalesReportOrgview>> GetOrgViewByBranchCode(string branchCode)
    {
        return await _dashBoardDl.GetOrgViewByBranchCode(branchCode);
    }
}