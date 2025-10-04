using System;
using System.Collections.Generic;
using System.Linq;
using Blazor.ECharts.Components;
using Blazor.ECharts.Options;
using Microsoft.AspNetCore.Components;
using Blazor.ECharts.Options.Enum;
using L = Blazor.ECharts.Options.Series.Bar;
using Blazor.ECharts.Options.Series;
using Nest;
using System.Runtime.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Data;
using System.util;
using Winit.Shared.Models.Models.ReportController;
using System.Text.Json;
using Winit.Shared.CommonUtilities.Common;

namespace Winit.UIComponents.Common.WINITCharts
{
    public partial class WGroupChart : ComponentBase
    {
        private EBar? chart = new();
        private EChartsOption<L.Bar>? ChartOption;
        private List<EventType> EventTypes = new List<EventType> { EventType.click, EventType.brush, EventType.brushselected };

        [Parameter]
        public EventCallback<EchartsEventArgs> Callback { get; set; }
        [Parameter] public List<string> ColumnNames { get; set; }

        // Data Values (First Column: Product, Others: Values)
        [Parameter]
        public List<List<object>> DataValues { get; set; }
        // Optional: Enable axisTick (Default: false)
        [Parameter] public bool EnableAxisTick { get; set; } = false;
        // Optional: Bar Colors
        [Parameter] public List<string>? BarColors { get; set; }

        // Bar Width (Optional)
        [Parameter] public string? BarWidth { get; set; }

        // Optional: Enable Legend (Default: true)
        [Parameter] public bool ShowLegend { get; set; } = true;
        // Optional: Show Background in Bars
        [Parameter] public bool ShowBackground { get; set; } = false;
        // Optional: Background Color
        [Parameter] public string BackgroundColor { get; set; } = "rgba(180, 180, 180, 0.2)";
        [Parameter] public List<SeriesConfig> SeriesConfigurations { get; set; } = new();


        // Tooltip Configuration
        private Tooltip TooltipConfig = new() { Trigger = TooltipTrigger.Axis };

        protected override void OnInitialized()
        {
            base.OnInitialized();
            ChartOption = new()
            {
                Tooltip = TooltipConfig,
                Legend = ShowLegend ? new Legend() : null,
                Dataset = new List<Blazor.ECharts.Options.DataSet>
                {
                    new() { Source = ConvertToDataset() }
                },
                XAxis = new() { new() { Type = AxisType.Category } },
                YAxis = new() { new() { Type = AxisType.Value } },
                Series = GenerateSeries(),
                Grid = new () { new Grid{ Left = "5%", Right = "5%", Bottom = "15%", ContainLabel = true } },
                DataZoom = new List<object>
        {
            new { type = "slider", show = true, start = 0, end = 10, xAxisIndex = 0 }, // Enable horizontal scrolling
            new { type = "inside", xAxisIndex = 0 } // Allow mouse-wheel scrolling
        }

            };
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await Task.Delay(100);
            if (firstRender && chart is not null)
            {
                //CommonFunctions.LogToConsole(JsonSerializer.Serialize(ChartOption, new JsonSerializerOptions { WriteIndented = true }));
                //Console.WriteLine(JsonSerializer.Serialize(ChartOption, new JsonSerializerOptions { WriteIndented = true }));
                await chart.SetupOptionAsync(ChartOption);
                StateHasChanged();
            }
        }

        // Convert DataValues to Dataset Format
        private List<object[]> ConvertToDataset()
        {
            var dataset = new List<object[]> { ColumnNames.ToArray() };
            foreach (var row in DataValues)
            {
                dataset.Add(row.ToArray()); // Convert List<object> to object[]
            }
            return dataset;
        }

        // Generate Series based on Column Names
        private List<object> GenerateSeries()
        {
            var seriesList = new List<object>();

            for (int i = 0; i < ColumnNames.Count - 1; i++)
            {
                var seriesConfig = SeriesConfigurations.ElementAtOrDefault(i) ?? new SeriesConfig();
                seriesList.Add(new
                {
                    Type = seriesConfig.Type,
                    ItemStyle = !string.IsNullOrEmpty(seriesConfig.Color) ? new { Color = seriesConfig.Color } : null,
                    Label = seriesConfig.Label,
                    Emphasis = new
                    {
                        Label = new
                        {
                            Show = seriesConfig.Label?.Show,  // Label should appear on hover as well
                            FontSize = (seriesConfig.Label?.FontSize ?? 12) + 2, // Slightly larger on hover
                            FontWeight = "bold"
                        }
                    },
                    BarWidth = BarWidth,
                    ShowBackground = ShowBackground, // Optional Background
                    BackgroundStyle = ShowBackground ? new { color = BackgroundColor } : null, // Optional Background Style
                });
            }
            return seriesList;
        }
        #region Events
        private void OnEchartsEvent(EchartsEventArgs args)
        {
            if (Callback.HasDelegate)
            {
                Callback.InvokeAsync(args);
            }
        }
        #endregion

    }
}
