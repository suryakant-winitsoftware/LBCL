using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Constants;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.Services;
using Winit.UIComponents.SnackBar;

namespace Winit.Modules.Scheme.BL.Classes
{
    public abstract class CreateStandingConfigurationBaseViewModel : SchemeViewModelBase, ICreateStandingConfigurationViewModel
    {
        public CreateStandingConfigurationBaseViewModel(IAppConfig appConfig, ApiService apiService,
            IServiceProvider serviceProvider, IAppUser appUser, ILoadingService loadingService, IAlertService alertService,
            IAppSetting appSetting, CommonFunctions commonFunctions, IAddProductPopUpDataHelper addProductPopUpDataHelper, IToast toast) :
            base(appConfig, apiService, serviceProvider, appUser, loadingService, alertService, appSetting, commonFunctions, addProductPopUpDataHelper, toast)
        {


        }
        public List<ISelectionItem> CategoryDDL { get; set; } = [];
        public List<ISelectionItem> TypeDDL { get; set; } = [];
        public List<ISelectionItem> StarRatingDDL { get; set; } = [];
        public List<ISelectionItem> TonnageDDL { get; set; } = [];
        public List<ISelectionItem> ProductDivisions { get; set; } = [];
        public List<ISelectionItem> CapacityDDL { get; set; } = [];
        public List<ISelectionItem> ProductGroupDDL { get; set; } = [];
        public List<ISelectionItem> SeriesDDL { get; set; } = [];
        public IStandingProvisionSchemeMaster StandingProvisionSchemeMaster { get; set; }
        public List<ISKUV1> SKUList { get; set; }
        public List<ISKUV1> ExcludedModels { get; set; }
        public bool IsDisable { get; set; }
        protected bool IsClone { get; set; }

        public async Task PopulateViewModel()
        {
            _loadingService.ShowLoading();
            // List<string>? orgs = await GetOrgHierarchyParentUIDsByOrgUID(_appUser.SelectedJobPosition.OrgUID);//[SelectedChannelPartner.UID];
            List<Task> tasks = new List<Task>();
            tasks.Add(PopulateApplicableToCustomersAndSKU());
            tasks.Add(GetSKUGroup());
            tasks.Add(GetSKUGroupType());
            IsNew = PageType.New.Equals(_commonFunctions.GetParameterValueFromURL(PageType.Page));
            if (IsNew)
            {
                StandingProvisionSchemeMaster.StandingProvisionScheme.UID = Guid.NewGuid().ToString();
                StandingProvisionSchemeMaster.StandingProvisionScheme.Code = GetSchemeCodeBySchemeName(SchemeConstants.S);
                StandingProvisionSchemeMaster.StandingProvisionScheme.CreatedByEmpName = _appUser.Emp.Name;
                StandingProvisionSchemeMaster.StandingProvisionScheme.CreatedTime = DateTime.Now.Date;
            }
            else
            {
                IsClone = PageType.Clone.Equals(_commonFunctions.GetParameterValueFromURL(PageType.Page));
                string UID = _commonFunctions.GetParameterValueFromURL("UID");
                tasks.Add(GetStandardProvisionMasterBYUID(UID));
                tasks.Add(PopulateApprovalEngine(UID));
                if (IsClone)
                {
                    SetCloneProvisionMaster();
                    IsNew = true;
                }
                else
                {
                    IsDisable = true;
                }
            }
            await Task.WhenAll(tasks);
            if (!IsNew)
            {
                await SetEditMode();
            }
            //SetEditModeForChannelPartner();
            PopulateDropDowns();
            IsIntialize = true;
            _loadingService.HideLoading();
        }

