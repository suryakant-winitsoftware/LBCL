using ClosedXML.Excel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Auth.BL.Interfaces;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Role.BL.Interfaces;
using Winit.Modules.Role.Model.Classes;
using Winit.Modules.Role.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common.Common.LocalStorage;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;
using Winit.UIModels.Common.Filter;
using WinIt.BreadCrum.Classes;
using WinIt.BreadCrum.Interfaces;
using WinIt.Shared;


namespace WinIt.Pages.Base
{
    public class BaseComponentBase : ComponentBase, IDisposable
    {
        [Inject] NavigationManager? _navigationManager { get; set; }
        [Inject] protected IDataManager? _dataManager { get; set; }
        [Inject] protected IAppUser? _appUser { get; set; }
        [Inject] private Winit.UIComponents.Common.Services.ILoadingService? _loadingService { get; set; }
        [Inject] private Winit.UIComponents.SnackBar.IToast? _tost { get; set; }
        [Inject] Winit.Modules.Base.BL.ILocalStorageService? _localStorage { get; set; }
        [Inject] protected IStringLocalizer<LanguageKeys>? Localizer { get; set; }
        [Inject] protected IDataService? _iDataService { get; set; }
        [Inject]
        protected AuthenticationStateProvider? _authStateProvider { get; set; }
        [Inject]
        protected Winit.Modules.Auth.BL.Interfaces.ILoginViewModel? _loginViewModel { get; set; }
        [Inject]
        protected Winit.Modules.Base.BL.ILocalStorageService? _localStorageService { get; set; }

        [Inject]
        protected IPermissionDataHelper? _permissionDataHelper { get; set; }
        [CascadingParameter]
        public required Task<AuthenticationState> AuthenticationState { get; set; }
        protected List<DataGridColumn> DataGridColumns { get; set; } = [];
        protected List<FilterModel> ColumnsForFilter = [];
        protected Winit.UIModels.Web.Breadcrum.Interfaces.IDataService BreadCrumDTO { get; private set; } = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel();

        protected IPermission? permission { get; set; }

        [Inject]
        IJSRuntime _jsRuntime { get; set; }

        [Inject]
        MemoryCleanupService CleanupService { get; set; }
        string UnAuthorizedRole = "UnAuthorizedRole";


        [Inject]
        protected PageStateHandler _pageStateHandler { get; set; }
        protected string HasViewAccess
        {
            get
            {
                return (permission != null &&
                    (CommonFunctions.GetBooleanValue(permission.FullAccess!)) || permission!.ViewAccess) ?
                    _appUser.Role.Code : UnAuthorizedRole;
            }
        }
        protected string HasFullAccess
        {
            get
            {
                return (permission != null &&
                    CommonFunctions.GetBooleanValue(permission.FullAccess!)) ?
                    _appUser.Role.Code : UnAuthorizedRole;
            }
        }
        protected string HasAddAccess
        {
            get
            {
                return (permission != null &&
                    (CommonFunctions.GetBooleanValue(permission.FullAccess!)) || permission!.AddAccess) ?
                    _appUser.Role.Code : UnAuthorizedRole;
            }
        }
        protected string HasEditAccess
        {
            get
            {
                return (permission != null &&
                   (CommonFunctions.GetBooleanValue(permission.FullAccess!)) || permission!.EditAccess) ?
                    _appUser.Role.Code : UnAuthorizedRole;
            }
        }
        protected string HasDeleteAccess
        {
            get
            {
                return (permission != null &&
                   (CommonFunctions.GetBooleanValue(permission.FullAccess!)) || permission!.EditAccess) ?
                    _appUser.Role.Code : UnAuthorizedRole;
            }
        }
        protected string HasDownloadAccess
        {
            get
            {
                return (permission != null &&
                    (CommonFunctions.GetBooleanValue(permission.FullAccess!)) || permission!.DownloadAccess) ?
                    _appUser.Role.Code : UnAuthorizedRole;
            }
        }
        protected string HasApprovalAccess
        {
            get
            {
                return (permission != null &&
                   (CommonFunctions.GetBooleanValue(permission.FullAccess!)) || permission.ApprovalAccess) ?
                    _appUser.Role.Code : UnAuthorizedRole;
            }

        }
        protected override void OnInitialized()
        {
            permission = new Permission();
        }

        protected override async Task OnInitializedAsync()
        {
            await GetPermissionOfCurrentPageByRole();
            await base.OnInitializedAsync();
        }

