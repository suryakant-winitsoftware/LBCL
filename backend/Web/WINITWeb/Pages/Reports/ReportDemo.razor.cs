using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Winit.Shared.Models.Models.ReportController;
using WinIt.Pages.Base;
using Winit.Shared.Models.Models.ReportController;
using Blazor.ECharts.Options;
using Blazor.ECharts.Options.Series;
using Winit.Shared.Models.Constants.ReportEngine;

namespace WinIt.Pages.Reports
{
    public partial class ReportDemo
    {
        private bool IsChartReady = false;
        //Line Chart
        private string[] xAxisLabels = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

        private Dictionary<string, int[]> seriesDataBasicLineChart = new Dictionary<string, int[]>
                {
                    { "Email", new[] { 120, 132, 101, 134, 90, 230, 210 }},
                };

        private Dictionary<string, int[]> seriesDataSmoothedLineChart = new Dictionary<string, int[]>
                {
                    { "Email", new[] { 200, 300, 100, 500, 400, 700, 300 }},
                };
        private Dictionary<string, int[]> seriesDataBasicAreaChart = new Dictionary<string, int[]>
                {
                    { "Email", new[] { 150, 250, 350, 150, 550, 450, 750 }},
                };

        private Dictionary<string, int[]> seriesDataStackedLine = new Dictionary<string, int[]>
                {
                    { "Email", new[] { 120, 132, 101, 134, 90, 230, 210 }},
                    { "Union Ads", new[] { 220, 182, 191, 234, 290, 330, 310 } },
                    { "Video Ads", new[] { 150, 232, 201, 154, 190, 330, 410 } },
                    { "Direct", new[] { 320, 332, 301, 334, 390, 330, 320 } },
                    { "Search Engine", new[] { 820, 932, 901, 934, 1290, 1330, 1320 } }
         };

        private Dictionary<string, int[]> seriesDataStackedArea = new Dictionary<string, int[]>
                {
                    { "Email", new[] { 120, 132, 101, 134, 90, 230, 210 }},
                    { "Union Ads", new[] { 220, 182, 191, 234, 290, 330, 310 } },
                    { "Video Ads", new[] { 150, 232, 201, 154, 190, 330, 410 } },
                    { "Direct", new[] { 320, 332, 301, 334, 390, 330, 320 } },
                    { "Search Engine", new[] { 820, 932, 901, 934, 1290, 1330, 1320 } }
         };

        // Basic Bar Chart
        private string[] daysBarChart = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
        private int[] salesDataBarChart = { 120, 200, 150, 80, 70, 110, 130 };
        private Dictionary<int, string>? barColors = new Dictionary<int, string> { { 2, "#a90000" } };

        //Group Bar Chart
        private List<string> groupBarColumnNames = new() { "product", "2015", "2016", "2017", "2018" };
        private List<List<object>> groupBarDataValues = new()
            {
                new() { "Matcha Latte", 43.3, 85.8, 93.7, 99 },
                new() { "Milk Tea", 83.1, 73.4, 55.1, 94 },
                new() { "Cheese Cocoa", 86.4, 65.2, 82.5, 88.6 },
                new() { "Walnut Brownie", 72.4, 53.9, 39.1, 20 }
            };

        private List<SeriesConfig> seriesConfigs = new()
        {
            new SeriesConfig { Type = "bar", Color = "#FF5733",
                Label = new LabelOptions {
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
            new SeriesConfig { Type = "line", Color = "#33FF57",
                Label = new LabelOptions {
                    Show = true,
                    Position = "insideBottom",
                    FontSize = 14,
                    FontWeight = "bold",
                    Color = "black",
                    Rotate = 90,
                    Distance = 20,
                    VerticalAlign = "middle",
                    Align = "left"
                }
            },
            new SeriesConfig { Type = "bar", Color = "#3357FF",
                Label = new LabelOptions {
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
            new SeriesConfig { Type = "bar", Color = "#fff",
                Label = new LabelOptions {
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
        };
        //Bar with negative
        private List<string> barNegativeYAxisLabels = new()
        {
            "Category A", "Category B", "Category C", "Category D", "Category E"
        };

        private List<ChartData> barNegativeData = new()
        {
            new ChartData{ Value = -5, Label = new LabelOptions { Color = "#fff" }, ItemStyle = new ItemStyleOptions{Color="#FF5733" } },
            new ChartData{ Value = -10, Label = new LabelOptions { Color = "#fff" }, ItemStyle = new ItemStyleOptions{Color="#3357FF" } },
            new ChartData{ Value = 15},
            new ChartData{ Value = 25},
            new ChartData{ Value = -8},
        };
        SeriesConfig barLineWithNegativeSeriesConfig = new SeriesConfig
        {
            Type = WChartType.Bar,
            Label = new LabelOptions
            {
                Show = true,
                Position = LabelPosition.Inside,
                FontSize = 14,
                FontWeight = "bold",
                Formatter = "{b} [{c}]"
            }
        };

        // Pie Chart
        private List<Winit.Shared.Models.Models.ReportController.PieDataItem> PieChartData = new()
        {
            new PieDataItem { Value = 4500, Name = "CSE",Code="C" },
            new PieDataItem { Value = 2900, Name = "IT",Code="I" },
            new PieDataItem { Value = 1000, Name = "ETC" , Code = "E"},
            new PieDataItem { Value = 2000, Name = "Others" ,Code="O" },
        };
        public Legend pieLegend { get; set; } = new() { Orient = Blazor.ECharts.Options.Enum.Orient.Vertical, Left = LabelPosition.Left };


        protected override async Task OnInitializedAsync()
        {

        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                //await Task.Delay(2000);  // Add delay to ensure DOM is ready
                IsChartReady = true;
                StateHasChanged();
            }
        }
        
        private void ChartsEvent(EchartsEventArgs args)
        {
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(args));
        }
    }
}
