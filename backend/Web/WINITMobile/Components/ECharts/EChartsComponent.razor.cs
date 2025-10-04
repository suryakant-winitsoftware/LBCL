using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace WINITMobile.Components.ECharts
{
    public partial class EChartsComponent : ComponentBase, IAsyncDisposable
    {
        private ElementReference _chartElement;
        private IJSObjectReference _chartInstance;
        private DotNetObjectReference<EChartsComponent> _objRef;

        [Parameter]
        public object Options { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _objRef = DotNetObjectReference.Create(this);
                await InitializeChart();
            }
            else
            {
                await UpdateChart();
            }
        }

        private async Task InitializeChart()
        {
            try
            {
                // Initialize ECharts with the element reference
                _chartInstance = await JSRuntime.InvokeAsync<IJSObjectReference>("echartsInterop.init", _chartElement);
                
                // Set the options
                await UpdateChart();

                // Add resize handler
                await JSRuntime.InvokeVoidAsync("window.addEventListener", "resize", async () =>
                {
                    if (_chartInstance != null)
                    {
                        await JSRuntime.InvokeVoidAsync("echartsInterop.resize", _chartInstance);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing chart: {ex.Message}");
            }
        }

        private async Task UpdateChart()
        {
            if (_chartInstance != null)
            {
                try
                {
                    await JSRuntime.InvokeVoidAsync("echartsInterop.setOption", _chartInstance, Options);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating chart: {ex.Message}");
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_chartInstance != null)
            {
                try
                {
                    await JSRuntime.InvokeVoidAsync("echartsInterop.dispose", _chartInstance);
                    await _chartInstance.DisposeAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error disposing chart: {ex.Message}");
                }
            }

            _objRef?.Dispose();
        }
    }
} 