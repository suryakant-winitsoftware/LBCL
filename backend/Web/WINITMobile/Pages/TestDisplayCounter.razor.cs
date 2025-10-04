using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using WINITMobile.Data;

namespace WINITMobile.Pages
{
    public partial class TestDisplayCounter : ComponentBase
    {
        [Parameter] public TestInt counter { get; set; }
    }
}
