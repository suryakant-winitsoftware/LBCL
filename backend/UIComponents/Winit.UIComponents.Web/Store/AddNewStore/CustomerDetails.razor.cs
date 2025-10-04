using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.UIModels.Web.Store;

namespace Winit.UIComponents.Web.Store.AddNewStore
{
    public partial class CustomerDetails : ComponentBase
    {
        [Parameter]public CustomerDetailsModel _CustomerDetails { get; set; }
        [Parameter]public EventCallback<CustomerDetailsModel> ONCustomerDetailsSaved { get; set; }
    }
}
