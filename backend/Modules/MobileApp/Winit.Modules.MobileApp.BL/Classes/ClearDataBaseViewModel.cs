using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Mobile.BL.Interfaces;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Common;
using Winit.Modules.Mobile.Model.Interfaces;
using Newtonsoft.Json;
using Winit.Modules.Mobile.Model.Interfaces;
using Winit.Modules.Mobile.Model.Classes;
using Winit.Modules.Base.Model;
using Winit.Modules.Emp.BL.Interfaces;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.Common.BL;
using System.Reflection;
using Nest;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.BL.Classes;
using Winit.Modules.Emp.DL.Interfaces;
using Winit.Shared.CommonUtilities.Common;

namespace Winit.Modules.Mobile.BL.Classes
{
    public abstract class ClearDataBaseViewModel : Winit.Modules.Mobile.BL.Interfaces.IClearDataViewModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        public List<IMobileAppAction> ClearDataLists { get; set; }
        public IMobileAppAction MobileAppAction { get; set; }
        public List<IUser> SalesRep = new List<IUser>();
        public List<ISelectionItem> EmpSalesRepSelectionItems { get; set; }
        public List<ISelectionItem> ActionSelectionItems { get; set; }
        public List<FilterCriteria> MobileAppActionFilterCriterials { get; set; }
        public List<ISelectionItem> EmpSelectionList { get; set; }
        public List<SortCriteria> SortCriterias { get; set; }

        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private Winit.Modules.Common.BL.Interfaces.IAppUser _appUser;
        private SelectionManager EmpSalesRepSM { get; set; }
        private SelectionManager ActionSM { get; set; }
        public ClearDataBaseViewModel(IServiceProvider serviceProvider,
              IFilterHelper filter,
              ISortHelper sorter,
              IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService, Winit.Modules.Common.BL.Interfaces.IAppUser appUser)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _appUser = appUser;
            ClearDataLists = new List<IMobileAppAction>();
            ActionSelectionItems = new List<ISelectionItem>();           
             EmpSalesRepSelectionItems = new List<ISelectionItem>();
            MobileAppActionFilterCriterials = new List<FilterCriteria>();
            EmpSelectionList = new List<ISelectionItem>();
            SortCriterias = new List<SortCriteria>();
        }
        public async Task ApplyFilter(List<Shared.Models.Enums.FilterCriteria> filterCriterias)
        {
            MobileAppActionFilterCriterials.Clear();
            MobileAppActionFilterCriterials.AddRange(filterCriterias);
            await PopulateViewModel();
        }
        public async Task ApplySort(SortCriteria sortCriteria)
        {
            SortCriterias.Clear();
            SortCriterias.Add(sortCriteria);
            await PopulateViewModel();
        }
        public async Task GetSalesman(string OrgUID)
        {
            EmpSelectionList.Clear();
            EmpSelectionList.AddRange(await GetSalesmanData(OrgUID));
        }
        public  async virtual Task PopulateViewModel()
        {
            await PopulateClearData();
        }
        public async virtual Task PopulateViewModelForDD()
        {
            
            SalesRep = await GetSalesRepDropdownData();
        }
        protected async Task PopulateClearData()
        {
            ClearDataLists = await GetClearData();
        }
        public async Task PageIndexChanged(int pageNumber)
        {
            PageNumber = pageNumber;
            await PopulateViewModel();
        }
        #region Business Logic       
        public async Task SaveClearData()
        {           
            SelectionManager selectionManager = new SelectionManager(EmpSalesRepSelectionItems, SelectionMode.Multiple);
            List<ISelectionItem> selectedItems = selectionManager.GetSelectedSelectionItems();
            ISelectionItem itemselected = ActionSelectionItems.Find(e => e.IsSelected);
            List<IMobileAppAction> mobileAppActions = new List<IMobileAppAction>();
            foreach (var item in selectedItems)
            {
                IMobileAppAction mobileApp = new MobileAppAction();
                mobileApp.EmpUID = item.UID;
                mobileApp.Action = itemselected.UID;
                mobileApp.ActionDate = DateTime.Now;
                mobileApp.OrgUID = _appUser.SelectedJobPosition.OrgUID;
                mobileAppActions.Add(mobileApp);
            }
            mobileAppActions.ForEach(e => AddCreateFields(e, true));
            await SaveClearData_Data(mobileAppActions);          
        }      
        public void AddCreateFields(IBaseModel baseModel, bool IsUIDRequired)
        {
            baseModel.CreatedBy = _appUser.Emp.UID;
            baseModel.ModifiedBy = _appUser.Emp.UID;
            baseModel.CreatedTime = DateTime.Now;
            baseModel.ModifiedTime = DateTime.Now;
            if (IsUIDRequired) baseModel.UID = Guid.NewGuid().ToString();
        }
        public void OnCancelFromPopUp()
        {
            EmpSalesRepSM?.DeselectAll();
            ActionSM?.DeselectAll();
        }
        #endregion
        #region Database or Services Methods
        public abstract Task<List<Winit.Modules.Mobile.Model.Interfaces.IMobileAppAction>> GetClearData();
        public abstract Task<bool> SaveClearData_Data(List<IMobileAppAction> mobileAppAction);
        public abstract Task<List<Winit.Modules.Mobile.Model.Interfaces.IUser>> GetSalesRepDropdownData();
        public abstract Task<List<ISelectionItem>> GetSalesmanData(string OrgUID);

        #endregion

    }
}
