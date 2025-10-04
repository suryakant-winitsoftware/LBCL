using Microsoft.AspNetCore.Components;

namespace WinIt.Pages.SKUClassGroupItems;
partial class SKUClassGroupItemsGrid
{
    [Parameter]
    public List<Winit.Modules.SKUClass.Model.UIInterfaces.ISKUClassGroupItemView>? SKUClassGroupItemViews { get; set; }
    [Parameter]
    public EventCallback<Winit.Modules.SKUClass.Model.UIInterfaces.ISKUClassGroupItemView> OnPlantDDClick { get; set; }
    protected override async Task OnInitializedAsync()
    {
        LoadResources(null, _languageService.SelectedCulture);
    }
}
