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
using Winit.Shared.Models.Models.ReportController;
using Microsoft.JSInterop;

namespace Winit.UIComponents.Common.WINITCharts
{
    public partial class WChartWithNegative : ComponentBase
    {
        private EBar? chart;
        private EChartsOption<L.Bar>? ChartOption;
        private List<EventType> EventTypes = new List<EventType> { EventType.click, EventType.brush, EventType.brushselected };
        [Parameter]
        public EventCallback<EchartsEventArgs> Callback { get; set; }
        [Parameter] public string Title { get; set; } = "";
        [Parameter]
        public List<string> YAxisLabels { get; set; } = new();
        [Parameter]
        public List<ChartData> Data { get; set; } = new();
        [Parameter] public SeriesConfig? SeriesConfig { get; set; }

        protected override async void OnInitialized()
        {
            ChartOption = new()
            {
                Title = new() { Text = Title },
                Tooltip = new()
                {
                    Trigger = TooltipTrigger.Axis,
                    AxisPointer = new() { Type = AxisPointerType.Shadow }
                },
                //Grid = new List<Grid> { new() {Show=true, Top = 80, Bottom = 30, Height = 500 } },
                Grid = new List<Grid> { new() { Left = "20%",
                    Right = "15%",
                    Top = "15%",
                    Bottom = "10%",
                    ContainLabel = true,
                    Height = Math.Max(300, YAxisLabels.Count * 20)   } },


                XAxis = new()
                {
                    new()
                    {
                        Type = AxisType.Value,
                        Position = PositionX.Top,
                        SplitLine = new()
                        {
                            LineStyle = new() { Type = LineStyleType.Dashed }
                        },

                    }
                },
                YAxis = new()
                {
                    new()
                    {
                        Type = AxisType.Category,
                        Data = YAxisLabels,
                        AxisLine = new() { Show = true },
                        AxisLabel = new() { Show = true },
                        AxisTick = new() { Show = true },
                        SplitLine = new() { Show = true }
                    }
                },
                Series = new List<object>()
                {
                    new
                    {
                        Name = SeriesConfig?.Name,
                        Type = SeriesConfig?.Type,
                        Stack = "Total",
                        Label = SeriesConfig?.Label?? new LabelOptions{ Show = true},
                        Data = Data.Select(data => new
                        {
                            Value = data.Value,
                            Label = data.Label ?? new LabelOptions(),
                            ItemStyle = data.ItemStyle ?? new ItemStyleOptions()
                        }).ToList()
                    }
                },
                //DataZoom = new()
                //{
                //    new
                //    {
                //         Type= "slider",  
                //        YAxisIndex= 0,Orient = "vertical",
                //        Start= 0,      
                //        End= 50,       
                //        Show= true,ZoomLock = false, 
                //        //MinSpan = 10,    
                //        //MaxSpan = 100
                //        FilterMode = "filter",Realtime = true,
                //    }
                //}
            };
           

        }
        


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && chart is not null)
            {
                await chart.SetupOptionAsync(ChartOption);
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
