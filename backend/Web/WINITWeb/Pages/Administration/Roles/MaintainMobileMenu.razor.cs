using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Resources;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;
using WinIt.BreadCrum.Classes;
using WinIt.BreadCrum.Interfaces;


namespace WinIt.Pages.Administration.Roles
{
    public partial class MaintainMobileMenu
    {
        protected override async Task OnInitializedAsync()
        {
            LoadResources(null, _languageService.SelectedCulture);
            await SetHeaderName();
           await _mobileMenu.PopulateViewModel();
        }
        [CascadingParameter]
        public EventCallback<WinIt.BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public async Task SetHeaderName()
        {
            _IDataService.BreadcrumList = new List<IBreadCrum>();
            _IDataService.HeaderText = @Localizer["maintain_user_roles"];
            _IDataService.BreadcrumList.Add(new BreadCrumModel() { SlNo = 1, Text = @Localizer["maintain_roles"], IsClickable = true, URL = "maintainUserRole" });
            _IDataService.BreadcrumList.Add(new BreadCrumModel() { SlNo = 1, Text = @Localizer["manage_web_menu"] });
            await CallbackService.InvokeAsync(_IDataService);
        }
       
    }
}
