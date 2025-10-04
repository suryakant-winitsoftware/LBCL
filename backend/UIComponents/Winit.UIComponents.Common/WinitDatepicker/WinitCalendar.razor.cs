using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Nest;
using Winit.UIModels.Common;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Winit.UIComponents.Common.WinitDatepicker
{
    public partial class WinitCalendar
    {
      
        [Parameter]
        public string id { get; set; }

        [Parameter]
        public string selectedDate { get; set; }

        [Parameter]
        public string width { get; set; }

        [Parameter]
        public string height { get; set; }

        [Parameter]
        public string MinDate { get; set; }

        [Parameter]
        public string MaxDate { get; set; }

        [Parameter]
        public EventCallback<CalenderWrappedData> OnChange { get; set; }

        [Parameter]
        public bool IsDisabled { get; set; } = false;

        private static Action<string, string>? DateChangeAction;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
               // await JSRuntime.InvokeVoidAsync("GetDatepicker", id, MinDate, MaxDate);
                await JSRuntime.InvokeVoidAsync("GetDatepicker",
                 DotNetObjectReference.Create(this), nameof(OnDateChanged), id, MinDate, MaxDate);
            }
        }
        [JSInvokable]

        public async Task<string> OnDateChanged(string id, string selectedValue)
        {
            var args = new CalenderWrappedData
            {
                Id = id,
                SelectedValue = selectedValue
            };
            await OnChange.InvokeAsync(args);

            // Return the formatted date back to JavaScript
            return selectedDate; // You can modify this based on your logic
        }

    }
}
