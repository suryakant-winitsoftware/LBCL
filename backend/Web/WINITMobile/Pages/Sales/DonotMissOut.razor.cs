using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;

namespace WINITMobile.Pages.Sales;

partial class DonotMissOut
{
    [Parameter]
    public EventCallback OnCloseClick { get; set; }

    [Parameter]
    public EventCallback OnSkipClick { get; set; }

    [Parameter]
    public List<ISelectionItem> Headers{ get; set; }
    [Parameter]
    public Dictionary<string, List<ISelectionItem>> HeaderDetails { get; set; }

    public void Handle_ProceedClick()
    {
        OnCloseClick.InvokeAsync();
    }
    protected override async Task OnInitializedAsync()
    {
        LoadResources(null, _languageService.SelectedCulture);
    }
}
