using Microsoft.AspNetCore.Components;


namespace WINITMobile.Pages.Return
{
    partial class ReturnSummaryListView
    {

        [Parameter]
        public List<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnSummaryItemView> Data { get; set; } = new List<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnSummaryItemView>();
        [Parameter]
        public EventCallback<string> OnCardClick { get; set; }
        protected override async Task OnInitializedAsync()
        {
            // Instead of calling GetProducts, set the ProductList property with sample data
            //serviceModel.ProductList = SampleData.GetSampleProducts();
            LoadResources(null, _languageService.SelectedCulture);

        }
    }
}
