using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Resources;
using Winit.Shared.CommonUtilities.Common;
using Winit.UIComponents.Common.Language;
using WinIt.BreadCrum.Classes;


namespace WinIt.Pages.SalesManagement.Distributor
{
    public partial class DistributorAdmin
    {
      
        protected async override Task OnInitializedAsync()
        {
            LoadResources(null, _languageService.SelectedCulture);

            await _distributorAdmin.PopulateViewModel();
           await SetHeaderName();
        }
        [CascadingParameter]
        public EventCallback<WinIt.BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public async Task SetHeaderName()
        {
            _IDataService.BreadcrumList = new();
            _IDataService.HeaderText = @Localizer["distributor_admin"];
            _IDataService.BreadcrumList.Add(new BreadCrumModel() { SlNo = 1, Text = @Localizer["maintain_distributor"], IsClickable = true, URL = "maintaindistributor" });
            _IDataService.BreadcrumList.Add(new BreadCrumModel() { SlNo = 2, Text = @Localizer["distributor_admin"], IsClickable = false, URL = "maintaindistributor" });
            await CallbackService.InvokeAsync(_IDataService);
        }
       
    }
}
