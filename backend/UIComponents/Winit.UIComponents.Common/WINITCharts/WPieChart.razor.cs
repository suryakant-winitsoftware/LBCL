using System;
using System.Collections.Generic;
using System.Linq;
using Blazor.ECharts.Components;
using Blazor.ECharts.Options;
using Microsoft.AspNetCore.Components;
using Blazor.ECharts.Options.Enum;
using L = Blazor.ECharts.Options.Series.Pie;
using Blazor.ECharts.Options.Series;
using Nest;
using System.Runtime.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Winit.Shared.Models.Models.ReportController;

namespace Winit.UIComponents.Common.WINITCharts
{
    public partial class WPieChart : ComponentBase
    {
        private EPie? chart = new EPie();
        private EChartsOption<L.Pie>? ChartOption;
        [Parameter]
        public EventCallback<EchartsEventArgs> Callback { get; set; }
        private List<EventType> EventTypes = new() { EventType.click, EventType.brush, EventType.brushselected };
        private string theme = "light";

        [Parameter] public string? Title { get; set; } = "";
        [Parameter] public string? SubText { get; set; } = "";
        [Parameter] public string? DataHeaderText { get; set; } = "";
        [Parameter] public Legend Legend { get; set; } = new() { Orient = Orient.Vertical, Left = "left" };
        [Parameter]
        public List<PieDataItem> Data { get; set; } = new();

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Legend.Width = 200;
            ChartOption = new()
            {
                Title = new() { Text = Title, Subtext = SubText, Left = "center" },
                Tooltip = new()
                {

                    Trigger = TooltipTrigger.Item,
                    //Formatter = new JFunc(@"function(params) {
                    //            let value = params.value;
                    //            if (value >= 1000000000) return params.name + ': ' + (value / 1000000000).toFixed(1) + 'B';
                    //            if (value >= 1000000) return params.name + ': ' + (value / 1000000).toFixed(1) + 'M';
                    //            if (value >= 1000) return params.name + ': ' + (value / 1000).toFixed(1) + 'K';
                    //            return params.name + ': ' + value;
                    //        }")
                },
                Legend = Legend,
                Series = new List<object>
                {
                    new
                    {
                        Name = DataHeaderText,
                        Type = "pie",
                        Radius = "60%",
                        Data = Data,
                        Label= new
                            {
                                Show = true,
                                Position = "outside",
                                Formatter = new JFunc(@"function(params) {
                                     let value = params.value;
                                if (value >= 1000000000) return params.name + '- ' + (value / 1000000000).toFixed(value % 1000000000==0?0:2) + 'B';
                                if (value >= 1000000) return params.name + '- ' + (value / 1000000).toFixed(value % 1000000==0?0:2) + 'M';
                                if (value >= 1000) return params.name + '- ' + (value / 1000).toFixed(value % 1000==0?0:2) + 'K';
                                return params.name + '- ' + value;
                                }")
                            },
                        Emphasis = new Emphasis
                        {
                            ItemStyle = new ItemStyle
                            {
                                ShadowBlur = 10,
                                ShadowOffsetX = 0,
                                ShadowColor = "rgba(0, 0, 0, 0.5)"
                            }
                        }
                    }
                }
            };
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await Task.Delay(100);
            if (firstRender && chart is not null)
            {
                await chart.SetupOptionAsync(ChartOption);
                StateHasChanged();
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
