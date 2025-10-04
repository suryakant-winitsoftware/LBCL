using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Base.Model;
using Winit.Modules.Common.BL.Classes;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Org.BL.Interfaces;
using Winit.Modules.Org.Model.Classes;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Org.BL.Classes
{
    public abstract class AddEditMaintainWarehouseBaseViewModel : IAddEditMaintainWarehouseViewModel
    {
        public IWarehouseItemView warehouseitemView { get; set; }
        public IOrgType WarehouseTypeDD { get; set; }
        public IEditWareHouseItemView WareHouseEditItemView { get; set; }
        public string Edit_ViewWarehouseuid { get; set; }
        public List<ISelectionItem> WareHouseTypeSelectionItems { get; set; }
        public List<IOrgType> WareHouseTypeDDList = new List<IOrgType>();
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private readonly IAppUser _appUser;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        public AddEditMaintainWarehouseBaseViewModel(IServiceProvider serviceProvider,
              IFilterHelper filter,
              ISortHelper sorter,
               IAppUser appUser,
              IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService
            )
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _appUser = appUser;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            WareHouseTypeSelectionItems = new List<ISelectionItem>();
            warehouseitemView = new WarehouseItemView();
            WareHouseEditItemView = new Winit.Modules.Org.Model.Classes.EditWareHouseItemView();
        }
        public async virtual Task PopulateViewModel()
        {
            //WareHouseTypeDDList = await GetWarehouseTypeDropdown();
            //string ORG_type = "WH";
            //if (_appUser.Emp.Code == "Admin"|| _appUser.Emp.Code == "BM1")
            //{
            //    WareHouseTypeDDList = WareHouseTypeDDList.Where(p => p.UID == ORG_type).ToList();
            //}
            //else
            //{
            //    WareHouseTypeDDList = WareHouseTypeDDList.Where(p => p.UID != ORG_type).ToList();
            //}

            try
            {
                WareHouseTypeDDList = await GetWarehouseTypeDropdown() ?? new List<IOrgType>();

                if (WareHouseTypeDDList != null && WareHouseTypeDDList.Any())
                {
                    string ORG_type = "WH";
                    if (_appUser.Emp.Code == "Admin" || _appUser.Emp.Code == "BM1")
                    {
                        WareHouseTypeDDList = WareHouseTypeDDList.Where(p => p.UID == ORG_type).ToList();
                    }
                    else
                    {
                        WareHouseTypeDDList = WareHouseTypeDDList.Where(p => p.UID != ORG_type).ToList();
                    }
                }
                else
                {
                    WareHouseTypeDDList = new List<IOrgType>();
                }
            }
            catch (Exception)
            {
                WareHouseTypeDDList = new List<IOrgType>();
            }
        }


        public async Task PopulateMaintainWarehouseEditDetails(string viewwarehouseuid)
        {
            WareHouseEditItemView = await GetMaintainWarehouseEditDetails(viewwarehouseuid);
            if (WareHouseEditItemView == null)
            {
                WareHouseEditItemView=new EditWareHouseItemView();
            }

        }
        #region Business Logic 
        public async Task SetEditForOrgTypeDD(IEditWareHouseItemView orgtype)
        {
            var selectedWarehouseType = WareHouseTypeSelectionItems?.Find(e => e.UID == orgtype.OrgTypeUID);
            if (selectedWarehouseType != null)
            {
                selectedWarehouseType.IsSelected = true;

            }
            WareHouseEditItemView = orgtype;
        }
        public async Task<bool> SaveUpdateWareHouse(IEditWareHouseItemView warehouse, bool Iscreate)
        {
            if (Iscreate)
            {
                warehouse.AddressUID = Guid.NewGuid().ToString();
                //  string OrgUID= _appUser.SelectedJobPosition.OrgUID;
                warehouse.ParentUID = _appUser?.SelectedJobPosition?.OrgUID;

                return await CreateUpdateWareHouseData(warehouse, true);
            }
            else
            {
                return await CreateUpdateWareHouseData(warehouse, false);
            }
        }
        #endregion
        #region Database or Services Methods
        public abstract Task<List<IOrgType>> GetWarehouseTypeDropdownByUser(string oRG_type);
        public abstract Task<List<Winit.Modules.Org.Model.Interfaces.IOrgType>> GetWarehouseTypeDropdown();
        public abstract Task<Winit.Modules.Org.Model.Interfaces.IEditWareHouseItemView> GetMaintainWarehouseEditDetails(string warehouseuid);
        public abstract Task<bool> CreateUpdateWareHouseData(Winit.Modules.Org.Model.Interfaces.IEditWareHouseItemView warehouseItem, bool IsCreate);
        #endregion
    }

}
