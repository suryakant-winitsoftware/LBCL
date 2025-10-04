using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Newtonsoft.Json;
using System.Globalization;
using System.Resources;
using Winit.Modules.ApprovalEngine.Model.Constants;
using Winit.Modules.Base.Model.Constants;
using Winit.Modules.ErrorHandling.Model.Classes;
using Winit.Modules.ErrorHandling.Model.Interfaces;
using Winit.Modules.Role.Model.Classes;
using Winit.Modules.Role.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;
using WinIt.Pages.Base;

namespace WinIt.Pages;

public partial class Index : BaseComponentBase
{
    [CascadingParameter] public EventCallback<IMenuMasterHierarchyView> ModuleHierarchyCallaBack { get; set; }

    [CascadingParameter] public EventCallback<WinIt.BreadCrum.Interfaces.IDataService> CallbackService { get; set; }

    private string UserID { get; set; } = string.Empty;
    private string Password { get; set; } = string.Empty;
    private bool ShowPassword { get; set; }
    private string ErrorMsg { get; set; } = string.Empty;
    private bool RememberMe { get; set; } = false;
    private string SelectedLanguage { get; set; } = "";
    public bool IsShowLangPopUp { get; set; } = true;
    private string SelectedCulture { get; set; } = "en-GB";
    private ElementReference PassWordElement;
    private bool IsForgotPasswordPopUpOpen { get; set; }
    private IModulesMasterView? masterView;
    private readonly Winit.Modules.Role.Model.Interfaces.IMenuMasterHierarchyView? _modulesMasterHierarchy;

    protected override void OnInitialized()
    {
        SetCulture(SelectedCulture);
    }

    protected override async Task OnInitializedAsync()
    {
        string? savedCulture = await _localStorageService.GetItem<string>("RememberedCulture");
        if (!string.IsNullOrEmpty(savedCulture))
        {
            _languageService.SelectedCulture = savedCulture;
        }
        else
        {
            _languageService.SelectedCulture = "en-GB";
        }

        LoadResources(null, _languageService.SelectedCulture);
        _languageService.OnLanguageChanged += LoadResources;
        _ = await _authStateProvider.GetAuthenticationStateAsync();
        System.Security.Claims.ClaimsPrincipal user = (await AuthenticationState).User;
        if (user.Identity.IsAuthenticated)
        {
            // _navigationManager.NavigateTo("dashboard");
        }
        else
        {
            await _localStorageService.RemoveItem(LocalStorageKeys.Token);
        }

        RememberMe = await _localStorageService.GetItem<bool>("RememberMe");
        if (RememberMe)
        {
            UserID = await _localStorageService.GetItem<string>("RememberedUsername");
            Password = await _localStorageService.GetItem<string>("RememberedPassword");
            var expirationDate = await _localStorageService.GetItem<DateTime>("ExpirationDate");

            if (expirationDate <= DateTime.Now)
            {
                // Remove expired credentials
                await _localStorageService.RemoveItem("RememberedUsername");
                await _localStorageService.RemoveItem("RememberedPassword");
                await _localStorageService.RemoveItem("RememberMe");
                await _localStorageService.RemoveItem("ExpirationDate");
            }
        }
        // base.OnInitialized();
    }

    private void LoadResources(object sender, string culture)
    {
        CultureInfo cultureInfo = CultureInfo.GetCultureInfo(culture);
        //ResourceManager resourceManager = new ResourceManager("WinIt.LanguageResources.IndexPage.index", typeof(Index).Assembly);
        //Localizer = new CustomStringLocalizer<Index>(resourceManager, cultureInfo);
        ResourceManager resourceManager =
            new ResourceManager("Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys",
                typeof(Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys).Assembly);
        Localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
    }


    private void ChangeLanguage(ChangeEventArgs e)
    {
        var newCulture = e.Value.ToString();
        _languageService.SelectedCulture = newCulture;
        SetCulture(newCulture);
        _localStorageService.SetItem("RememberedCulture", newCulture);

        StateHasChanged();
    }

    private void SetCulture(string culture)
    {
        CultureInfo cultureInfo = new CultureInfo(culture);
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        LoadResources(null, culture);
    }

    private async Task KeyHandler(KeyboardEventArgs e, string inputBox = "Password")
    {
        if (e.Key == "Enter")
        {
            if (inputBox == "UserId")
            {
                await PassWordElement.FocusAsync();
            }
            else
            {
                await OnLoginClick();
            }
        }
    }

