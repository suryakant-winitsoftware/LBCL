using iTextSharp.text;
using Winit.Modules.DashBoard.BL.Interfaces;
using Winit.Modules.DashBoard.Model.Classes;
using Winit.Modules.DashBoard.Model.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.Shared.Models.Models.ReportController;

namespace Winit.Modules.DashBoard.BL.Classes;

public abstract class DashboardBaseViewmodel : IDashboardReportViewmodel
{
    public List<ISelectionItem>  ProductGroupDDL{ get; set; } = [];
    public List<ISelectionItem>  ProductTypeDDL{ get; set; } = [];
    protected IAppSetting _setting { get; set; }
    protected IAppConfig _appConfig { get; set; }
    public List<PieDataItem> SalesPerformances { get; set; } = [];
    public List<ICategoryPerformance> Categories { get; set; } = [];
    public List<IGrowthWiseChannelPartner> GrowthWiseChannelPartners { get; set; } = [];
    public List<IGrowthWiseChannelPartner> GrowthvsDegrowthChannelPartners { get; set; } = [];
    public List<ChartData> BarNegativeData { get; set; } = [];
    public List<string> BarNegativeYAxisLabels { get; set; } = [];
    public List<string> CategorySalesPerformanceVolumeLabel { get; set; } = [];
    public List<int> CategorySalesPerformanceVolumeData { get; set; } = [];
    public List<IDistributorPerformance> DistributorPerformance { get; set; } = [];
   public  CategoryWiseTopChhannelPartnersRequest? _categoryWiseTopChhannelPartnersRequest { get; set; }

    public List<string> Top10ChannelPartnersLabels { get; set; } = new()
    {
        "product", (DateTime.Now.Year - 1).ToString(), DateTime.Now.Year.ToString()
    };

    public List<List<object>> Top10ChannelPartnersData { get; set; } = [];

    public async Task PopulateViewModel()
    {
        await PopulateReports();
    }
    public void OnProductTypeSelected(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null)
        {
            _categoryWiseTopChhannelPartnersRequest?.Types.Clear();
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
            {
                _categoryWiseTopChhannelPartnersRequest?.Types.AddRange(dropDownEvent.SelectionItems.Select(p => p.Code));

                //StandingProvisionSchemeMaster.StandingProvisionScheme.SKUCategoryCode = dropDownEvent.SelectionItems.FirstOrDefault().Code;
            }
            else
            {
                //StandingProvisionSchemeMaster.StandingProvisionScheme.SKUCategoryCode = string.Empty;
            }
        }
    }
    public void OnProductGroupSelected(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null)
        {
            _categoryWiseTopChhannelPartnersRequest?.Groups.Clear();
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
            {
                _categoryWiseTopChhannelPartnersRequest?.Groups.AddRange(dropDownEvent.SelectionItems.Select(p => p.Code));
                //StandingProvisionSchemeMaster.StandingProvisionScheme.SKUCategoryCode = dropDownEvent.SelectionItems.FirstOrDefault().Code;
            }
            //else
            //{
            //    //StandingProvisionSchemeMaster.StandingProvisionScheme.SKUCategoryCode = string.Empty;
            //}
        }
    }
    protected void SetTopChanelPartners(List<IGrowthWiseChannelPartner> growthWiseChannelPartners)
    {
        if (growthWiseChannelPartners != null)
        {
            GrowthWiseChannelPartners.Clear();
            Top10ChannelPartnersData.Clear();
            foreach (var gwcp in growthWiseChannelPartners)
            {
                GrowthWiseChannelPartners.Add(gwcp);
                Top10ChannelPartnersData.Add(new()
                {
                    gwcp.ChannelPartnerName,
                    gwcp.LastYearSales,
                    gwcp.CurrentYearSales,
                    // gwcp.SalesGrowth,
                    // gwcp.GrowthPercentage,
                });
            }
        }
    }

    protected void SetGrowthvsDegrowthData(List<IGrowthWiseChannelPartner> growthWiseChannelPartners)
    {
        if (growthWiseChannelPartners != null)
        {
            BarNegativeData.Clear();
            BarNegativeYAxisLabels.Clear();
            GrowthvsDegrowthChannelPartners.Clear();
            foreach (var gwcp in growthWiseChannelPartners)
            {
                GrowthvsDegrowthChannelPartners.Add(gwcp);
                BarNegativeData.Add(gwcp.GrowthPercentage > 0
                    ? new ChartData()
                    {
                        Value = gwcp.GrowthPercentage,
                    }
                    : new ChartData()
                    {
                        Value = gwcp.GrowthPercentage,
                        Label = new LabelOptions { Color = "#fff" },
                        ItemStyle = new ItemStyleOptions
                        {
                            Color = "#FF5733"
                        }
                    });
                BarNegativeYAxisLabels.Add(gwcp.ChannelPartnerName);
            }
            // GrowthvsDegrowthChannelPartners.Add(new GrowthWiseChannelPartner()
            // {
            //     ChannelPartnerName = "ABC Store3",
            //     CurrentYearSales = 7200,
            //     LastYearSales = 8000,
            //     SalesGrowth = -800,
            //     GrowthPercentage = -10,
            // });
            // BarNegativeData.Add( new ChartData()
            //     {
            //         Value = -10,
            //         Label = new LabelOptions { Color = "#fff" },
            //         ItemStyle = new ItemStyleOptions
            //         {
            //             Color = "#FF5733"
            //         }
            //     });
            // BarNegativeYAxisLabels.Add("ABC Store3");
        }
    }

    protected abstract Task PopulateReports();
}