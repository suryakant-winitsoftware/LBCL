using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Store.BL.Interfaces;

public interface IStoreGroupViewModel
{
    public List<IStoreGroupItemView> StoreGroupItemViews { get; set; }
    public List<Shared.Models.Enums.FilterCriteria> FilterCriterias { get; set; }
    Task PopulateViewModel();
    Task GetChildGrid(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView storeGroupItemView);
    List<ISelectionItem> GetStoreGroupTypeSelectionItems(int Level, bool IsAddItembtn, string? ParentUID = null, bool IsAll = false);
    Task<bool> CreateStoreGroup(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView storeGroupItemView);
    Task<bool> DeleteStoreGroup(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView storeGroupItemView);
    Task<bool> UpdateStoreGroup(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView storeGroupItemView);
    Task<IStoreGroupItemView> CreateClone(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView storeGroupItemView);
    Task StoreGroupTypeSelectedForParent(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView Context, String UID);
    Task ApplyFilter(IDictionary<string, string> keyValuePairs);
    Task CreateStoreGroupHierarchy(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView storeGroupItemView);

}