        #region set drop downs
        protected void PopulateDropDowns()
        {

            ProductDivisions.Clear();
            ProductDivisions.AddRange(_appUser!.ProductDivisionSelectionItems ?? []);
            ProductDivisions.ForEach(p => p.IsSelected = StandingProvisionSchemeMaster?.StandingProvisionSchemeDivisions?.Any(q => q.DivisionOrgUID == p.UID) ?? false);

            string categoryUID = SKUGroupType.Find(p => p.Code == SKUGroupTypeContants.Category)?.UID ?? string.Empty;
            string typeUID = SKUGroupType.Find(p => p.Code == SKUGroupTypeContants.Product_Type)?.UID ?? string.Empty;
            string starRatingUID = SKUGroupType.Find(p => p.Code == SKUGroupTypeContants.StarRating)?.UID ?? string.Empty;
            string tonnageUID = SKUGroupType.Find(p => p.Code == SKUGroupTypeContants.TONAGE)?.UID ?? string.Empty;
            string itemSeriesUID = SKUGroupType.Find(p => p.Code == SKUGroupTypeContants.Item_Series)?.UID ?? string.Empty;
            string capacityUID = SKUGroupType.Find(p => p.Code == SKUGroupTypeContants.Capacity)?.UID ?? string.Empty;
            string groupUID = SKUGroupType.Find(p => p.Code == SKUGroupTypeContants.ProductGroup)?.UID ?? string.Empty;

            CategoryDDL.Clear();
            TypeDDL.Clear();
            StarRatingDDL.Clear();
            TonnageDDL.Clear();
            SeriesDDL.Clear();
            CapacityDDL.Clear();
            ProductGroupDDL.Clear();
            foreach (var item in SKUGroup)
            {
                ISelectionItem selectionItem = _serviceProvider.CreateInstance<ISelectionItem>();

                selectionItem.UID = item.UID;
                selectionItem.Label = item.Name;
                selectionItem.Code = item.Code;

                if (item.SKUGroupTypeUID.Equals(categoryUID))
                {
                    selectionItem.IsSelected = (item.Code == StandingProvisionSchemeMaster?.StandingProvisionScheme?.SKUCategoryCode);
                    CategoryDDL.Add(selectionItem);
                }
                else if (item.SKUGroupTypeUID.Equals(typeUID))
                {
                    selectionItem.IsSelected = (item.Code == StandingProvisionSchemeMaster?.StandingProvisionScheme?.SKUTypeCode);
                    TypeDDL.Add(selectionItem);
                }
                else if (item.SKUGroupTypeUID.Equals(starRatingUID))
                {
                    selectionItem.IsSelected = (item.Code == StandingProvisionSchemeMaster?.StandingProvisionScheme?.StarRatingCode);
                    StarRatingDDL.Add(selectionItem);
                }
                else if (item.SKUGroupTypeUID.Equals(tonnageUID))
                {
                    selectionItem.IsSelected = (item.Code == StandingProvisionSchemeMaster?.StandingProvisionScheme?.SKUTonnageCode);
                    TonnageDDL.Add(selectionItem);
                }
                else if (item.SKUGroupTypeUID.Equals(itemSeriesUID))
                {
                    selectionItem.IsSelected = (item.Code == StandingProvisionSchemeMaster?.StandingProvisionScheme?.SKUSeriesCode);
                    SeriesDDL.Add(selectionItem);
                }
                else if (item.SKUGroupTypeUID.Equals(capacityUID))
                {
                    selectionItem.IsSelected = (item.Code == StandingProvisionSchemeMaster?.StandingProvisionScheme?.StarCapacityCode);
                    CapacityDDL.Add(selectionItem);
                }
                else if (item.SKUGroupTypeUID.Equals(groupUID))
                {
                    selectionItem.IsSelected = (item.Code == StandingProvisionSchemeMaster?.StandingProvisionScheme?.SKUProductGroup);
                    ProductGroupDDL.Add(selectionItem);
                }
            }
        }
        #endregion

        #region UI Logic


