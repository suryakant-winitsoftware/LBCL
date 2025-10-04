using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WINITMobile.Pages.Base;
using System.Resources;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Mobile;
namespace WINITMobile.Pages.Return;

partial class ReturnSummary
{
    private string SearchString = "";
    private bool IsInitialized { get; set; }
    protected override async Task OnInitializedAsync()
    {
        _backbuttonhandler.ClearCurrentPage();
        await _ReturnSummeryViewModel.PopulateViewModel();
        IsInitialized = true;
        LoadResources(null, _languageService.SelectedCulture);

    }
    protected void LoadResources(object sender, string culture)
    {
        CultureInfo cultureInfo = new CultureInfo(culture);
        ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys).Assembly);
        Localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
    }
    private void OnDateChange()
    {
        ActiveTabChanged(_ReturnSummeryViewModel?.TabList[0]);
    }

    private void OnSearching(string value)
    {
        SearchString = value;
        _ReturnSummeryViewModel.ApplySearch(value);
    }

    public void CardClicked(string orderUID)
    {
        _navigationManager.NavigateTo($"returnorderdetails/{orderUID}");
    }

    private void ActiveTabChanged(Winit.Shared.Models.Common.ISelectionItem selectedTab)
    {
        if(!selectedTab.IsSelected)
        {
            _ReturnSummeryViewModel.OnTabSelected(selectedTab);
            if (!string.IsNullOrEmpty(SearchString))
            {
                _ReturnSummeryViewModel.ApplySearch(SearchString);
            }
            StateHasChanged(); // Ensure the component updates to reflect the changes
        }
    }
}

