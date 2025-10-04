using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Globalization;
using System.Reflection;
using System.Resources;
using Winit.Modules.Role.Model.Classes;
using Winit.Modules.Role.Model.Interfaces;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;
using WinIt.Pages.Base;

namespace WinIt.Shared
{
    public partial class MenuNavigation : ComponentBase
    {
        [CascadingParameter]
        public required Task<AuthenticationState> AuthenticationState { get; set; }
        public bool IsMenuNeeded { get; set; } = true;

        protected override void OnInitialized()
        {
            IsMenuNeeded = _navigationManager.Uri.Contains("selfRegistration");
            LoadResources(null, _languageService.SelectedCulture);
            if (!IsMenuNeeded)
            {
                _menuHierarchy.OnMenuAddigned += RefreshMenu;
            }
            if (_menuHierarchy.ModuleMasterHierarchies is not null)
            {
                var selectedMenu = _menuHierarchy.ModuleMasterHierarchies.
                    Find(p => p.SubModuleHierarchies.Any(q => q.SubSubModules.Any(r => _navigationManager.Uri.Contains(r.RelativePath))));
                if (selectedMenu != null)
                {
                    OnMenuClick(selectedMenu);
                }
            }
        }
        protected void RefreshMenu(Winit.Modules.Role.Model.Interfaces.IMenuMasterHierarchyView menuHierarchies)
        {
            _menuHierarchy = menuHierarchies;
            _menuHierarchy.OnMenuAddigned -= RefreshMenu;
            StateHasChanged();
        }


        private async Task OnLogOutClick()
        {
            if (await _alertService.ShowConfirmationReturnType(@Localizer["logout"], @Localizer["are_you_sure_you_want_to_logout?"], @Localizer["yes"], @Localizer["no"]))
            {
                await _localStorageService.RemoveItem("token");
                await _authStateProvider.GetAuthenticationStateAsync();
                _navigationManager.NavigateToLogout(_navigationManager.BaseUri);
            }
        }

        protected void LoadResources(object sender, string culture)
        {
            CultureInfo cultureInfo = new CultureInfo(culture);
            ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys).Assembly);
            Localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
        }
        protected void OnMenuClick(MenuHierarchy selectedModule)
        {
            if (selectedModule == null || !selectedModule.IsClicked)
            {
                foreach (var module in _menuHierarchy.ModuleMasterHierarchies)
                {
                    if (module == selectedModule)
                    {
                        module.IsClicked = true;
                    }
                    else
                    {
                        module.IsClicked = false;
                    }
                }
            }
        }
    }
}
