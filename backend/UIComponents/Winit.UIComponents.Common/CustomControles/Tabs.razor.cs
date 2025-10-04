using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Winit.UIComponents.Common.CustomControles;

partial class Tabs
{
    [Parameter]
    public List<Winit.Shared.Models.Common.ISelectionItem> TabItems { get; set; }

    [Parameter]
    public bool IsVertical { get; set; }
    [Parameter]
    public bool IsCountRequired { get; set; }

    [Parameter]
    public EventCallback<Winit.Shared.Models.Common.ISelectionItem> OnTabSelect { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await Task.Run(() => base.OnInitialized());
    }

    public void Refresh()
    {
        StateHasChanged();
    }
}

