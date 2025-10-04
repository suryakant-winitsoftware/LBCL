using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using WINITMobile.Data;

namespace WINITMobile.Pages
{
    public partial class Test : ComponentBase
    {
        public TestInt counter = new TestInt();
        private void CheckParent()
        {
            counter.CounterValue++;
            //StateHasChanged();
        }
        private void HandleCounterValueChanged()
        {

        }
    }
}
