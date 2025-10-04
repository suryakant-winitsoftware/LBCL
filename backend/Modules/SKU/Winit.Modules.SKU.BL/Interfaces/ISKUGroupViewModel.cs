using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.Model.UIInterfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.SKU.BL.Interfaces;

public interface ISKUGroupViewModel
{
    public List<ISKUGroupItemView> SKUGroupItemViews { get; set; }
    public List<ISelectionItem> SupplierSelectionItems { get; set; }
    public List<Shared.Models.Enums.FilterCriteria> FilterCriterias { get; set; }
    Task PopulateViewModel();
    Task getChildGrid(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView sKUGroupItemView);
    Task<List<ISelectionItem>> GetSKuGroupTypeSelectionItems(int Level, bool IsAddItembtn, string ParentUID = null,bool IsAll = false);
    Task<bool> CreateSKUGroup(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView sKUGroupItemView);
    Task<bool> DeleteSKUGroup(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView sKUGroupItemView);
    Task<bool> UpdateSKUGroup(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView sKUGroupItemView);
    Task<ISKUGroupItemView> CreateClone(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView sKUGroupItemView);
    Task SKUGroupTypeSelectedForParent(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView Context, String UID);
    Task ApplyFilter(IDictionary<string, string> keyValuePairs);
    Task CreateSKUHierarchy(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView sKUGroupItemView);

}
