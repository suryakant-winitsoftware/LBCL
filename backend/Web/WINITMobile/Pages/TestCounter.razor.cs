using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using WINITMobile.Data;

namespace WINITMobile.Pages
{
    public partial class TestCounter : ComponentBase
    {
        [Parameter] public TestInt counter { get; set; }
        [Parameter] public EventCallback CounterValueChanged { get; set; }
        private void IncrementCounter()
        {
            counter.CounterValue++;
            CounterValueChanged.InvokeAsync();
        }
    }
}
