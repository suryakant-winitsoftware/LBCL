using Microsoft.Extensions.DependencyInjection;
using Winit.Modules.Base.BL;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.Services;
using Winit.UIComponents.SnackBar;


namespace Winit.Modules.Scheme.BL.Classes
{
    public abstract class SellOutSchemeBaseViewModel : SchemeViewModelBase, ISellOutSchemeHeaderItemViewModel
    {




        public SellOutSchemeBaseViewModel(IServiceProvider serviceProvider,
                IFilterHelper filter,
                ISortHelper sorter,
                IListHelper listHelper,
                IAppUser appUser,
                IAppSetting appSetting,
                IDataManager dataManager,
                IAppConfig appConfigs, ApiService apiService,
                ILoadingService loadingService,
                IAlertService alertService, CommonFunctions commonFunctions, IAddProductPopUpDataHelper addProductPopUpDataHelper, IToast toast) : base(appConfigs, apiService,
                    serviceProvider, appUser, loadingService, alertService, appSetting, commonFunctions, addProductPopUpDataHelper, toast)
        {

            SellOutMaster = _serviceProvider.GetRequiredService<ISellOutMasterScheme>();
            SellOutMaster.SellOutSchemeHeader = _serviceProvider.GetRequiredService<ISellOutSchemeHeader>();
            SellOutMaster.SellOutSchemeLines = [];
        }
        public ISellOutMasterScheme SellOutMaster { get; set; }
        public List<Winit.Shared.Models.Common.ISelectionItem> SellOutReasons { get; set; } = [];
        public List<IPreviousOrders> PreviousOrdersList { get; set; } = [];


        #region UI Login

        public async Task PopulateViewModel()
        {
            IsNew = PageType.New.Equals(_commonFunctions.GetParameterValueFromURL(PageType.Page));
            await GetAllChannelPartner();
            await GetSellOutReasons("SELLOUTREASON");
            if (IsNew)
            {
                IsWalletInfoNeededForChannelPartner = true;
                SellOutMaster.SellOutSchemeHeader.UID = Guid.NewGuid().ToString();
                SellOutMaster.SellOutSchemeHeader.Code = GetSchemeCodeBySchemeName(SchemeConstants.SO);

            }
            else
            {
                await GetSellOutMasterByUID(_commonFunctions.GetParameterValueFromURL("UID"));
            }

        }
        protected void SetEditMode()
        {
            if (SellOutMaster == null || SellOutMaster.SellOutSchemeHeader == null ||
                string.IsNullOrEmpty(SellOutMaster.SellOutSchemeHeader.FranchiseeOrgUID) || SellOutMaster.SellOutSchemeLines is null)
            {
                return;
            }
            foreach (var item in SellOutMaster.SellOutSchemeLines)
            {
                ISKU sKU = SKUV1s!.Find(p => p.UID == item.SkuUID);
                item.SkuName = sKU?.Name ?? "NA";
                item.SkuCode = sKU?.Code ?? "NA";

                item.ResonsDDL = [];
                foreach (var s in SellOutReasons)
                {
                    ISelectionItem selectionItem = new SelectionItem()
                    {
                        UID = s.UID,
                        Code = s.Code,
                        Label = s.Label,
                        IsSelected = (s.Code == item.Reason)
                    };
                    item.ResonsDDL.Add(selectionItem);
                }
            }
        }
        protected async Task SetEditModeData(string UID)
        {




            // Populate form fields with existing data
            //SetChannelPartnerSelectedonEditMode(sellOutMasterScheme.SellOutSchemeHeader.FranchiseeOrgUID);



            //Branch_P2Amount = SellOutMaster.SellOutSchemeHeader.AvailableProvision2Amount;
            //SellOutMaster.SellOutSchemeHeader.TotalCreditNote = SellOutMaster.SellOutSchemeHeader.TotalCreditNote;
            //SellOutMaster.SellOutSchemeHeader = SellOutMaster.SellOutSchemeHeader;

            //SellOutMaster.SellOutSchemeLines.Clear();

            foreach (var item in SellOutMaster.SellOutSchemeLines)
            {
                item.SkuName = SKUV1s.Find(p => p.UID == item.SkuUID)?.Name ?? "NA";

                item.ResonsDDL = [];
                foreach (var s in SellOutReasons)
                {
                    ISelectionItem selectionItem = new SelectionItem()
                    {
                        UID = s.UID,
                        Code = s.Code,
                        Label = s.Label,
                        IsSelected = (s.Code == item.Reason)
                    };
                    item.ResonsDDL.Add(selectionItem);
                }
            }

            // sellOutSchemeLineList = sellOutMasterScheme.SellOutSchemeLine;

            // Select the channel partner in the dropdown


        }
        public async Task GetSkusByOrgUID()
        {
            if (SelectedChannelPartner == null) return;

            //SKUList = await GetSKUsByOrgUID(SelectedChannelPartner.UID);
            PreviousOrdersList = await GetPreviousOrdersByOrgUID(SelectedChannelPartner.UID);
        }
        private async Task<List<IPreviousOrders>> GetPreviousOrdersByOrgUID(string UID)
        {
            return await GetPreviousOrdersFromAPI(UID);
        }
        public abstract Task<List<IPreviousOrders>> GetPreviousOrdersFromAPI(string UID);
        private bool IsProductAlreadyAdded(IPreviousOrders sku)
        {
            return SellOutMaster.SellOutSchemeLines.Any(selectedSku => selectedSku.SkuUID == sku.SKUUID);
        }
        public async Task HandleSelectedCustomers(List<IPreviousOrders> selectedItems)
        {
            //SelectedSKUs = selectedCustomers;
            //await Task.Yield();

            if (selectedItems != null && selectedItems.Count > 0)
            {
                //List<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> sKUPrices = await GetSKUPrice(sKULIst.Select(p => p.UID).ToList());
                foreach (var item in selectedItems)
                {
                    if (!IsProductAlreadyAdded(item))
                    {
                        ISellOutSchemeLine sellOutSchemeLine = _serviceProvider.CreateInstance<ISellOutSchemeLine>()!;
                        sellOutSchemeLine.SkuUID = item.SKUUID;
                        sellOutSchemeLine.SkuName = item.SKUName;
                        sellOutSchemeLine.SkuCode = item.SKUCode;
                        sellOutSchemeLine.SellOutSchemeHeaderUID = SellOutMaster.SellOutSchemeHeader.UID;
                        sellOutSchemeLine.UnitPrice = item.LastUnitPrice;
                        AddCreateFields(sellOutSchemeLine);
                        sellOutSchemeLine.ResonsDDL = [];
                        foreach (var s in SellOutReasons)
                        {
                            ISelectionItem selectionItem = new SelectionItem()
                            {
                                UID = s.UID,
                                Code = s.Code,
                                Label = s.Label,
                                IsSelected = false
                            };
                            sellOutSchemeLine.ResonsDDL.Add(selectionItem);
                        }

                        SellOutMaster.SellOutSchemeLines.Add(sellOutSchemeLine);
                    }
                }
            }
            await Task.CompletedTask;
        }
        protected void Update()
        {
            AddUpdateFields(SellOutMaster.SellOutSchemeHeader);
        }
        protected void Save()
        {
            AddCreateFields(SellOutMaster.SellOutSchemeHeader, false);
            SellOutMaster.SellOutSchemeHeader.EmpUID = _appUser.Emp.UID;
            SellOutMaster.SellOutSchemeHeader.JobPositionUID = _appUser.SelectedJobPosition.UID;
            SellOutMaster.SellOutSchemeHeader.LineCount = SellOutMaster.SellOutSchemeLines?.Count ?? 0;
            SellOutMaster.SellOutSchemeHeader.OrgUID = _appUser.SelectedJobPosition.OrgUID;
            SellOutMaster.SellOutSchemeHeader.FranchiseeOrgUID = SelectedChannelPartner!.UID;
            SellOutMaster.SellOutSchemeHeader.AvailableProvision2Amount = Branch_P2Amount;
            SellOutMaster.SellOutSchemeHeader.AvailableProvision3Amount = HO_P3Amount;
            SellOutMaster.SellOutSchemeHeader.ContributionLevel1 = SellOutMaster.SellOutSchemeHeader.ContributionLevel1;
            SellOutMaster.SellOutSchemeHeader.ContributionLevel2 = SellOutMaster.SellOutSchemeHeader.ContributionLevel2;
            //SellOutMaster.SellOutSchemeHeader.TotalCreditNote = AvailableProvisionAmount;
            SellOutMaster.SellOutSchemeHeader.StandingProvisionAmount = SellOutMaster.SellOutSchemeHeader.TotalCreditNote;
            SellOutMaster.SellOutSchemeHeader.TotalCreditNote = SellOutMaster.SellOutSchemeHeader.TotalCreditNote;
            SellOutMaster.SellOutSchemeHeader.LineCount = SellOutMaster.SellOutSchemeLines.Count();
        }

