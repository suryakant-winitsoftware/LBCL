using Winit.Modules.Base.BL;
using Winit.Modules.Base.Model;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.Services;
using Winit.UIComponents.SnackBar;

namespace Winit.Modules.Scheme.BL.Classes;

public class SchemeViewModelBase : SchemeApprovalEngineBaseViewModel, ISchemeViewModelBase
{
    public IToast _toast { get; }
    public IAddProductPopUpDataHelper _addProductPopUpDataHelper { get; }
    public List<SKUAttributeDropdownModel>? SKUAttributeData { get; set; }

    protected readonly Winit.Shared.Models.Common.IAppConfig _appConfig;
    protected readonly ApiService _apiService;
    protected readonly IServiceProvider _serviceProvider;
    protected readonly IAppUser _appUser;
    protected readonly ILoadingService _loadingService;
    protected readonly IAlertService _alertService;
    protected readonly IAppSetting _appSetting;
    protected readonly CommonFunctions _commonFunctions;


    public SchemeViewModelBase(Winit.Shared.Models.Common.IAppConfig appConfig, ApiService apiService,
        IServiceProvider serviceProvider, IAppUser appUser, ILoadingService loadingService, IAlertService alertService,
        IAppSetting appSetting, CommonFunctions commonFunctions, IAddProductPopUpDataHelper addProductPopUpDataHelper, IToast toast) : base(appConfig, apiService,
    serviceProvider, appUser, loadingService, alertService,
    appSetting, commonFunctions)
    {
        _apiService = apiService;
        _appConfig = appConfig;
        _serviceProvider = serviceProvider;
        _appUser = appUser;
        _loadingService = loadingService;
        _alertService = alertService;
        _appSetting = appSetting;
        _commonFunctions = commonFunctions;
        _addProductPopUpDataHelper = addProductPopUpDataHelper;
        _toast = toast;
        Stores = [];
        ChannelPartner = [];

        // IsForHOView = appUser.Role.Code.ToUpper() != SchemeConstants.BM;
        //IsWalletInfoNeededForChannelPartner = true;
        StandingProvisions = new();
        Branch_P2Amount = 0;
    }

