using System.Drawing;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http;
using System.Resources;
using Blazor.ECharts.Options;
using Winit.Modules.Common.Model.Classes;
using Winit.Modules.Currency.Model.Classes;
using Winit.Modules.Currency.Model.Interfaces;
using Winit.Modules.Emp.Model.Classes;
using Winit.Modules.ErrorHandling.Model.Classes;
using Winit.Modules.ErrorHandling.Model.Interfaces;
using Winit.Modules.JobPosition.Model.Classes;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Role.Model.Classes;
using Winit.Modules.Role.Model.Interfaces;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common.Language;
using WinIt.Pages.Base;
using Winit.Shared.Models.Constants.ReportEngine;
using Winit.Shared.Models.Models.ReportController;
using Winit.Modules.DashBoard.BL.Classes;

namespace WinIt.Pages;

partial class DashBoard : BaseComponentBase
{
    //boolean added by shanmukha
    private bool IsFilterOpen = false;
    private bool IsLoad = false;

    protected async override Task OnInitializedAsync()
    {
        try
        {
            ShowLoader();
            await _viewmodel.PopulateViewModel();
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            isTop10CPLoad = true;
            IsLoad = true;
            HideLoader();
        }
    }
    bool pieYTD = true;
    public async Task GetSalesPerformanceMTD(bool isYTD)
    {
        pieYTD = isYTD;
        ShowLoader();
        await ((DashboardWebViewmodel)_viewmodel).GetSalesPerformanceMTD(isYTD);
        HideLoader();
        StateHasChanged();
    }
    bool isTop10CPLoad { get; set; }
    private async Task GetTopChanelPartnersByCategoryAndGroup()
    {
        ShowLoader();
        isTop10CPLoad = false;
        StateHasChanged();
        await ((DashboardWebViewmodel)_viewmodel).GetTopChanelPartnersByCategoryAndGroup();
        isTop10CPLoad = true;
        StateHasChanged();
        HideLoader();
    }

    #region Pie Chart

    private List<Winit.Shared.Models.Models.ReportController.PieDataItem> PieChartData = new()
    {
        new PieDataItem { Value = 19, Name = "Aman" },
        new PieDataItem { Value = 16, Name = "Electronic " },
        new PieDataItem { Value = 15, Name = "Shees" },
        new PieDataItem { Value = 22, Name = "new" },
        new PieDataItem { Value = 22, Name = "Anurag" },
        new PieDataItem { Value = 29, Name = "BM sons" },
    };

    public Legend pieLegend { get; set; } = new()
    { Orient = Blazor.ECharts.Options.Enum.Orient.Vertical, Right = LabelPosition.Right };

    private void ChartsEvent(EchartsEventArgs args)
    {
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(args));
    }

    #endregion

    #region Bar with Negative

    private List<string> barNegativeYAxisLabels = new()
    {
        "Category A", "Category B", "Category C", "Category D", "Category E"
    };

    private List<ChartData> barNegativeData = new()
    {
        new ChartData
        {
            Value = -5, Label = new LabelOptions { Color = "#fff" },
            ItemStyle = new ItemStyleOptions { Color = "#FF5733" }
        },
        new ChartData
        {
            Value = -10, Label = new LabelOptions { Color = "#fff" },
            ItemStyle = new ItemStyleOptions { Color = "#3357FF" }
        },
        new ChartData { Value = 15 },
        new ChartData { Value = 25 },
        new ChartData { Value = -8 },
    };

    private List<ChartData> TargetvsAchivement = new()
    {
        new ChartData { Value = 15 },
        new ChartData { Value = 25 },
        new ChartData { Value = 8 },
        new ChartData { Value = 23 },
        new ChartData { Value = 27 },
    };

    SeriesConfig barLineWithNegativeSeriesConfig = new SeriesConfig
    {
        Type = WChartType.Bar,
        Label = new LabelOptions
        {
            Show = true,
            Position = LabelPosition.Inside,
            FontSize = 20,

            //FontWeight = "bold",
            Formatter = "{b} [{c}%]"
        }
    };

    #endregion


    #region Basic Bar Chart

    private string[] daysBarChart = { "18k3", "12k3", "24k3", "18k3", "12k3", "18k3", "Sun" };
    private int[] salesDataBarChart = { 70, 80, 95, 50, 80, 95, 50, 80, 80 };
    private Dictionary<int, string>? barColors = new Dictionary<int, string> { { 2, "#a90000" } };

    #endregion


    #region Group Bar Chart

    private List<string> groupBarColumnNames = new()
        { "product", (DateTime.Now.Year - 1).ToString(), DateTime.Now.Year.ToString() };

    private List<List<object>> groupBarDataValues = new()
    {
        new() { "Matcha Latte", 43.3, 85.8 },
        new() { "Milk Tea", 83.1, 73.4 },
        new() { "Cheese Cocoa", 86.4, 65.2},
        new() { "Walnut Brownie", 7240, 53.9 }
    };

    private List<SeriesConfig> seriesConfigs = new()
    {
        new SeriesConfig
        {
            Type = "bar", Color = "#FF5733",
            Label = new LabelOptions
            {
                Show = true,
                Position = "insideBottom",
                FontSize = 14,
                FontWeight = "bold",
                Color = "#fff",
                Rotate = 90,
                Distance = 20,
                VerticalAlign = "middle",
                Align = "left"
            }
        },

        new SeriesConfig
        {
            Type = "bar", Color = "#3357FF",
            Label = new LabelOptions
            {
                Show = true,
                Position = "insideBottom",
                FontSize = 14,
                FontWeight = "bold",
                Color = "#fff",
                Rotate = 90,
                Distance = 20,
                VerticalAlign = "middle",
                Align = "left"
            }
        },
        // new SeriesConfig
        // {
        //     Type = "line", Color = "#33FF57",
        //     Label = new LabelOptions
        //     {
        //         Show = true,
        //         Position = "insideBottom",
        //         FontSize = 14,
        //         FontWeight = "bold",
        //         Color = "black",
        //         Rotate = 90,
        //         Distance = 20,
        //         VerticalAlign = "middle",
        //         Align = "left"
        //     }
        // },
        // new SeriesConfig
        // {
        //     Type = "bar", Color = "#fff",
        //     Label = new LabelOptions
        //     {
        //         Show = true,
        //         Position = "insideBottom",
        //         FontSize = 14,
        //         FontWeight = "bold",
        //         Color = "#fff",
        //         Rotate = 90,
        //         Distance = 20,
        //         VerticalAlign = "middle",
        //         Align = "left"
        //     }
        // },
    };

    #endregion
}