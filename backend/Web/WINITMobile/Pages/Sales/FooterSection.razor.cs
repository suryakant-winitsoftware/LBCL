using Microsoft.JSInterop;

namespace WINITMobile.Pages.Sales
{
    public partial class FooterSection
    {

        private bool ShowDetail = false;
        private void ToggleDetail()
        {
            ShowDetail = !ShowDetail;
        }
        protected override async Task OnInitializedAsync()
        {

            LoadResources(null, _languageService.SelectedCulture);

        }
        private async Task PreviewButtonCallback()
        {
            if (OnPreviewButtonClicked.HasDelegate)
            {
                await OnPreviewButtonClicked.InvokeAsync(null);
            }
        }

        private async Task PlaceOrderButtonCallback()
        {
            if (OnPlaceOrderButtonClicked.HasDelegate)
            {
                await OnPlaceOrderButtonClicked.InvokeAsync(null);
            }
        }
        private async Task DistrubutorButtonCallback()
        {
            if (OnDistributorClicked.HasDelegate)
            {
                await OnDistributorClicked.InvokeAsync(null);
            }
        }

        //method to handle the "Back" button.
        private void HandleBackbutton()
        {
            // Restore the scroll position before navigating back
            _ = JSRuntime.InvokeVoidAsync("restoreScrollPosition");
            //NavigationManager.NavigateTo("createsalesorder", forceLoad: false);
            NavigateTo("createsalesorder");
        }


    }
}