    public delegate void AddSub(string key, string value);
    public bool IsNew { get; set; }
    public bool IsDisable { get; set; }
    public bool IsViewMode { get; set; }
    public bool IsIntialize { get; set; }
    public List<Winit.Shared.Models.Common.ISelectionItem> ChannelPartner { get; set; } = [];
    public List<IWallet> Wallets { get; set; } = [];
    protected List<IStore> Stores { get; set; } = [];
    public IStore? SelectedChannelPartner { get; set; }
    public List<ISKUV1> SKUV1s { get; set; } = [];
    public List<ISKUV1> SelectedSKUs { get; set; } = [];
    public List<IFileSys> FileSysList { get; set; } = [];
    public List<IFileSys> ModifiedFileSysList { get; set; } = [];
    public List<ISelectionItem> BranchDDL { get; set; } = [];
    public List<ISelectionItem> BroadClassificationDDL { get; set; } = [];
    protected List<IStandingProvision> StandingProvisions { get; set; }
    protected bool IsWalletInfoNeededForChannelPartner { get; set; }
    public decimal? Branch_P2Amount { get; set; }
    public decimal? HO_P3Amount { get; set; }
    public decimal? HO_S_Amount { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal AvailableLimit { get; set; }
    public decimal CurrentOutStanding { get; set; }


    #region UI Common Logic
    protected List<ISKUGroupType> SKUGroupType = [];
    protected List<ISKUGroup> SKUGroup = [];
    protected async Task GetSKUGroupType()
    {
        SKUGroupType.Clear();
        SKUGroupType.AddRange(await _addProductPopUpDataHelper.GetSKUGroupType(new()));
    }
    protected async Task GetSKUGroup()
    {
        SKUGroup.Clear();
        SKUGroup.AddRange(await _addProductPopUpDataHelper.GetSKUGroup(new()));
    }


    public bool IsNegativeValue(string inPut, out decimal val)
    {
        val = CommonFunctions.GetDecimalValue(inPut);
        if (val < 0)
        {
            _toast.Add(title: "Alert", severity: Winit.UIComponents.SnackBar.Enum.Severity.Error, message: "You are not allowed to enter negative values");
            val = 0;
            return true;
        }
        return false;
    }
    public bool IsNegativeIntValue(string inPut, out int val)
    {
        val = CommonFunctions.GetIntValue(inPut);
        if (val < 0)
        {
            _toast.Add(title: "Alert", severity: Winit.UIComponents.SnackBar.Enum.Severity.Error, message: "You are not allowed to enter negative values");
            val = 0;
            return true;
        }
        return false;
    }
    protected string GetSchemeCodeBySchemeName(string scheme)
    {
        return scheme + DateTime.Now.ToString("yyyyMMddhhmmssfff");
    }

    public async Task<bool> ValidateContributions(decimal? contribution1, decimal? contribution2, decimal? contribution3)
    {
        if ((CommonFunctions.GetDecimalValue(contribution1!) + CommonFunctions.GetDecimalValue(contribution2!) + CommonFunctions.GetDecimalValue(contribution3!)) != 100)
        {
            await _alertService.ShowErrorAlert("Validation Error", "Branch, HO(P3) and HO(S) Contribution sum must 100%.");
            return false;
        }
        return true;
    }
    public void SetChannelPartnerSelectedonEditMode(string channelPartnerUID)
    {
        ChannelPartner.ForEach(partner => partner.IsSelected = partner.UID == channelPartnerUID);
        base.SelectedChannelPartner = this.SelectedChannelPartner = Stores.Find(p => p.UID == channelPartnerUID);
    }
    public void OnFilesUpload(List<IFileSys> fileSys)
    {
        if (fileSys != null)
            ModifiedFileSysList.AddRange(fileSys.Except(ModifiedFileSysList));
    }
    protected void CreateFields(IBaseModel baseModel)
    {
        baseModel.UID = Guid.NewGuid().ToString();
        baseModel.CreatedBy = _appUser.Emp.UID;
        baseModel.ModifiedBy = _appUser.Emp.UID;
        baseModel.CreatedTime = DateTime.Now.Date;
        baseModel.ModifiedTime = DateTime.Now.Date;
    }
    protected List<ISelectionItem> SelectedBranches = [];
    protected List<ISelectionItem> SelectedCP = [];
    protected List<ISelectionItem> SelectedBC = [];
    public void OnBroadClassificationSelected(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null)
        {
            SelectedBC.Clear();
            SelectedBC.AddRange(dropDownEvent.SelectionItems);
        }
    }
    public void OnBranchSelected(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null)
        {
            SelectedBranches.Clear();
            SelectedBranches.AddRange(dropDownEvent.SelectionItems);
        }
    }

    public void OnChannelpartnerSelectedUI(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null)
        {
            SelectedCP.Clear();
            SelectedCP.AddRange(dropDownEvent.SelectionItems);
        }
    }
    protected List<ISchemeBranch> SchemeBranches { get; private set; } = [];
    protected List<ISchemeBroadClassification> SchemeBroadClassifications { get; private set; } = [];
    protected List<ISchemeOrg> SchemeOrgs { get; private set; } = [];
    public List<IStore> AllCustomersByFilters()
    {
        List<IStore> selectionItems = new List<IStore>();
        //selectionItems = Stores.Where(SelectedCP, SelectedBC=>(p,q,r)=>q.c);
        bool isBranchSelected = SelectedBranches.Any();
        bool isChannelPartnerSelected = SelectedCP.Any();
        if (isBranchSelected && isChannelPartnerSelected)
        {
            Stores.ForEach(p =>
            {
                if (SelectedCP.Any(q => p.UID == q.UID && q.IsSelected) &&
                    SelectedBranches.Any(q => q.UID == p.BranchUID) &&
                    SelectedBC.Any(q => q.Code!.Equals(p.BroadClassification, StringComparison.OrdinalIgnoreCase)))
                {
                    selectionItems.Add(p);
                }
            });
        }
        else if (isBranchSelected)
        {
            Stores.ForEach(p =>
            {
                if (SelectedBranches.Any(q => q.UID == p.BranchUID) &&
                    SelectedBC.Any(q => q.Code!.Equals(p.BroadClassification, StringComparison.OrdinalIgnoreCase)))
                {
                    selectionItems.Add(p);
                }
            });
        }
        else
        {
            Stores.ForEach(p =>
            {
                if (SelectedBC.Any(q => q.Code!.Equals(p.BroadClassification, StringComparison.OrdinalIgnoreCase)))
                {
                    selectionItems.Add(p);
                }
            });
        }
        return selectionItems;
    }
    protected void SetEditModeForApplicabletoCustomers(List<ISchemeBranch> schemeBranches,
        List<ISchemeOrg> schemeOrgs, List<ISchemeBroadClassification> schemeBroadClassifications)
    {
        SchemeBranches.Clear();
        SchemeBranches.AddRange(schemeBranches);
        SchemeOrgs.Clear();
        SchemeOrgs.AddRange(schemeOrgs);
        SchemeBroadClassifications.Clear();
        SchemeBroadClassifications.AddRange(schemeBroadClassifications);
        SelectedBranches.Clear();
        SelectedCP.Clear();
        SelectedBC.Clear();

        if (SchemeBranches != null && SchemeBranches.Count > 0)
        {
            BranchDDL.ForEach(p =>
            {
                p.IsSelected = SchemeBranches.Any(q => q.BranchCode == p.Code);
                if (p.IsSelected)
                {
                    SelectedBranches.Add(p);
                }
            }
            );
        }
        if (SchemeBroadClassifications != null && SchemeBroadClassifications.Count > 0)
        {
            BroadClassificationDDL.ForEach(p =>
            {
                p.IsSelected = SchemeBroadClassifications.Any(q => q.BroadClassificationCode == p.Code);
                if (p.IsSelected)
                {
                    SelectedBC.Add(p);
                }
            }
            );
        }
        if (SchemeOrgs != null && SchemeOrgs.Count > 0)
        {
            ChannelPartner.ForEach(p =>
            {
                p.IsSelected = SchemeOrgs.Any(q => q.OrgUID == p.UID);
                if (p.IsSelected)
                {
                    SelectedCP.Add(p);
                }
            }
            );
        }


    }
    protected void PrePareApplicabletoCustomers(string linkedItemUID, string linkedItemType)
    {
        if (SelectedBranches.Any())
        {
            if (!IsNew)
                SchemeBranches.RemoveAll(p => !SelectedBranches.Select(p => p.Code).ToList().Contains(p.BranchCode));

            SelectedBranches.ForEach(p =>
            {
                if (!SchemeBranches.Any(q => q.BranchCode == p.Code))
                {
                    ISchemeBranch schemeBranch = _serviceProvider.CreateInstance<ISchemeBranch>();
                    CreateFields(schemeBranch);
                    schemeBranch.LinkedItemType = linkedItemType;
                    schemeBranch.LinkedItemUID = linkedItemUID;
                    schemeBranch.BranchCode = p.Code;

                    SchemeBranches.Add(schemeBranch);
                }

            });
        }
        if (SelectedBC.Any())
        {
            if (!IsNew)
                SchemeBroadClassifications.RemoveAll(p => !SelectedBC.Select(p => p.Code).ToList().Contains(p.BroadClassificationCode));

            SelectedBC.ForEach(p =>
            {

                if (!SchemeBroadClassifications.Any(q => q.BroadClassificationCode == p.Code))
                {
                    ISchemeBroadClassification schemeBC = _serviceProvider.CreateInstance<ISchemeBroadClassification>();
                    CreateFields(schemeBC);
                    schemeBC.LinkedItemType = linkedItemType;
                    schemeBC.LinkedItemUID = linkedItemUID;
                    schemeBC.BroadClassificationCode = p.Code;
                    SchemeBroadClassifications.Add(schemeBC);
                }
            });
        }
        if (SelectedCP.Any())
        {
            if (!IsNew)
                SchemeOrgs.RemoveAll(p => !SelectedCP.Select(p => p.UID).ToList().Contains(p.OrgUID));
            SelectedCP.ForEach(p =>
            {

                if (!SchemeOrgs.Any(q => q.OrgUID == p.UID))
                {
                    ISchemeOrg schemeOrg = _serviceProvider.CreateInstance<ISchemeOrg>();
                    CreateFields(schemeOrg);
                    schemeOrg.LinkedItemType = linkedItemType;
                    schemeOrg.LinkedItemUID = linkedItemUID;
                    schemeOrg.OrgUID = p.UID;
                    SchemeOrgs.Add(schemeOrg);
                }
            });
        }
    }
    protected bool IsApplicableToCustomersValidated()
    {
        bool isOrg = true;
        bool isBC = true;
        bool isBranch = true;
        if (SelectedCP == null || SelectedCP.Count == 0)
        {
            isOrg = false;
        }
        if (SelectedBC == null || SelectedBC.Count == 0)
        {
            isBC = false;
        }
        if (SelectedBranches == null || SelectedBranches.Count == 0)
        {
            isBranch = false;
        }
        return isBranch || isBC || isOrg;
    }
    protected async Task PopulateApplicableToCustomersAndSKU()
    {

        await Task.WhenAll(
        //GetAllSKUs(),
        GetAllChannelPartner(),
        GetBranchDetails(),
        GetBroadClassificationHeaderDetails(new()
        {
            FilterCriterias = new()
            {
                new("IsActive", true, FilterType.Equal)
            },
        }));
    }
    
    protected async Task GetSKUAttributeData()
    {
        try
        {
            SKUAttributeData = await _addProductPopUpDataHelper.GetSKUAttributeData();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    #endregion

    public async Task GetSKUsAndPriceList_OnChannelpartnerSelection(DropDownEvent dropDownEvent)
    {
        _loadingService.ShowLoading();
        await OnChannelpartnerSelected(dropDownEvent);
        if (SelectedChannelPartner != null)
        {
            await GetSKUsByOrgUID(SelectedChannelPartner.UID);
            await GetStandingProvisionAmountByChannelPartnerUID(SelectedChannelPartner.UID);
        }
        _loadingService.HideLoading();
    }
    private Dictionary<string, string> DivisionEmpKVPair { get; set; }

    protected async Task GetSKUsByOrgUID(string orgUID)
    {
        if (orgUID == null || SelectedChannelPartner == null || _appUser?.OrgUIDs == null)
            return;

        List<string>? orgs = await GetOrgHierarchyParentUIDsByOrgUID(orgUID);

        List<IAsmDivisionMapping>? divisions = await GetAsmDivisionMappingByUID("address", SelectedChannelPartner!.ShippingAddressUID);
        if (divisions == null || !divisions.Any())
        {

            divisions = new List<IAsmDivisionMapping>();
        }
        DivisionEmpKVPair = divisions.ToDictionary(
        e => e.DivisionUID,// Key: Division UID
        f => f.AsmEmpUID// Value: Employee UID
        ) ?? new Dictionary<string, string>();

        PagingRequest pagingRequest = new()
        {
            FilterCriterias = new List<FilterCriteria>
            {
                new FilterCriteria("OrgUID", _appUser.OrgUIDs, FilterType.Equal),
                new FilterCriteria("StoreUID", SelectedChannelPartner.UID, FilterType.Equal),
                new FilterCriteria("Date", DateTime.Now, FilterType.Equal),
                new FilterCriteria("SupplierOrgUIDs", DivisionEmpKVPair.Keys, FilterType.In),
            }
        };

        SKUV1s = await GetAllSKUs(pagingRequest) ?? new List<ISKUV1>();

        if (!SKUV1s.Any())
        {
            _toast.Add("Error", "There is no SKU's for this channel partner", UIComponents.SnackBar.Enum.Severity.Warning);
        }
    }

    protected async Task<List<string>?> GetOrgHierarchyParentUIDsByOrgUID(string orgUID)
    {
        ApiResponse<List<string>> apiResponse = await _apiService.FetchDataAsync<List<string>>(
        $"{_appConfig.ApiBaseUrl}Org/GetOrgHierarchyParentUIDsByOrgUID",
        HttpMethod.Post, new List<string>
        {
            orgUID
        });

        return apiResponse.Data;
    }
    public async Task OnChannelpartnerSelected(DropDownEvent dropDownEvent)
    {
        _loadingService.ShowLoading();
        ISelectionItem? selectionItem = null;
        if (dropDownEvent != null)
        {
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
            {
                selectionItem = dropDownEvent.SelectionItems.FirstOrDefault();
                base.SelectedChannelPartner = this.SelectedChannelPartner = Stores.Find(p => p.UID == selectionItem?.UID);
                if (SelectedChannelPartner != null)
                {
                    if (IsWalletInfoNeededForChannelPartner)
                    {
                        Wallets = await GetWalletByOrgUID(SelectedChannelPartner.UID);
                        if (Wallets != null && Wallets.Count > 0)
                        {
                            Branch_P2Amount = Wallets.FirstOrDefault(p => p.LinkedItemType == WalletConstants.Branch && p.Type == WalletConstants.P2)?.ActualAmount;
                            HO_P3Amount = Wallets.FirstOrDefault(p => p.LinkedItemType == WalletConstants.HO && p.Type == WalletConstants.P3)?.ActualAmount;
                            HO_S_Amount = Wallets.FirstOrDefault(p => p.LinkedItemType == WalletConstants.HO && p.Type == WalletConstants.P3S)?.ActualAmount;
                        }
                        else
                        {
                            Branch_P2Amount = 0;
                            HO_P3Amount = 0;
                            HO_S_Amount = 0;
                            _toast.Add("Error", "There is no provision amount for this channel partner");
                        }
                    }
                    //SetApprovalEngineRuleByChannelPartner();
                }


            }
            else
            {
                base.SelectedChannelPartner = this.SelectedChannelPartner = null;
                Wallets.Clear();
                Branch_P2Amount = 0;
                HO_P3Amount = 0;
                HO_S_Amount = 0;
            }
        }
        _loadingService.HideLoading();
    }

    protected async Task<List<ISKUV1>> GetAllSKUs(PagingRequest pagingRequest)
    {
        try
        {
            ApiResponse<PagedResponse<ISKUV1>> apiResponse =
                await _apiService.FetchDataAsync<PagedResponse<ISKUV1>>(
                $"{_appConfig.ApiBaseUrl}SKU/SelectAllSKUDetails",
                HttpMethod.Post, pagingRequest);

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
            {
                return apiResponse.Data.PagedData.ToList();
            }
        }
        catch (Exception)
        {
            throw;
        }
        return [];
    }
    protected async Task<List<IWallet>> GetWalletByOrgUID(string OrgUID)
    {

        try
        {
            Winit.Shared.Models.Common.ApiResponse<List<IWallet>> apiResponse = await _apiService.FetchDataAsync<List<IWallet>>(
            $"{_appConfig.ApiBaseUrl}Wallet/GetWalletByOrgUID?OrgUID={OrgUID}",
            HttpMethod.Get);


            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data != null)
            {
                return apiResponse.Data.ToList();
            }
        }
        catch (Exception ex)
        {

        }
        return [];
    }
    protected async Task GetStandingProvisionAmountByChannelPartnerUID(string channelPartnerUID)
    {

        Winit.Shared.Models.Common.ApiResponse<List<IStandingProvision>> apiResponse = await _apiService.FetchDataAsync<List<IStandingProvision>>(
        $"{_appConfig.ApiBaseUrl}SellInSchemeHeader/GetStandingProvisionAmountByChannelPartnerUID?channelPartnerUID={channelPartnerUID}",
        HttpMethod.Get);


        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.Count > 0)
        {
            StandingProvisions = apiResponse.Data.ToList();
        }
    }
    public async Task GetAllChannelPartner()
    {
        Winit.Shared.Models.Common.ApiResponse<List<Winit.Modules.Store.Model.Interfaces.IStore>> apiResponse = await _apiService.FetchDataAsync<List<Winit.Modules.Store.Model.Interfaces.IStore>>(
        $"{_appConfig.ApiBaseUrl}Store/GetChannelPartner?jobPositionUid={_appUser.SelectedJobPosition.UID}",
        HttpMethod.Get);

        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
        {
            Stores = apiResponse.Data;
            ChannelPartner.Clear();
            foreach (var s in apiResponse.Data)
            {
                ISelectionItem selectionItem = new SelectionItem()
                {
                    UID = s.UID,
                    Code = s.Code,
                    Label = $"[{s.Code}] {s.Name}",
                };
                ChannelPartner.Add(selectionItem);
            }
        }
    }

    protected async Task<PagedResponse<ISKUPrice>> PopulatePriceMaster(string broadClassification, string branchUID, List<FilterCriteria>? filterCriterias = null, List<SortCriteria>? sortCriterias = null,
        int? pageNumber = null, int? pageSize = null,
        bool? isCountRequired = null)
    {
        PagingRequest pagingRequest = new()
        {
            SortCriterias = sortCriterias,
            PageNumber = pageNumber ?? 0,
            PageSize = pageSize ?? 0,
            FilterCriterias = filterCriterias,
            IsCountRequired = isCountRequired ?? false
        };
        ApiResponse<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>> apiResponse = await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>>(
        $"{_appConfig.ApiBaseUrl}SKUPrice/SelectAllSKUPriceDetailsByBroadClassification?broadClassification={broadClassification}&branchUID={branchUID}&type={_appSetting.PriceApplicationModel}",
        HttpMethod.Post, pagingRequest);

        return apiResponse != null && apiResponse.Data != null && apiResponse.Data.PagedData != null
            ? apiResponse.Data
            : new PagedResponse<ISKUPrice>();
    }

    public async Task<ApiResponse<string>> UploadFiles(List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> files)
    {
        return await _apiService.FetchDataAsync(
        $"{_appConfig.ApiBaseUrl}FileSys/CreateFileSysForBulk", HttpMethod.Post, files);
    }
    protected async Task<List<IFileSys>> GetAttatchedFiles(string LinkedItemUID, string fileSysType = null)
    {
        PagingRequest request = new()
        {
            FilterCriterias = new()
            {
                new FilterCriteria("LinkedItemUID", LinkedItemUID, FilterType.Equal),
            }
        };
        ApiResponse<PagedResponse<IFileSys>> apiResponse = await _apiService.FetchDataAsync<PagedResponse<IFileSys>>(
        $"{_appConfig.ApiBaseUrl}FileSys/SelectAllFileSysDetails", HttpMethod.Post, request);
        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
        {
            return apiResponse.Data.PagedData.ToList();
        }
        return [];
    }





    public async Task<List<IAsmDivisionMapping>?> GetAsmDivisionMappingByUID(string linkedItemType, string linkedItemUID)
    {
        try
        {
            Winit.Shared.Models.Common.ApiResponse<List<IAsmDivisionMapping>> apiResponse = await
                _apiService.FetchDataAsync<List<IAsmDivisionMapping>>
                    ($"{_appConfig.ApiBaseUrl}Store/GetAsmDivisionMappingByUID/{linkedItemType}/{linkedItemUID}", HttpMethod.Get);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data is not null)
            {
                return apiResponse.Data;
            }
            return default;
        }
        catch (Exception)
        {

            throw;
        }
    }
    protected async Task GetBroadClassificationHeaderDetails(PagingRequest pagingRequest)
    {
        ApiResponse<PagedResponse<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader>> apiResponse =
            await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader>>
            ($"{_appConfig.ApiBaseUrl}BroadClassificationHeader/GetBroadClassificationHeaderDetails",
            HttpMethod.Post, pagingRequest);
        if (apiResponse != null && apiResponse.IsSuccess & apiResponse.Data != null && apiResponse.Data?.PagedData != null)
        {
            BroadClassificationDDL.Clear();
            foreach (var item in apiResponse.Data.PagedData)
            {
                ISelectionItem selectionItem = new SelectionItem()
                {
                    UID = item.UID,
                    Code = item.Name,
                    Label = item.Name,
                };
                BroadClassificationDDL.Add(selectionItem);
            }
        }
    }
    protected async Task GetBranchDetails()
    {
        Winit.Shared.Models.Common.ApiResponse<List<IBranch>> apiResponse = await _apiService.FetchDataAsync<List<IBranch>>(
        $"{_appConfig.ApiBaseUrl}Branch/GetBranchByJobPositionUid?jobPositionUid={_appUser.SelectedJobPosition.UID}",
        HttpMethod.Get);

        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data != null)
        {
            foreach (IBranch b in apiResponse.Data)
            {
                BranchDDL.Add(new SelectionItem()
                {
                    UID = b.UID,
                    Code = b.Code,
                    Label = b.Name,
                });
            }
        }
    }
}
