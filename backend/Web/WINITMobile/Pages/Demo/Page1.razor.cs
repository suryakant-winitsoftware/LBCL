using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace WINITMobile.Pages.Demo
{
    public partial class Page1 : ComponentBase, IDisposable
    {
        protected override async Task OnInitializedAsync()
        {
            Guid vk = scopedService.ScopeIdentifier;
        }
        public void Dispose()
        {
            if (scopedService is IDisposable disposableService)
            {
                disposableService.Dispose();
            }
        }
    }
}