        public void OnCatogorySelected(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
                {
                    StandingProvisionSchemeMaster.StandingProvisionScheme.SKUCategoryCode = dropDownEvent.SelectionItems.FirstOrDefault().Code;
                }
                else
                {
                    StandingProvisionSchemeMaster.StandingProvisionScheme.SKUCategoryCode = string.Empty;
                }
            }
        }
        public void OnTypeSelected(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
                {
                    StandingProvisionSchemeMaster.StandingProvisionScheme.SKUTypeCode = dropDownEvent.SelectionItems.FirstOrDefault().Code;
                }
                else
                {
                    StandingProvisionSchemeMaster.StandingProvisionScheme.SKUTypeCode = string.Empty;
                }
            }
        }
        public void OnStarRatingSelected(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
                {
                    StandingProvisionSchemeMaster.StandingProvisionScheme.StarRatingCode = dropDownEvent.SelectionItems.FirstOrDefault().Code;
                }
                else
                {
                    StandingProvisionSchemeMaster.StandingProvisionScheme.StarRatingCode = string.Empty;
                }
            }
        }
        public void OnTonnageSelected(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
                {
                    StandingProvisionSchemeMaster.StandingProvisionScheme.SKUTonnageCode = dropDownEvent.SelectionItems.FirstOrDefault().Code;
                }
                else
                {
                    StandingProvisionSchemeMaster.StandingProvisionScheme.SKUTonnageCode = string.Empty;
                }
            }
        }
        public void OnItemSeriesSelected(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
                {
                    StandingProvisionSchemeMaster.StandingProvisionScheme.SKUSeriesCode = dropDownEvent!.SelectionItems!.FirstOrDefault()!.Code;
                }
                else
                {
                    StandingProvisionSchemeMaster.StandingProvisionScheme.SKUSeriesCode = string.Empty;
                }
            }
        }
        public void OnCapacitySelected(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
                {
                    StandingProvisionSchemeMaster.StandingProvisionScheme.StarCapacityCode = dropDownEvent.SelectionItems.FirstOrDefault().Code;
                }
                else
                {
                    StandingProvisionSchemeMaster.StandingProvisionScheme.StarCapacityCode = string.Empty;
                }
            }
        }
        public void OnGroupSelected(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
                {
                    StandingProvisionSchemeMaster.StandingProvisionScheme.SKUProductGroup = dropDownEvent.SelectionItems.FirstOrDefault().Code;
                }
                else
                {
                    StandingProvisionSchemeMaster.StandingProvisionScheme.SKUProductGroup = string.Empty;
                }
            }
        }

        public void OnDivisionSelected(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null)
            {
                StandingProvisionSchemeMaster.StandingProvisionSchemeDivisions.Clear();
                if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
                {
                    foreach (ISelectionItem selectionItem in dropDownEvent.SelectionItems)
                    {
                        IStandingProvisionSchemeDivision standingProvisionSchemeDivision = _serviceProvider.CreateInstance<IStandingProvisionSchemeDivision>();

                        standingProvisionSchemeDivision.UID = Guid.NewGuid().ToString();
                        standingProvisionSchemeDivision.DivisionOrgUID = selectionItem.UID;
                        standingProvisionSchemeDivision.StandingProvisionUID = StandingProvisionSchemeMaster.StandingProvisionScheme.UID;
                        standingProvisionSchemeDivision.CreatedBy = _appUser.Emp.UID;
                        standingProvisionSchemeDivision.ModifiedBy = _appUser.Emp.UID;
                        standingProvisionSchemeDivision.CreatedTime = DateTime.Now;
                        standingProvisionSchemeDivision.ModifiedTime = DateTime.Now;

                        StandingProvisionSchemeMaster.StandingProvisionSchemeDivisions.Add(standingProvisionSchemeDivision);
                    }
                }
            }
        }

        public async Task GetProductsOnAddButtonClick()
        {

            List<FilterCriteria> filters = new List<FilterCriteria>()
            {
                new FilterCriteria("OrgUID",_appUser.OrgUIDs,FilterType.In),
            };
            if (!string.IsNullOrEmpty(StandingProvisionSchemeMaster.StandingProvisionScheme.SKUCategoryCode))
            {
                string[] cat = [$"{StandingProvisionSchemeMaster.StandingProvisionScheme.SKUCategoryCode}_{SKUGroupTypeContants.Category}"];
                filters.Add(new FilterCriteria(SKUGroupTypeContants.Category, cat, FilterType.In));
            }
            if (!string.IsNullOrEmpty(StandingProvisionSchemeMaster.StandingProvisionScheme.SKUTypeCode))
            {
                string[] cat = [$"{StandingProvisionSchemeMaster.StandingProvisionScheme.SKUTypeCode}_{SKUGroupTypeContants.Product_Type}"];
                filters.Add(new FilterCriteria(SKUGroupTypeContants.Product_Type, cat, FilterType.In));
            }
            if (!string.IsNullOrEmpty(StandingProvisionSchemeMaster.StandingProvisionScheme.StarRatingCode))
            {
                string[] cat = [$"{StandingProvisionSchemeMaster.StandingProvisionScheme.StarRatingCode}_{SKUGroupTypeContants.StarRating}"];
                filters.Add(new FilterCriteria(SKUGroupTypeContants.StarRating, cat, FilterType.In));
            }
            if (!string.IsNullOrEmpty(StandingProvisionSchemeMaster.StandingProvisionScheme.SKUTonnageCode))
            {
                string[] cat = [$"{StandingProvisionSchemeMaster.StandingProvisionScheme.SKUTonnageCode}_{SKUGroupTypeContants.TONAGE}"];
                filters.Add(new FilterCriteria(SKUGroupTypeContants.TONAGE, cat, FilterType.In));
            }
            if (!string.IsNullOrEmpty(StandingProvisionSchemeMaster.StandingProvisionScheme.SKUProductGroup))
            {
                string[] cat = [$"{StandingProvisionSchemeMaster.StandingProvisionScheme.SKUProductGroup}_{SKUGroupTypeContants.ProductGroup}"];
                filters.Add(new FilterCriteria(SKUGroupTypeContants.ProductGroup, cat, FilterType.In));
            }
            if (StandingProvisionSchemeMaster.StandingProvisionSchemeDivisions.Any())
            {
                filters.Add(new FilterCriteria("SupplierOrgUIDs", StandingProvisionSchemeMaster.StandingProvisionSchemeDivisions.Select(p => p.DivisionOrgUID).ToList(), FilterType.In));
            }
            SKUV1s = await GetAllSKUs(new PagingRequest() { FilterCriterias = filters });
        }
        public void GetSelectedItems(List<ISKUV1> sKUs)
        {
            sKUs.ForEach(s =>
            {
                if (!ExcludedModels.Any(p => p.UID == s.UID))
                {
                    ExcludedModels.Add(s);
                }
            });
        }
        protected async Task SetEditMode()
        {
            SetEditModeForApplicabletoCustomers(StandingProvisionSchemeMaster.SchemeBranches,
                StandingProvisionSchemeMaster.SchemeOrgs, StandingProvisionSchemeMaster.SchemeBroadClassifications);
            await SetExcludedModels();
        }
        protected async Task SetExcludedModels()
        {
            try
            {
                if (StandingProvisionSchemeMaster == null || StandingProvisionSchemeMaster.StandingProvisionScheme == null || string.IsNullOrEmpty(StandingProvisionSchemeMaster.StandingProvisionScheme.ExcludedModels))
                {
                    ExcludedModels = new();
                }
                else
                {
                    string[]? excluded = StandingProvisionSchemeMaster.StandingProvisionScheme.ExcludedModels?.Split(",");
                    if (excluded != null && excluded.Length > 0)
                    {
                        PagingRequest pagingRequest = new PagingRequest()
                        {
                            FilterCriterias = new() { new("uid", excluded, FilterType.In) },
                        };
                        ExcludedModels = await GetAllSKUs(pagingRequest);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region Validation
        protected void ValidateDetails(out string message, out bool isVal)
        {
            message = string.Empty;
            isVal = true;
            if (string.IsNullOrEmpty(StandingProvisionSchemeMaster.StandingProvisionScheme.Description))
            {
                message += "Description,";
                isVal = false;
            }
            if (StandingProvisionSchemeMaster.StandingProvisionScheme.Amount == 0)
            {
                message += "Amount,";
                isVal = false;
            }
            if (StandingProvisionSchemeMaster.StandingProvisionScheme.StartDate == null || StandingProvisionSchemeMaster.StandingProvisionScheme.StartDate == default)
            {
                message += "Start Date,";
                isVal = false;
            }
            if (StandingProvisionSchemeMaster.StandingProvisionScheme.EndDate == null || StandingProvisionSchemeMaster.StandingProvisionScheme.EndDate == default)
            {
                message += "End Date ";
                isVal = false;
            }
            if (isVal)
            {
                if (StandingProvisionSchemeMaster.StandingProvisionScheme.EndDate <= StandingProvisionSchemeMaster.StandingProvisionScheme.StartDate)
                {
                    message = "End Date should not be less than or equal to the Start Date";
                    isVal = false;
                }
            }
            else
            {
                message = $"The following field(s) shouldn't be empty {message.Substring(0, message.Length - 1)}";
            }
        }
        protected void ValidateProductType(out string message, out bool isVal)
        {

            message = string.Empty;
            isVal = true;
            bool isProductsValidated = !(string.IsNullOrEmpty(StandingProvisionSchemeMaster.StandingProvisionScheme.SKUTypeCode) &&
                string.IsNullOrEmpty(StandingProvisionSchemeMaster.StandingProvisionScheme.SKUCategoryCode) &&
                string.IsNullOrEmpty(StandingProvisionSchemeMaster.StandingProvisionScheme.StarRatingCode) &&
                string.IsNullOrEmpty(StandingProvisionSchemeMaster.StandingProvisionScheme.SKUSeriesCode) &&
                string.IsNullOrEmpty(StandingProvisionSchemeMaster.StandingProvisionScheme.StarCapacityCode) &&
                string.IsNullOrEmpty(StandingProvisionSchemeMaster.StandingProvisionScheme.SKUProductGroup) &&
                string.IsNullOrEmpty(StandingProvisionSchemeMaster.StandingProvisionScheme.SKUTonnageCode));
            if (!isProductsValidated && StandingProvisionSchemeMaster.StandingProvisionSchemeDivisions.Count == 0)
            {

                message = "Please select Division and one other in applicable to products";
                isVal = false;
            }
            else if (!isProductsValidated)
            {
                message = "Please select any one other in applicable to products along with Division";
                isVal = false;
            }
            else if (StandingProvisionSchemeMaster.StandingProvisionSchemeDivisions.Count == 0)
            {
                message = "Please select  Division";
                isVal = false;
            }
        }
        protected void ValidateApplicableToProducts(out string message, out bool isVal)
        {
            message = string.Empty;
            isVal = true;
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
            if (isOrg || isBC || isBranch)
            {

            }
            else
            {
                isVal = false;
                message = "Please select any one in applicable to customer";
            }

            //if (SelectedBranches.Count == 0 &&
            //    SelectedCP.Count == 0 &&
            //    SelectedBC.Count == 0)
            //{
            //    message = "Please select any one in applicable to customer";
            //    isVal = false;
            //}
        }
        #endregion

        #region Saving and Setting View Mode
        protected void SetCloneProvisionMaster()
        {
            StandingProvisionSchemeMaster.StandingProvisionScheme.UID = Guid.NewGuid().ToString();
            StandingProvisionSchemeMaster.StandingProvisionScheme.CreatedBy = _appUser.Emp.UID;
            StandingProvisionSchemeMaster.StandingProvisionScheme.CreatedTime = DateTime.Now;
            StandingProvisionSchemeMaster.StandingProvisionScheme.Code = GetSchemeCodeBySchemeName(SchemeConstants.S);
            StandingProvisionSchemeMaster.SchemeBranches.ForEach(branch =>
            {
                branch.CreatedBy = _appUser.Emp.UID;
                branch.CreatedTime = DateTime.Now;
                branch.UID = Guid.NewGuid().ToString();
                branch.LinkedItemUID = StandingProvisionSchemeMaster.StandingProvisionScheme.UID;
            });

            StandingProvisionSchemeMaster.SchemeBroadClassifications.ForEach(BC =>
            {
                BC.CreatedBy = _appUser.Emp.UID;
                BC.CreatedTime = DateTime.Now;
                BC.UID = Guid.NewGuid().ToString();
                BC.LinkedItemUID = StandingProvisionSchemeMaster.StandingProvisionScheme.UID;
            });

            StandingProvisionSchemeMaster.SchemeOrgs.ForEach(AO =>
            {
                AO.CreatedBy = _appUser.Emp.UID;
                AO.CreatedTime = DateTime.Now;
                AO.UID = Guid.NewGuid().ToString();
                AO.LinkedItemUID = StandingProvisionSchemeMaster.StandingProvisionScheme.UID;
            });

            StandingProvisionSchemeMaster.StandingProvisionSchemeDivisions.ForEach(Divs =>
            {
                Divs.CreatedBy = _appUser.Emp.UID;
                Divs.CreatedTime = DateTime.Now;
                Divs.UID = Guid.NewGuid().ToString();
                Divs.StandingProvisionUID = StandingProvisionSchemeMaster.StandingProvisionScheme.UID;
            });
        }
        protected void PrepareProvisionMaster()
        {
            if (IsNew)
            {
                StandingProvisionSchemeMaster.StandingProvisionScheme.CreatedBy = _appUser?.Emp.UID;
                StandingProvisionSchemeMaster.StandingProvisionScheme.CreatedTime = DateTime.Now;
                StandingProvisionSchemeMaster.StandingProvisionScheme.SS = 0;
                StandingProvisionSchemeMaster.StandingProvisionScheme.Status = SchemeConstants.Pending;
                StandingProvisionSchemeMaster.StandingProvisionScheme.OrgUID = _appUser!.SelectedJobPosition.OrgUID;
                StandingProvisionSchemeMaster.StandingProvisionScheme.JobPositionUID = _appUser.SelectedJobPosition.UID;
                PrePareApplicabletoCustomers(StandingProvisionSchemeMaster.StandingProvisionScheme.UID, SchemeConstants.StandingProvision);

                StandingProvisionSchemeMaster.SchemeBranches = SchemeBranches;
                StandingProvisionSchemeMaster.SchemeOrgs = SchemeOrgs;
                StandingProvisionSchemeMaster.SchemeBroadClassifications = SchemeBroadClassifications;
            }
            StandingProvisionSchemeMaster.StandingProvisionScheme.ModifiedBy = _appUser?.Emp.UID;
            StandingProvisionSchemeMaster.StandingProvisionScheme.ModifiedTime = DateTime.Now;
            StandingProvisionSchemeMaster.StandingProvisionScheme.ExcludedModels = string.Join(",", ExcludedModels.Select(x => x.UID).ToList());

            //StandingProvisionSchemeMaster.StandingProvisionScheme.ExcludedModels = JsonConvert.SerializeObject(strings);
        }
        //protected void SetEditModeForChannelPartner()
        //{
        //    foreach (ISelectionItem selectionItem in ChannelPartner)
        //    {
        //        selectionItem.IsSelected = StandingProvisionSchemeMaster.SchemeOrgs?.Any(p => p.OrgUID == selectionItem.UID) ?? false;
        //    }
        //}

        #endregion

        #region abstract classes
        protected abstract Task GetStandardProvisionMasterBYUID(string UID);

        #endregion
    }
}
