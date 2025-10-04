using Microsoft.AspNetCore.Components;
using Winit.Shared.Models.Common;

namespace Winit.UIComponents.Mobile.Common;

public partial class MobileTabs : ComponentBase
{
    [Parameter]
    public required List<ISelectionItem> TabItems { get; set; }

    [Parameter]
    public bool IsVertical { get; set; }

    [Parameter]
    public EventCallback<Winit.Shared.Models.Common.ISelectionItem> OnTabSelect { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await Task.Run(base.OnInitialized);
    }

    public void Refresh()
    {
        StateHasChanged();
    }
}