    /*
    public string GenerateChallengeCode()
    {
        return DateTime.UtcNow.ToString("yyyyMMddHHmmss");
    }
    public string EncryptWithChallenge(string text, string challenge)
    {
        string passwordWithChallenge = text + challenge;
        return HashWithSHA256(passwordWithChallenge);
    }
    private string HashWithSHA256(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(hashBytes);
        }
    }
    */
    public async Task OnLoginClick()
    {
        try
        {
            await _localStorageService.SetItem("Username", UserID);
            if (RememberMe)
            {
                await _localStorageService.SetItem("RememberedUsername", UserID);
                await _localStorageService.SetItem("RememberedPassword", Password);
                await _localStorageService.SetItem("RememberMe", true);
                var expirationDate = DateTime.Now.AddDays(30);
                await _localStorageService.SetItem("ExpirationDate", expirationDate);
            }
            else
            {
                await _localStorageService.RemoveItem("RememberedUsername");
                await _localStorageService.RemoveItem("RememberedPassword");
                await _localStorageService.RemoveItem("RememberMe");
                await _localStorageService.RemoveItem("ExpirationDate");
            }

            if (string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(UserID))
            {
                ShowErrorSnackBar("Error", "Missing fields");
                return;
            }
            else
            {
                ShowLoader();
                UserID = UserID.Trim();
                string challengeCode = _sHACommonFunctions.GenerateChallengeCode();
                string tempPass = Password;
                Password = _sHACommonFunctions.EncryptPasswordWithChallenge(Password.Trim(), challengeCode);
                Winit.Modules.Auth.Model.Interfaces.ILoginResponse? tokenData =
                    await _loginViewModel.GetTokenByLoginCredentials(UserID, Password, challengeCode, null);
                if (tokenData != null)
                {
                    if (tokenData.AuthMaster != null && tokenData.StatusCode == 200)
                    {
                        await _localStorageService.SetItem(LocalStorageKeys.Token, tokenData.Token);
                        _ = await _authStateProvider.GetAuthenticationStateAsync();
                        await Task.WhenAll(_userMasterDataViewModel.GetUserMasterData(UserID),
                         _commonMasterData.PageLoadevents());

                        if (_appUser?.Emp == null)
                        {
                            ShowErrorSnackBar("Alert", "User data not found");
                        }
                        else if (_appUser.Emp.ApprovalStatus == ApprovalConst.Approved)
                        {
                            ShowSuccessSnackBar("Success", "User login successful");
                            _navigationManager.NavigateTo("landing");
                        }
                        else
                        {
                            ShowErrorSnackBar("Alert", "User is not active.");
                        }
                    }
                    else
                    {
                        Password = tempPass;
                        if (tokenData.StatusCode == 403)
                        {
                            ShowErrorSnackBar("Login failed",
                                tokenData.ErrorMessage.Equals("Forbidden") ? "Login failed please contact winit" : tokenData.ErrorMessage);
                        }
                        else if (tokenData.StatusCode == 503)
                        {
                            ShowErrorSnackBar("Login failed", tokenData.ErrorMessage);
                        }
                        else if (tokenData.StatusCode == 401)
                        {
                            ShowErrorSnackBar("Login failed", tokenData.ErrorMessage);
                        }
                        else if (tokenData.StatusCode == 0)
                        {
                            ShowErrorSnackBar("Login failed", "Backend Server down");
                        }
                    }
                }
                else
                {
                    Password = tempPass;
                    ShowErrorSnackBar("Login failed", "Invalid username or password");
                }

                HideLoader();
            }
        }
        catch (Exception ex)
        {
            ShowErrorSnackBar(@Localizer["error"], ex.Message);
            HideLoader();
        }
    }

    public async Task SetHeaderName()
    {
        _IDataService.BreadcrumList = new()
        {
            new BreadCrum.Classes.BreadCrumModel()
                { SlNo = 1, Text = "Maintain Customer", IsClickable = true, URL = "ManageCustomers" },
            new BreadCrum.Classes.BreadCrumModel()
                { SlNo = 2, Text = "Add/Edit Customer Price ", IsClickable = false, URL = "Store" }
        };
        _IDataService.HeaderText = "Add/Edit Customer Price ";
        await CallbackService.InvokeAsync(_IDataService);
    }

