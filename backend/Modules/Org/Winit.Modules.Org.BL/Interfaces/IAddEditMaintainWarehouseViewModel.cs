using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Org.BL.Interfaces;

public interface IAddEditMaintainWarehouseViewModel
{
    public IWarehouseItemView warehouseitemView { get; set; }
    public IOrgType WarehouseTypeDD { get; set; }

    public IEditWareHouseItemView WareHouseEditItemView { get; set; }

    public List<ISelectionItem> WareHouseTypeSelectionItems { get; set; }
    Task PopulateViewModel();
    Task<bool> SaveUpdateWareHouse(IEditWareHouseItemView warehouse, bool Iscreate);
    // Task GetWarehouseAddressforEditorViewDetailsData(string viewwarehouseuid);
    Task PopulateMaintainWarehouseEditDetails(string viewwarehouseuid);
    Task SetEditForOrgTypeDD(IEditWareHouseItemView orgtype);
}
