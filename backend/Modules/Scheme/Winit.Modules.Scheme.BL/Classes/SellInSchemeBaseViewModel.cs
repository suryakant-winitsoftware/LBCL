using SkiaSharp;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.Services;
using Winit.UIComponents.SnackBar;


namespace Winit.Modules.Scheme.BL.Classes;

public abstract class SellInSchemeBaseViewModel : SchemeViewModelBase, ISellInSchemeViewModel
{
    public SellInSchemeBaseViewModel(IAppConfig appConfig, ApiService apiService,
        IServiceProvider serviceProvider, IAppUser appUser, ILoadingService loadingService,
        IAlertService alertService, IAppSetting appSetting, CommonFunctions commonFunctions, IAddProductPopUpDataHelper addProductPopUpDataHelper,
        IToast toast) :
        base(appConfig, apiService, serviceProvider, appUser, loadingService, alertService, appSetting, commonFunctions,
        addProductPopUpDataHelper, toast)
    {
        GetUserTypeWhileCreatingScheme(true, out string userType, out int ruleId);
        UserType = userType;
        RuleId = ruleId;
    }
    public ISellInSchemeDTO _SellInSchemeDTO { get; set; }
    public bool IsBranchDisabled { get; set; }
    public bool IsExpired { get; set; }

    /// <summary>
    /// Channel Partner
    /// </summary>
    /// <returns></returns>

    public string ChannelPartnerName { get; set; } = "Select Channel Partner";
    public bool IsDisplayChannelPartner { get; set; }

    public async Task PopulateViewModel()
    {
        List<Task> tasks = new List<Task>()
        {
            GetSKUAttributeData(), PopulateApplicableToCustomersAndSKU(),
        };
        IsNew = PageType.New.Equals(_commonFunctions.GetParameterValueFromURL(PageType.Page));

        if (IsNew)
        {
            _SellInSchemeDTO!.SellInHeader.UID = Guid.NewGuid().ToString();
            _SellInSchemeDTO.SellInHeader.CreatedBy = _appUser.Emp.UID;
            _SellInSchemeDTO.SellInHeader.CreatedTime = DateTime.Now;
            _SellInSchemeDTO.SellInHeader.Code = GetSchemeCodeBySchemeName(SchemeConstants.SI);
        }
        else
        {
            string UID = _commonFunctions.GetParameterValueFromURL("UID");
            await GetSellInMasterByHeaderUID(UID);
            await PopulateApprovalEngine(UID);
            SelectedChannelPartner = _serviceProvider.CreateInstance<IStore>();
            SelectedChannelPartner.UID = _SellInSchemeDTO?.SellInHeader?.FranchiseeOrgUID ?? string.Empty;
            string[] orgs = [SelectedChannelPartner.UID];
        }

        await Task.WhenAll(tasks);


        if (IsNew)
        {
            ChannelPartner.Clear();
            SelectedBranches.Clear();
            BranchDDL.ForEach(p =>
            {
                if (p.UID.Equals(_appUser!.SelectedJobPosition.BranchUID))
                {
                    p.IsSelected = true;
                    SelectedBranches.Add(p);
                }
            });
            IsBranchDisabled = BranchDDL.Any(p => p.IsSelected);
        }
        else
        {
            if (_SellInSchemeDTO != null && _SellInSchemeDTO.SellInHeader != null)
            {
                IsExpired = _SellInSchemeDTO.SellInHeader.EndDate < DateTime.Now;
            }
            if (_SellInSchemeDTO != null && _SellInSchemeDTO.SellInSchemeLines != null & _SellInSchemeDTO.SellInSchemeLines?.Count > 0)
            {
                _SellInSchemeDTO.SellInSchemeLines!.OrderBy(propa => propa.LineNumber);
                SetEditMode();
                SetEditModeForApplicabletoCustomers(_SellInSchemeDTO.SchemeBranches, _SellInSchemeDTO.SchemeOrgs, _SellInSchemeDTO.SchemeBroadClassifications);
                if (SchemeOrgs != null && SchemeOrgs.Count > 0 && SchemeBroadClassifications.Count > 0)
                {
                    ChannelPartner.Clear();
                    Stores.ForEach(p =>
                    {
                        if (SchemeBroadClassifications.Any(p => p.BroadClassificationCode.Equals(p.BroadClassificationCode, StringComparison.OrdinalIgnoreCase)))
                        {
                            ISelectionItem selectionItem = _serviceProvider.CreateInstance<ISelectionItem>();
                            selectionItem.Code = p.Code;
                            selectionItem.Label = p.Name;
                            selectionItem.UID = p.UID;

                            selectionItem.IsSelected = SchemeOrgs.Any(q => q.OrgUID == p.UID);
                            if (selectionItem.IsSelected)
                            {
                                SelectedCP.Add(selectionItem);
                            }
                            ChannelPartner.Add(selectionItem);
                        }
                    }
                    );
                }
            }
        }
    }


