using Winit.Modules.DashBoard.Model.Classes;
using Winit.Modules.DashBoard.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.DashBoard.BL.Interfaces;

public interface IDashBoardBL
{
    Task<List<ISalesPerformance>> GetSalesPerformance(int month, int year, int count);
    Task<List<ICategoryPerformance>> GetCategorySalesPerformance(int month, int year, int count);
    Task<List<IGrowthWiseChannelPartner>> GetTopChanelPartners(int LastYear, int CurrentYear, int count);
    Task<List<IGrowthWiseChannelPartner>> GetGrowthVsDeGrowth(int LastYear, int CurrentYear, int count);
    Task<List<IDistributorPerformance>> GetTargetVsAchievement(int Year, int Month, int count);
    Task<List<IGrowthWiseChannelPartner>> GetTopChanelPartnersByCategoryAndGroup(CategoryWiseTopChhannelPartnersRequest request);
    Task<PagedResponse<IBranchSalesReport>> GetBranchSalesReport(List<SortCriteria> sortCriterias, int pageNumber, 
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, bool isForExport);
    Task<List<IBranchSalesReportAsmview>> GetAsmViewByBranchCode(string branchCode);
    Task<List<IBranchSalesReportOrgview>> GetOrgViewByBranchCode(string branchCode);
}