        public void Dispose()
        {
            Console.WriteLine($"{this.GetType().Name} disposed");
            CleanupService.OnCleanupRequested -= Dispose;
        }
        protected async Task GetPermissionOfCurrentPageByRole(string routeName = null)
        {
            try
            {

                if (string.IsNullOrEmpty(routeName))
                {
                    var uri = _navigationManager.Uri;
                    var path = new Uri(uri).AbsolutePath;
                    routeName = path.Split('/').LastOrDefault();
                }
                permission = await _permissionDataHelper.GetPermissionByPage(_appUser.Role.UID, _appUser.Role.IsPrincipalRole, routeName);
            }
            catch (Exception ex) { }
        }
        //[CascadingParameter]
        //public EventCallback<WinIt.BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public async Task SetBreadCrum(string HeaderText, List<IBreadCrum> BreadcrumList)
        {
            _iDataService.BreadcrumList = new List<IBreadCrum>();
            _iDataService.HeaderText = HeaderText;
            _iDataService.BreadcrumList = BreadcrumList;
            //await CallbackService.InvokeAsync(_iDataService);
        }
        protected void LoadResources(object sender, string culture)
        {
            CultureInfo cultureInfo = new CultureInfo(culture);
            ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Web.Languagekeys", typeof(Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys).Assembly);
            Localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
        }
        private async Task<bool> IsFirstLoad_LocalStorage()
        {
            try
            {
                if (_localStorage == null)
                {
                    return false; // or throw an exception, depending on your error handling strategy
                }
                // Check if a flag indicating the first load exists in LocalStorage or SessionStorage
                // If the flag doesn't exist, it means it's the first load
                bool? isAppLoaded = await _localStorage.GetItem<bool?>("IS_APP_LOADED");
                if (!isAppLoaded.HasValue || !isAppLoaded.Value)
                {
                    // Set the flag indicating the first load
                    await _localStorage.SetItem("IS_APP_LOADED", true);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private bool IsFirstLoad()
        {
            try
            {
                if (_dataManager == null)
                {
                    return false; // or throw an exception, depending on your error handling strategy
                }
                // Check if a flag indicating the first load exists in LocalStorage or SessionStorage
                // If the flag doesn't exist, it means it's the first load
                bool? isAppLoaded = (bool?)_dataManager.GetData("IS_APP_LOADED");
                if (!isAppLoaded.HasValue || !isAppLoaded.Value)
                {
                    // Set the flag indicating the first load
                    _dataManager.SetData("IS_APP_LOADED", true);
                    return true;
                }


                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        protected void Navigate(string navigationURL, string queryString, object data)
        {
            if (data != null)
            {
                _dataManager?.SetData(navigationURL, data);
            }
            _navigationManager?.NavigateTo($"{navigationURL}{queryString}");
        }

        //srinadh
        protected void ShowLoader(string message = "")
        {
            InvokeAsync(() =>
            _loadingService?.ShowLoading(message));
        }
        protected void HideLoader()
        {
            InvokeAsync(() =>
            _loadingService?.HideLoading());
        }
        protected void ShowSuccessSnackBar(string? header = null, string? body = null)
        {
            _tost?.Add(header ?? string.Empty, body ?? string.Empty, Winit.UIComponents.SnackBar.Enum.Severity.Success);
        }
        protected void ShowErrorSnackBar(string? header = null, string? body = null)
        {
            _tost?.Add(header ?? string.Empty, body ?? string.Empty, Winit.UIComponents.SnackBar.Enum.Severity.Error);
        }
        public async Task ExportToExcelAsync<T>(List<T> data, Dictionary<string, string> columnMappings, string fileName)
        {
            if (data == null || !data.Any())
            {
                throw new ArgumentException("Data is empty or null.");
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Sheet1");

                // Get properties of the object dynamically
                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                          .ToDictionary(p => p.Name, p => p);

                // Add headers based on provided column mappings
                int colIndex = 1;
                foreach (var column in columnMappings)
                {
                    worksheet.Cell(1, colIndex).Value = column.Value; // Excel column name
                    colIndex++;
                }

                // Populate data rows
                for (int rowIndex = 0; rowIndex < data.Count; rowIndex++)
                {
                    colIndex = 1;
                    foreach (var column in columnMappings)
                    {
                        if (properties.TryGetValue(column.Key, out var prop))
                        {
                            var value = prop.GetValue(data[rowIndex]);
                            worksheet.Cell(rowIndex + 2, colIndex).Value = value?.ToString() ?? "";
                        }
                        colIndex++;
                    }
                }

                // Convert to base64 for Blazor download
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var bytes = stream.ToArray();
                    string base64 = Convert.ToBase64String(bytes);

                    // Trigger download in Blazor using JS Interop
                    await _jsRuntime.InvokeVoidAsync("eval", $"var a = document.createElement('a'); a.href = 'data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64,{base64}'; a.download = '{fileName}.xlsx'; a.click();");
                }
            }
        }
    }
}