    #region UI Logic
    public new void OnBroadClassificationSelected(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null)
        {
            SelectedBC.Clear();
            SelectedBC.AddRange(dropDownEvent.SelectionItems);
            ChannelPartner.Clear();
            if (SelectedBC.Count > 0 && SelectedBranches.Any())
            {
                PopulateStoresByBroadClassificationAndBranch(SelectedBC.First().Code, SelectedBranches.First().UID);
            }
            else
            {
                if (!IsBranchDisabled)
                {
                    BranchDDL.ForEach(q =>
                    {
                        q.IsSelected = false;
                    });
                } 
            }
            isAddProductClick = false;
        }
    }
    public void OnBranchSelected(DropDownEvent dropDownEvent)
    {
        ChannelPartner.Clear();
        if (dropDownEvent != null)
        {
            SelectedBranches.Clear();
            SelectedBranches.AddRange(dropDownEvent.SelectionItems);
        }
        if (SelectedBC != null && SelectedBC.Any() && SelectedBranches != null && SelectedBranches.Any())
        {
            PopulateStoresByBroadClassificationAndBranch(SelectedBC.First().Code, SelectedBranches.First().UID);
        }
        isAddProductClick = false;
        ChannelPartnerName = "Select Channel Partner";
    }
    protected void PopulateStoresByBroadClassificationAndBranch(string broadClassification, string branch)
    {
        //ISelectionItem BC = SelectedBC.FirstOrDefault<ISelectionItem>();
        Stores.ForEach(p =>
        {
            if (broadClassification.Equals(p.BroadClassification, StringComparison.OrdinalIgnoreCase) && branch.Equals(p.BranchUID, StringComparison.OrdinalIgnoreCase))
            {
                ISelectionItem selectionItem = _serviceProvider.CreateInstance<ISelectionItem>();
                selectionItem.UID = p.UID;
                selectionItem.Label = p.Name;
                selectionItem.Code = p.Code;
                ChannelPartner.Add(selectionItem);

                //BranchDDL.ForEach(q =>
                //{
                //    if (q.UID == p.BrancUID)
                //    {
                //        q.IsSelected = true;
                //    }
                //});
            }
        });
    }

    bool isAddProductClick = false;
    public async Task<bool> OnAddProduct_Click()
    {
        ValidateOnAddProduct_Click(isVal: out bool isVal, message: out string message);
        if (!isVal)
        {
            await _alertService.ShowErrorAlert("Alert", message);
            return false;
        }
        if (isAddProductClick)
            return true;

        SKUV1s.Clear();
        SKUV1s.AddRange(await GetAllSKUs(new()
        {
            FilterCriterias = new()
            {
                new("OrgUID", _appUser.OrgUIDs, FilterType.Equal),
                new("BroadClassification", SelectedBC.FirstOrDefault()?.Code, FilterType.Equal),
                new("BranchUID", SelectedBranches.FirstOrDefault()?.UID, FilterType.Equal),
            }
        }));

        return isAddProductClick = true;
    }

    public async Task GetSelectedItems(List<Winit.Modules.SKU.Model.Interfaces.ISKUV1> sKULIst)
    {
        _loadingService.ShowLoading();
        if (sKULIst != null && sKULIst.Count > 0)
        {
            List<string> skuUids = new();
            List<Winit.Modules.SKU.Model.Interfaces.ISKUV1> skus = new();
            if (_SellInSchemeDTO!.SellInSchemeLines != null && _SellInSchemeDTO.SellInSchemeLines.Count > 0)
            {
                foreach (var sku in sKULIst)
                {
                    bool isExist = _SellInSchemeDTO.SellInSchemeLines.Any(p => p.SKUUID == sku.UID);
                    if (!isExist)
                    {
                        skus.Add(sku);
                        skuUids.Add(sku.UID);
                    }
                }
            }
            else
            {
                skus = sKULIst;
                skuUids = skus.Select(p => p.UID).ToList();
            }
            if (skuUids.Count == 0)
            {
                _loadingService.HideLoading();
                return;
            }
            PagingRequest pagingRequest = new()
            {
                FilterCriterias = new()
                {
                    new("SKUUID", skuUids, FilterType.In),
                }
            };

            List<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> sKUPrices =
                (await PopulatePriceMaster(SelectedBC.FirstOrDefault()?.Code ?? string.Empty, SelectedBranches.First().UID, pagingRequest.FilterCriterias)).PagedData.ToList();

            foreach (var item in skus)
            {
                Winit.Modules.SKU.Model.Interfaces.ISKUPrice? sKUPrice = sKUPrices?.Find(p => p.SKUUID == item.UID);
                if (sKUPrice != null)
                {
                    ISellInSchemeLine sellInSchemeLine = new SellInSchemeLine()
                    {
                        SellInSchemeHeaderUID = _SellInSchemeDTO.SellInHeader.UID,
                        SKUUID = item.UID,
                        SKUCode = item.Code,
                        SKUName = $"[{item.Code}]{item.Name}",
                        UID = Guid.NewGuid().ToString(),
                        CreatedBy = _appUser.Emp.UID,
                        ModifiedBy = _appUser.Emp.UID,
                        CreatedTime = DateTime.UtcNow,
                        ModifiedTime = DateTime.Now,
                        DPPrice = sKUPrice.DummyPrice,
                        MinimumSellingPrice = sKUPrice.PriceLowerLimit,
                        LadderingPrice = sKUPrice.Price,
                        FinalDealerPrice = sKUPrice.Price,
                        LineNumber = _SellInSchemeDTO.SellInSchemeLines!.Count() + 1,
                        IsDeleted = 0,
                        InvoiceDiscountType = SelleInConstants.Value,
                        CreditNoteDiscountType = SelleInConstants.Value,
                        // StandingProvisionAmount = CommonFunctions.GetDecimalValue(StandingProvisions?.Find(p => p.SKUUID == item.UID)?.Amount),
                    };
                    _SellInSchemeDTO.SellInSchemeLines!.Add(sellInSchemeLine);
                }

            }
        }
        _loadingService.HideLoading();
    }
    #endregion

    #region Calculation
    public void OnDealerRequestedPriceEnter(ISellInSchemeLine sellInSchemeLine, string Amount)
    {

        if (IsNegativeIntValue(Amount, out int entereValue))
        {
            sellInSchemeLine.RequestPrice = 0;
            return;
        }
        //decimal entereValue = CommonFunctions.GetDecimalValue(Amount);
        sellInSchemeLine.RequestPrice = entereValue;
        CalculateFinalDealerPriceMargin(sellInSchemeLine);
        if (sellInSchemeLine.RequestPrice != 0)
        {
            // sellInSchemeLine.FinalDealerPrice = sellInSchemeLine.LadderingPrice - sellInSchemeLine.InvoiceDiscount - sellInSchemeLine.CreditNoteDiscount;
        }
    }

    public void OnInvoiceDiscountEnter(ISellInSchemeLine sellInSchemeLine, string Amount)
    {
        if (IsNegativeValue(Amount, out decimal entereValue))
        {
            sellInSchemeLine.InvoiceDiscount = 0;
            return;
        }
        //decimal entereValue = CommonFunctions.GetDecimalValue(Amount);
        sellInSchemeLine.InvoiceDiscount = entereValue;

        if (sellInSchemeLine.InvoiceDiscountType == SelleInConstants.Percent)
        {
            sellInSchemeLine.Temp_InvoiceDiscount = ((sellInSchemeLine.LadderingPrice / 100) * entereValue);
            //sellInSchemeLine.FinalDealerPrice = sellInSchemeLine.LadderingPrice - sellInSchemeLine.Temp_InvoiceDiscount
            //    - (sellInSchemeLine.CreditNoteDiscountType == SelleInConstants.Percent ? sellInSchemeLine.Temp_CreditNoteDiscount : sellInSchemeLine.CreditNoteDiscount)
            //   - sellInSchemeLine.ProvisionAmount2;
        }
        //else
        //{
        //    sellInSchemeLine.FinalDealerPrice = sellInSchemeLine.LadderingPrice - sellInSchemeLine.InvoiceDiscount -
        //        (sellInSchemeLine.CreditNoteDiscountType == SelleInConstants.Percent ? sellInSchemeLine.Temp_CreditNoteDiscount :
        //        sellInSchemeLine.CreditNoteDiscount) - sellInSchemeLine.ProvisionAmount2;
        //}
        CalculateFinalDealerPriceMargin(sellInSchemeLine);


    }
    public void OnCreditNoteEnter(ISellInSchemeLine sellInSchemeLine, string Amount)
    {
        if (IsNegativeValue(Amount, out decimal entereValue))
        {
            sellInSchemeLine.CreditNoteDiscount = 0;
            return;
        }
        //decimal entereValue = CommonFunctions.GetDecimalValue(Amount);
        sellInSchemeLine.CreditNoteDiscount = entereValue;

        if (sellInSchemeLine.CreditNoteDiscountType == SelleInConstants.Percent)
        {
            sellInSchemeLine.Temp_CreditNoteDiscount = ((sellInSchemeLine.LadderingPrice / 100) * entereValue);
        }
        CalculateFinalDealerPriceMargin(sellInSchemeLine);
    }
    public void CalculateFinalDealerPriceMargin(ISellInSchemeLine sellInSchemeLine)
    {
        sellInSchemeLine.FinalDealerPrice = sellInSchemeLine.LadderingPrice
            - (sellInSchemeLine.InvoiceDiscountType == SelleInConstants.Percent ?
                sellInSchemeLine.Temp_InvoiceDiscount : sellInSchemeLine.InvoiceDiscount)
            - (sellInSchemeLine.CreditNoteDiscountType == SelleInConstants.Percent ?
                sellInSchemeLine.Temp_CreditNoteDiscount : sellInSchemeLine.CreditNoteDiscount)
            - sellInSchemeLine.ProvisionAmount2;

        if (_appUser.Role.HasP3Access)
        {
            sellInSchemeLine.FinalDealerPrice = sellInSchemeLine.FinalDealerPrice
                - sellInSchemeLine.ProvisionAmount3 - sellInSchemeLine.StandingProvisionAmount;
        }

        sellInSchemeLine.Margin = sellInSchemeLine.FinalDealerPrice - sellInSchemeLine.MinimumSellingPrice;
    }

    public void OnProvisionAmount2Enter(ISellInSchemeLine sellInSchemeLine, string Amount)
    {
        if (IsNegativeValue(Amount, out decimal entereValue))
        {
            sellInSchemeLine.ProvisionAmount2 = 0;
            return;
        }
        // decimal entereValue = CommonFunctions.GetDecimalValue(Amount);
        sellInSchemeLine.ProvisionAmount2 = entereValue;
        //if (sellInSchemeLine.InvoiceDiscountType == SelleInConstants.Percent)
        //{
        //    sellInSchemeLine.FinalDealerPrice = sellInSchemeLine.LadderingPrice - sellInSchemeLine.Temp_InvoiceDiscount -
        //        sellInSchemeLine.Temp_CreditNoteDiscount - sellInSchemeLine.ProvisionAmount2;
        //}
        //else
        //{
        //    sellInSchemeLine.FinalDealerPrice = sellInSchemeLine.LadderingPrice - sellInSchemeLine.InvoiceDiscount -
        //        sellInSchemeLine.CreditNoteDiscount - sellInSchemeLine.ProvisionAmount2;
        //}
        CalculateFinalDealerPriceMargin(sellInSchemeLine);


    }
    public void OnProvisionAmount3Enter(ISellInSchemeLine sellInSchemeLine, string Amount)
    {
        if (IsNegativeValue(Amount, out decimal entereValue))
        {
            sellInSchemeLine.ProvisionAmount3 = 0;
            return;
        }
        //decimal entereValue = CommonFunctions.GetDecimalValue(Amount);
        sellInSchemeLine.ProvisionAmount3 = entereValue;
        CalculateFinalDealerPriceMargin(sellInSchemeLine);
    }
    #endregion
    protected (bool, string) Validate()
    {
        bool isVal = true;
        string message = string.Empty;
        //bool requestedPrice = _SellInSchemeDTO!.SellInSchemeLines.Any(p => p.RequestPrice == 0);
        //bool committedQty = _SellInSchemeDTO!.SellInSchemeLines.Any(p => p.CommittedQty == 0);
        //if (requestedPrice)
        //{
        //    message += "Dealer Requested Price,";
        //    isVal = false;
        //}
        //if (committedQty)
        //{
        //    message += "Committed Qty By CP ,";
        //    isVal = false;
        //}
        if (!isVal)
        {
            message = "Please enter : " + message.Substring(0, message.Length - 1);
        }
        return (isVal, message);
    }
    public void IsItemsDiscountValidated(out bool isVal, out string message)
    {
        message = string.Empty;
        isVal = _SellInSchemeDTO.SellInSchemeLines.Any();
        if (!isVal)
        {
            message = "Add atleast one item to Continue! ";
            return;
        }
        string itemCodes = string.Empty;
        foreach (var item in _SellInSchemeDTO.SellInSchemeLines)
        {
            if (item.CommittedQty == 0 && item.InvoiceDiscount == 0 && item.CreditNoteDiscount == 0
        && item.ProvisionAmount2 == 0 && item.ProvisionAmount3 == 0)
            {
                itemCodes += $"{item.SKUCode}, ";
                isVal = false;
            }
        }
        message = $"For Item Codes: {itemCodes} .... no data entered please check.";
    }
    public void ValidateOnAddProduct_Click(out bool isVal, out string message)
    {
        isVal = true;
        message = string.Empty;
        List<string> messages = [];
        if (IsNew)
        {
            //if (SelectedChannelPartner == null)
            //{
            //    isVal = false;
            //    message += "Channel Partner,";
            //}
            if (_SellInSchemeDTO!.SellInHeader.MaxDealCount == 0)
            {
                isVal = false;
                messages.Add("Request type ");
            }
            if (_SellInSchemeDTO.SellInHeader.ValidFrom == null)
            {
                isVal = false;
                messages.Add("Start date ");
            }
        }
        if (_SellInSchemeDTO!.SellInHeader.EndDate == null)
        {
            isVal = false;
            messages.Add("End date ");
        }
        // if (!string.IsNullOrEmpty(message))
        // {
        //     message = "Please Select : " + message;
        // }
        if (isVal && _SellInSchemeDTO!.SellInHeader.EndDate < _SellInSchemeDTO!.SellInHeader.ValidFrom)
        {
            isVal = false;
            messages.Add("End Date should be greater than Start Date ");
        }
        if (isVal)
        {
            if (SelectedBC.Count == 0)
            {
                isVal = false;
                messages.Add("Broad classification ");
            }
            if (SelectedBranches.Count == 0)
            {
                isVal = false;
                messages.Add("Branch ");
            }
        }
        if (!isVal)
            message = "Please select these fields " + string.Join(",", messages) + " to continue";
    }

    protected void SetEditMode()
    {

        _SellInSchemeDTO!.SellInSchemeLines.OrderBy(p => p.LineNumber);
        foreach (var item in _SellInSchemeDTO!.SellInSchemeLines)
        {
            //item.Margin = item.FinalDealerPrice - item.MinimumSellingPrice;
            if (item.InvoiceDiscountType == SelleInConstants.Percent)
            {
                item.Temp_InvoiceDiscount = ((item.LadderingPrice / 100) * item.InvoiceDiscount);
            }
            if (item.CreditNoteDiscountType == SelleInConstants.Percent)
            {
                item.Temp_CreditNoteDiscount = ((item.LadderingPrice / 100) * item.CreditNoteDiscount);
            }
            CalculateFinalDealerPriceMargin(item);
        }
        foreach (ISelectionItem selectionItem in ChannelPartner)
        {
            if (selectionItem.UID == _SellInSchemeDTO.SellInHeader.FranchiseeOrgUID)
            {
                selectionItem.IsSelected = true;

                SelectedChannelPartner = Stores.Find(p => p.UID == selectionItem.UID);
                //SetApprovalEngineRuleByChannelPartner();
            }
        }
        if (_SellInSchemeDTO.Wallet != null && _SellInSchemeDTO.Wallet.Count > 0)
        {
            Branch_P2Amount = _SellInSchemeDTO.Wallet.FirstOrDefault()!.BalanceAmount;
        }

    }
    private void PrePareApplicabletoCustomersCreate()
    {
        SelectedBranches.ForEach(p =>
        {
            ISchemeBranch schemeBranch = _serviceProvider.CreateInstance<ISchemeBranch>();
            CreateFields(schemeBranch);
            schemeBranch.LinkedItemType = SchemeConstants.SellInScheme;
            schemeBranch.LinkedItemUID = _SellInSchemeDTO.SellInHeader.UID;
            schemeBranch.BranchCode = p.Code!;
        });

    }
    protected void PrePareApplicabletoCustomers()
    {
        PrePareApplicabletoCustomers(_SellInSchemeDTO!.SellInHeader.UID, SchemeConstants.SellInScheme);
        _SellInSchemeDTO.SchemeBranches.Clear();
        _SellInSchemeDTO.SchemeBranches.AddRange(SchemeBranches);
        _SellInSchemeDTO.SchemeOrgs.Clear();
        _SellInSchemeDTO.SchemeOrgs.AddRange(SchemeOrgs);
        _SellInSchemeDTO.SchemeBroadClassifications.Clear();
        _SellInSchemeDTO.SchemeBroadClassifications.AddRange(SchemeBroadClassifications);
        //if (SelectedBranches.Any())
        //{
        //    if (!IsNew)
        //        _SellInSchemeDTO.SchemeBranches.RemoveAll(p => !SelectedBranches.Select(p => p.Code).ToList().Contains(p.BranchCode));

        //    SelectedBranches.ForEach(p =>
        //    {
        //        if (!_SellInSchemeDTO.SchemeBranches.Any(q => q.BranchCode == p.Code))
        //        {
        //            ISchemeBranch schemeBranch = _serviceProvider.CreateInstance<ISchemeBranch>();
        //            CreateFields(schemeBranch);
        //            schemeBranch.LinkedItemType = SchemeConstants.SellInScheme;
        //            schemeBranch.LinkedItemUID = _SellInSchemeDTO.SellInHeader.UID;
        //            schemeBranch.BranchCode = p.Code;

        //            _SellInSchemeDTO.SchemeBranches.Add(schemeBranch);
        //        }

        //    });
        //}
        //if (SelectedBC.Any())
        //{
        //    if (!IsNew)
        //        _SellInSchemeDTO.SchemeBroadClassifications.RemoveAll(p => !SelectedBC.Select(p => p.Code).ToList().Contains(p.BroadClassificationCode));

        //    SelectedBC.ForEach(p =>
        //    {

        //        if (!_SellInSchemeDTO.SchemeBroadClassifications.Any(q => q.BroadClassificationCode == p.Code))
        //        {
        //            ISchemeBroadClassification schemeBC = _serviceProvider.CreateInstance<ISchemeBroadClassification>();
        //            CreateFields(schemeBC);
        //            schemeBC.LinkedItemType = SchemeConstants.SellInScheme;
        //            schemeBC.LinkedItemUID = _SellInSchemeDTO.SellInHeader.UID;
        //            schemeBC.BroadClassificationCode = p.Code;
        //            _SellInSchemeDTO.SchemeBroadClassifications.Add(schemeBC);
        //        }


        //    });
        //}
        //if (SelectedCP.Any())
        //{
        //    if (!IsNew)
        //        _SellInSchemeDTO.SchemeOrgs.RemoveAll(p => !SelectedCP.Select(p => p.UID).ToList().Contains(p.OrgUID));
        //    SelectedCP.ForEach(p =>
        //    {

        //        if (!_SellInSchemeDTO.SchemeOrgs.Any(q => q.OrgUID == p.UID))
        //        {
        //            ISchemeOrg schemeOrg = _serviceProvider.CreateInstance<ISchemeOrg>();
        //            CreateFields(schemeOrg);
        //            schemeOrg.LinkedItemType = SchemeConstants.SellInScheme;
        //            schemeOrg.LinkedItemUID = _SellInSchemeDTO.SellInHeader.UID;
        //            schemeOrg.OrgUID = p.Code;
        //            _SellInSchemeDTO.SchemeOrgs.Add(schemeOrg);
        //        }
        //    });
        //}
    }

    #region Abstract classes
    protected abstract Task<List<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>> GetSKUPrice(List<string> skuUids);
    protected abstract Task GetSellInMasterByHeaderUID(string UID);
    #endregion
}
