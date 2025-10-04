using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.UIModels.Web.SKU;

namespace Winit.UIComponents.Web.SalesManagement.PriceManagement
{
    public partial class Details : ComponentBase
    {
        [Parameter]
        public SKUPriceGroup SKUPriceGroup { get; set; }
    }
}
