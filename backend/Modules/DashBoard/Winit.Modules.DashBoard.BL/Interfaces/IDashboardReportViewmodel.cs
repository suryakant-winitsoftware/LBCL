using Winit.Modules.DashBoard.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.Shared.Models.Models.ReportController;

namespace Winit.Modules.DashBoard.BL.Interfaces;

public interface IDashboardReportViewmodel
{
    List<string> Top10ChannelPartnersLabels { get; set; }
    List<List<object>> Top10ChannelPartnersData { get; set; }
    List<ChartData> BarNegativeData { get; set; }
    List<string> BarNegativeYAxisLabels { get; set; }
    Task PopulateViewModel();
    void OnProductTypeSelected(DropDownEvent dropDownEvent);
    void OnProductGroupSelected(DropDownEvent dropDownEvent);

    List<PieDataItem> SalesPerformances { get; set; }
    List<ICategoryPerformance> Categories { get; set; }
    List<IGrowthWiseChannelPartner> GrowthWiseChannelPartners { get; set; }
    List<IGrowthWiseChannelPartner> GrowthvsDegrowthChannelPartners { get; set; }
    List<IDistributorPerformance> DistributorPerformance { get; set; }
    List<string> CategorySalesPerformanceVolumeLabel { get; set; }
    List<int> CategorySalesPerformanceVolumeData { get; set; }
    List<ISelectionItem> ProductGroupDDL { get; set; }
    List<ISelectionItem> ProductTypeDDL { get; set; }
}