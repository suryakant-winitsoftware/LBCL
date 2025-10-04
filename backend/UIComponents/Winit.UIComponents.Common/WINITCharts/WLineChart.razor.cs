using System;
using System.Collections.Generic;
using System.Linq;
using Blazor.ECharts.Components;
using Blazor.ECharts.Options;
using Microsoft.AspNetCore.Components;
using Blazor.ECharts.Options.Enum;
using L = Blazor.ECharts.Options.Series.Line;

namespace Winit.UIComponents.Common.WINITCharts
{
    public partial class WLineChart : ComponentBase
    {
        private ELine? chart = new ELine();
        private EChartsOption<L.Line>? ChartOption;
        private string theme = "light";
        [Parameter]
        public EventCallback<EchartsEventArgs> Callback { get; set; }
        [Parameter] public string Title { get; set; } = "Line Chart";
        [Parameter] public string[] XAxisLabels { get; set; } = Array.Empty<string>();  // Make dynamic
        [Parameter] public Dictionary<string, int[]> SeriesData { get; set; } = new();
        [Parameter] public bool Stacked { get; set; } = false;
        [Parameter] public bool HasArea { get; set; } = false;
        [Parameter] public bool Smooth { get; set; } = false;

        //Added Configurable Tooltip Options
        [Parameter] public TooltipTrigger TooltipTrigger { get; set; } = TooltipTrigger.Axis;  // Enum
        [Parameter] public AxisPointerType AxisPointerType { get; set; } = AxisPointerType.Line; // Enum
        [Parameter] public string BackgroundColor { get; set; } = "#ffffff";
        [Parameter] public string BorderColor { get; set; } = "#ccc";
        [Parameter] public string Formatter { get; set; } = ""; // Custom formatter if needed

        protected override void OnInitialized()
        {
            base.OnInitialized();
            ChartOption = new()
            {
                Title = new() { Text = Title },
                Tooltip = new()
                {
                    Trigger = TooltipTrigger,
                    AxisPointer = new() { Type = AxisPointerType },
                    BackgroundColor = BackgroundColor,
                    BorderColor = BorderColor,
                    Formatter = !string.IsNullOrWhiteSpace(Formatter) ? Formatter : null
                },
                Legend = new() { Data = SeriesData.Keys.ToArray() },
                Grid = new() { new() { Left = "3%", Right = "4%", Bottom = "3%", ContainLabel = true } },
                XAxis = new() { new() { Type = AxisType.Category, Data = XAxisLabels, BoundaryGap = false } },
                YAxis = new() { new() { Type = AxisType.Value } },
                Series = SeriesData.Select(data => new L.Line()
                {
                    Name = data.Key,
                    Type = "line",
                    Data = data.Value,
                    Stack = Stacked ? "Total" : null,
                    AreaStyle = HasArea ? new AreaStyle() : null,
                    Smooth = Smooth
                }).ToList<object>()
            };
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && chart is not null)
            {
                await chart.SetupOptionAsync(ChartOption);
                StateHasChanged(); // Ensure Blazor re-renders properly
            }
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
