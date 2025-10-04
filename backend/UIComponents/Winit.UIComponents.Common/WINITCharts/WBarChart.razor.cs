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
using Winit.Shared.Models.Constants.ReportEngine;

namespace Winit.UIComponents.Common.WINITCharts
{
    public partial class WBarChart : ComponentBase
    {
        private EBar? chart = new EBar();
        private EChartsOption<L.Bar>? ChartOption;
        
        private List<EventType> EventTypes = new List<EventType> { EventType.click, EventType.brush, EventType.brushselected };
        private string theme = "light";
        [Parameter]
        public EventCallback<EchartsEventArgs> Callback { get; set; }
        [Parameter] public string[] XAxisLabels { get; set; } = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
        [Parameter] public int[] Data { get; set; } = { 0 };
        // Optional: Enable axisTick (Default: false)
        [Parameter] public bool? EnableAxisTick { get; set; } = true;
        /// <summary>
        /// Can be any value from WChartType
        /// </summary>
        [Parameter] public string? ChartType { get; set; } = WChartType.Bar; 
        // Optional: Bar Width (null means not set)
        /// <summary>
        /// Can be in % or px
        /// </summary>
        [Parameter] public string? BarWidth { get; set; }

        // Optional: Custom bar colors (index-based)
        [Parameter] public Dictionary<int, string>? BarColors { get; set; } // Key: Index, Value: Color
        // Optional: Show Background in Bars
        [Parameter] public bool ShowBackground { get; set; } = false;
        // Optional: Background Color
        [Parameter] public string BackgroundColor { get; set; } = "rgba(180, 180, 180, 0.2)";

        private Tooltip TooltipConfig = new()
        {
            Trigger = TooltipTrigger.Axis, // Equivalent to trigger: 'axis'
            AxisPointer = new TooltipAxisPointer()
            {
                Type = AxisPointerType.Shadow // Equivalent to type: 'shadow'
            }
        };
        protected override void OnInitialized()
        {
            base.OnInitialized();
            ChartOption = new()
            {
                Tooltip = TooltipConfig,
                XAxis = new()
                {
                    new()
                    {
                        Type = AxisType.Category,
                        Data = XAxisLabels,
                        AxisTick = EnableAxisTick??false ? new AxisTick() { AlignWithLabel = true } : null
                    }
                },
                YAxis = new() { new() { Type = AxisType.Value } },
                Series = new List<object>
                {
                    new
                    {
                        Data = ConvertToSeriesData(Data, BarColors),
                        Type = ChartType,
                        BarWidth = BarWidth,
                        ShowBackground = ShowBackground, // Optional Background
                        BackgroundStyle = ShowBackground ? new { color = BackgroundColor } : null // Optional Background Style
                    }
                }
            };
            
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await Task.Delay(100); // Delay to ensure rendering is complete
            if (firstRender && chart is not null)
            {
                await chart.SetupOptionAsync(ChartOption);
                StateHasChanged(); // Ensure Blazor re-renders properly
            }
        }
        

        // Convert int[] to List<object> (Allowing styling per bar)
        private List<object> ConvertToSeriesData(int[] data, Dictionary<int, string>? colors)
        {
            var seriesData = new List<object>();
            for (int i = 0; i < data.Length; i++)
            {
                if (colors != null && colors.ContainsKey(i)) // Apply color if specified
                {
                    seriesData.Add(new
                    {
                        value = data[i],
                        itemStyle = new { color = colors[i] }
                    });
                }
                else
                {
                    seriesData.Add(data[i]); // Normal data without styling
                }
            }
            return seriesData;
        }
        #region Events
        private void OnEchartsEvent(EchartsEventArgs args)
        {
            if(Callback.HasDelegate)
            {
                Callback.InvokeAsync(args);
            }
        }
        #endregion
    }
}