        public async Task<bool> DeleteSellOutLine()
        {
            return false;
        }

        //private async Task<bool> HandleUpdateSellOutMaster()
        //{

        //    //ISellOutMasterScheme sellOutMasterScheme = _serviceProvider.GetService<ISellOutMasterScheme>()!;
        //    //if (sellOutMasterScheme == null)
        //    //{
        //    //    throw new CustomException(ExceptionStatus.Failed, "SellOutMasterScheme service is not available.");
        //    //}
        //    //sellOutMasterScheme.SellOutSchemeHeader = sellOutSchemeHeader;
        //    //AddUpdateFields(sellOutMasterScheme.SellOutSchemeHeader);
        //    //sellOutMasterScheme.SellOutSchemeLine = sellOutSchemeLineList;
        //    //foreach (var item in sellOutMasterScheme.SellOutSchemeLine)
        //    //{
        //    //    AddUpdateFields(item);
        //    //}

        //    throw new CustomException(ExceptionStatus.Failed, "Purchase Order Saving failed...");
        //}


        private void AddCreateFields(Winit.Modules.Base.Model.IBaseModel baseModel, bool IsUIDRequired = true)
        {
            baseModel.CreatedBy = _appUser?.Emp?.UID;
            baseModel.ModifiedBy = _appUser?.Emp?.UID;
            baseModel.CreatedTime = DateTime.Now;
            baseModel.ModifiedTime = DateTime.Now;
            baseModel.SS = 0;
            if (IsUIDRequired) baseModel.UID = Guid.NewGuid().ToString();
        }
        private void AddUpdateFields(Winit.Modules.Base.Model.IBaseModel baseModel)
        {
            baseModel.ModifiedBy = _appUser?.Emp?.UID;
            baseModel.ModifiedTime = DateTime.Now;
        }
        public Task SaveDataAsync(ISellOutSchemeHeaderItem item)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region API Calling




        #endregion


        public abstract Task ApproveSellOutSchemeHeaderItemAsync(string uid);

        protected abstract Task GetSellOutMasterByUID(string uid);

        protected abstract Task GetSellOutReasons(string ListHeaderUID);
        public abstract Task<List<ISKUMaster>> GetSKUsMasterBySKUUIDs(SKUMasterRequest sKUMasterRequest);


    }
}
