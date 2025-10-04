using Microsoft.Extensions.DependencyInjection;
using Winit.Modules.ApprovalEngine.BL.Interfaces;
using Winit.Modules.Base.BL;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.Services;
using Winit.UIComponents.SnackBar;

namespace Winit.Modules.Scheme.BL.Classes
{
    public abstract class SalesPromotionSchemeBaseViewModel : SchemeViewModelBase, ISalesPromotionSchemeViewModel
    {

        //variables
        public Winit.Modules.Scheme.Model.Interfaces.ISalesPromotionScheme SalesPromotion { get; set; }
        public List<Winit.Modules.Scheme.Model.Interfaces.ISalesPromotionScheme> SalesPromotions { get; set; }
        public List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> ApprovedFiles { get; set; } = [];
        public List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> SalesPromotionFiles { get; set; } = [];
        public bool ShowPOTab { get; set; }
        //public bool IsEditMode { get; set; }
        public bool IsViewMode { get; set; }

        public string PromotionUID { get; set; }
        public string AttachedDocument { get; set; }
        public List<ISelectionItem> ActivityType { get; set; } = [];
        // Dependency Injection
        public readonly IServiceProvider _serviceProvider;
        public readonly IFilterHelper _filter;
        public readonly ISortHelper _sorter;
        public readonly IListHelper _listHelper;
        public readonly IAppUser _appUser;


        protected Winit.Shared.Models.Common.PagingRequest PagingRequest = new Winit.Shared.Models.Common.PagingRequest()
        {
            FilterCriterias = [new FilterCriteria("status", new List<string> { SchemeConstants.Pending_Execution, SchemeConstants.Approved, SchemeConstants.Executed }, FilterType.In)]
        };
        // Constructor
        public SalesPromotionSchemeBaseViewModel(
            IAppConfig appConfig,
            ApiService apiService,
            IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper,
            IAppUser appUser,
            IAppSetting appSetting,
            IDataManager dataManager, ILoadingService loadingService, IAlertService alertService, CommonFunctions commonFunctions, IAddProductPopUpDataHelper addProductPopUpDataHelper,
            IToast toast
        ) : base(appConfig, apiService, serviceProvider, appUser, loadingService, alertService, appSetting, commonFunctions, addProductPopUpDataHelper, toast)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _appUser = appUser;


            // Initialize the SalesPromotion property
            SalesPromotion = _serviceProvider.GetRequiredService<ISalesPromotionScheme>()!;
        }
        /// <summary>
        /// Create Sales Promotion
        /// </summary>
        /// <returns></returns>
        public async Task PopulateViewModel()
        {

            IsNew = PageType.New.Equals(_commonFunctions.GetParameterValueFromURL(PageType.Page));

            await GetAllChannelPartner();

            if (IsNew)
            {
                SalesPromotion.Code = GetSchemeCodeBySchemeName(SchemeConstants.SP);
                SalesPromotion.ToDate = DateTime.Now;
                SalesPromotion.FromDate = DateTime.Now;
                PromotionUID = Guid.NewGuid().ToString();
                SalesPromotion.IsPOHandledByDMS = true;
            }
            else
            {
                PromotionUID = _commonFunctions.GetParameterValueFromURL("UID");
                await LoadSalesPromotion();
                await PopulateApprovalEngine(PromotionUID);
            }
            await GetListItem();
        }
        /// <summary>
        /// Excecution Of Sales Promotion
        /// </summary>
        /// <returns></returns>
        public async Task PopulateViewModelForExecuteSales()
        {
            await GetAllChannelPartner();
            await GetAllSalesPromotions();
            await GetListItem();

        }
        public async Task SetEditModeOnClick()
        {
            FileSysList = await GetAttatchedFiles(PromotionUID);
            SetEditMode();
        }
        protected void AddCreateFields(Winit.Modules.Base.Model.IBaseModel baseModel, bool IsUIDRequired = true)
        {
            baseModel.CreatedBy = _appUser?.Emp?.UID;
            baseModel.ModifiedBy = _appUser?.Emp?.UID;
            baseModel.CreatedTime = DateTime.Now;
            baseModel.ModifiedTime = DateTime.Now;
            baseModel.SS = 0;
            //  if (IsUIDRequired) baseModel.UID = Guid.NewGuid().ToString();
        }
        protected void AddUpdateFields(Winit.Modules.Base.Model.IBaseModel baseModel)
        {
            baseModel.ModifiedBy = _appUser?.Emp?.UID;
            baseModel.ModifiedTime = DateTime.Now;
            baseModel.SS = 0;
        }
        protected void SetActivityType(List<Winit.Modules.ListHeader.Model.Interfaces.IListItem> listItems)
        {
            ActivityType.Clear();

            listItems.ForEach(item =>
            {
                ActivityType.Add(new SelectionItem
                {
                    UID = item.UID,
                    Code = item.Code,
                    Label = item.Name,
                    IsSelected = SalesPromotion.ActivityType == item.Code
                });
            });
        }


        public void OnActivityTypeChanged(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null)
            {
                ISelectionItem? selectionItem = dropDownEvent.SelectionItems?.FirstOrDefault();
                if (selectionItem == null)
                {
                    SalesPromotion.ActivityType = string.Empty;
                }
                else
                {
                    SalesPromotion.ActivityType = selectionItem.Code;
                }
            }
        }
        protected void SetEditMode()
        {
            SetChannelPartnerSelectedonEditMode(SalesPromotion.FranchiseeOrgUID);
            if (FileSysList != null && FileSysList.Count > 0)
            {
                SalesPromotionFiles.Clear();
                ApprovedFiles.Clear();

                SalesPromotionFiles.AddRange(FileSysList.FindAll(p => p.FileSysType == SchemeConstants.SalesPromotion));
                ApprovedFiles.AddRange(FileSysList.FindAll(p => p.FileSysType == SchemeConstants.ApprovedFilesysType));
            }
            foreach (ISelectionItem selectionItem in ChannelPartner)
            {
                if (selectionItem.UID == SalesPromotion?.FranchiseeOrgUID)
                {
                    selectionItem.IsSelected = true;
                }
            }
            if (SalesPromotion!.Status == SchemeConstants.Approved)
            {
                ShowPOTab = true;
                IsViewMode = true;
            }
        }
        public abstract Task<bool> CreateSalesPromotion();
        public abstract Task<bool> UpdateSalesPromotion(SalesPromotionSchemeApprovalDTO salesPromotionSchemeApprovalDTO);
        public abstract Task DeleteSalesPromotion();
        protected abstract Task GetAllSalesPromotions();
        protected abstract Task LoadSalesPromotion();
        public abstract Task<bool> ApproveSalesPromotion(SalesPromotionSchemeApprovalDTO salesPromotionSchemeApprovalDTO);
        public abstract Task<bool> RejectSalesPromotion(SalesPromotionSchemeApprovalDTO salesPromotionSchemeApprovalDTO);
        public abstract Task GetListItem();


    }
}
