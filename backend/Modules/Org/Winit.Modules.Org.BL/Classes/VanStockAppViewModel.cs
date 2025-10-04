using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Org.BL.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Org.BL.Classes
{
    public class VanStockAppViewModel : ViewWareHouse_VanStockBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly IOrgBL _orgBl;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private List<string> _propertiesToSearch = new List<string>();
        public List<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView> VanStockitemsList { set; get; }
        public VanStockAppViewModel(IServiceProvider serviceProvider,
             IFilterHelper filter, IOrgBL orgBl,
             ISortHelper sorter,
             IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService)
         : base(serviceProvider, filter, sorter, listHelper, appConfigs, apiService)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _orgBl = orgBl;
            _sorter = sorter;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            Warehouse_VanSalablestockList = new List<IWarehouseStockItemView>();
            Warehouse_VanFocstockList = new List<IWarehouseFOCStockItemView>();
            FilterCriterias = new List<FilterCriteria>();
            WareHouseSelectionItems = new List<ISelectionItem>();
            // Property set for Search
            _propertiesToSearch.Add("Code");
            _propertiesToSearch.Add("Name");
        }
        public override Task<List<IWarehouseFOCStockItemView>> GetWarehouse_VanFocStockData()
        {
            throw new NotImplementedException();
        }
        public override Task<List<Winit.Modules.Org.Model.Interfaces.IOrg>> GetWarehouseDropdownDataDD(string OrgTypeUID)
        {
            throw new NotImplementedException();
        }
        public override Task<List<Winit.Modules.Org.Model.Interfaces.IOrg>> GetVanDropdownDataDD(string OrgTypeUID)
        {
            throw new NotImplementedException();
        }
        public override Task<List<IWarehouseStockItemView>> GetWarehouse_VanNormal_SaleableStockData()
        {
            throw new NotImplementedException();
        }
        
        public async Task<List<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView>> GetVanStockList(string warehouseUID, string orgUID, StockType? stockType)
        {
            
           IEnumerable<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView>  vanstockitems = await _orgBl.GetVanStockItems(warehouseUID, orgUID, stockType);

           return VanStockitemsList = vanstockitems.ToList();
        }

        public override Task<List<IWareHouseStock>> GridDataForMaintainWareHouseStock(List<string> organisationalDDSelectedItems, string wareHouseDDSelectedItem, List<string> subWareHouseDDSelectedItems)
        {
            throw new NotImplementedException();
        }

        public override Task<List<IOrg>> GetSubWarehouseDropdownDataDD(string OrgTypeUID, string wareHouseDDSelectedItem, string branchUID)
        {
            throw new NotImplementedException();
        }

        public override Task<List<ISKUGroup>> GetWarehouseFilterDropdownDataDD(string starRating)
        {
            throw new NotImplementedException();
        }
    }
}