    private async Task LoadSettingMaster()
    {
        PagingRequest pagingRequest = new();

        ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}Setting/SelectAllSettingDetails",
            HttpMethod.Post, pagingRequest);

        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
        {
            string data =
                new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
            PagedResponse<Winit.Modules.Setting.Model.Classes.Setting> pagedResponse =
                JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Setting.Model.Classes.Setting>>(data);
            if (pagedResponse != null && pagedResponse.PagedData != null)
            {
                _appSetting.PopulateSettings(pagedResponse.PagedData);
            }
        }
    }

    private async Task LoadErrorDetailsAsync()
    {
        ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}KnowledgeBase/GetErrorDetailsAsync",
            HttpMethod.Post);

        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
        {
            string data =
                new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
            Dictionary<string, ErrorDetail> errorDetailDictionaryConcrete =
                JsonConvert.DeserializeObject<Dictionary<string, ErrorDetail>>(data);

            // Convert to Dictionary<string, IErrorDetail>
            Dictionary<string, IErrorDetail> errorDetailDictionary = errorDetailDictionaryConcrete
                .ToDictionary(kv => kv.Key, kv => (IErrorDetail)kv.Value);

            if (errorDetailDictionary != null)
            {
                await _errorHandlerBL.SetErrorDetailDictionary(errorDetailDictionary);
            }
        }
    }

    public void ResetbreadCrum(object sender, LocationChangedEventArgs args)
    {
        _ = CallbackService.InvokeAsync(_IDataService);
    }

    private readonly List<SelectionItem> selectionItems = new()
    {
        new SelectionItem() { Label = "Test1", IsSelected = false },
        new SelectionItem() { Label = "Test2", IsSelected = false },
        new SelectionItem() { Label = "Test3", IsSelected = false },
        new SelectionItem() { Label = "Test4", IsSelected = false },
        new SelectionItem() { Label = "Test5", IsSelected = false },
        new SelectionItem() { Label = "Test7", IsSelected = false },
        new SelectionItem() { Label = "Test6", IsSelected = false },
        new SelectionItem() { Label = "Test8", IsSelected = false },
        new SelectionItem() { Label = "Test9", IsSelected = false },
        new SelectionItem() { Label = "Test77", IsSelected = false },
        new SelectionItem() { Label = "Test77", IsSelected = false },
        new SelectionItem() { Label = "Test67", IsSelected = false },
        new SelectionItem() { Label = "Test68", IsSelected = false },
        new SelectionItem() { Label = "Test65", IsSelected = false },
    };

    public void TriggerClientSideError()
    {
        throw new Exception(@Localizer["this_is_test_exception"]);
    }

    private async Task PopulateAfterLoginData()
    {
        _loadingService.ShowLoading();
        //await GetAllMenuDetails();
        //await GetAppuser();


        //await _iCommonMasterData.PageLoadevents();

        //await GetAppuserCurrency();
        //object obj = dataManager.GetData(Winit.Shared.Models.Constants.CommonMasterDataConstants.SKUGroup);
        //await LoadSettingMaster();
        //NavigationManager.LocationChanged -= ResetbreadCrum;
        //await CallbackService.InvokeAsync(null);
        //await LoadErrorDetailsAsync();
        _loadingService.HideLoading();
    }

    private async Task OnForgotPassowrdOkClick()
    {
        (bool IsSuccessResponse, string Msg) = await _loginViewModel.VerifyUserIdAndSendRandomPassword(UserID);
        if (IsSuccessResponse)
        {
            ShowSuccessSnackBar(@Localizer["success"], Msg);
            IsForgotPasswordPopUpOpen = false;
        }
        else
        {
            ShowErrorSnackBar(@Localizer["error"], Msg);
        }
    }

    public async Task GetAllMenuDetails()
    {
        Winit.Shared.Models.Common.ApiResponse<string> apiResponse =
            await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}Role/GetAllModulesMaster?Platform=Web",
                HttpMethod.Get);
        if (apiResponse != null)
        {
            if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
            {
                ApiResponse<ModulesMasterView>? apiResponse1 =
                    JsonConvert.DeserializeObject<ApiResponse<ModulesMasterView>>(apiResponse.Data);
                if (apiResponse1 != null && apiResponse1.Data != null && apiResponse1.StatusCode == 200)
                {
                    masterView = apiResponse1.Data;
                    _dataManager?.SetData(nameof(IModulesMasterView), masterView);
                }
            }
        }
    }
}