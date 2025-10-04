using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.Model.Classes;

public class StoreGroupItemView : StoreGroup, IStoreGroupItemView
{
    public string ParentStoreGroupTypeUID { get; set; } = string.Empty;
    public bool IsCreatePopUpOpen { get; set; }
    public bool IsUpdatePopUpOpen { get; set; }
    public bool IsDeletePopUpOpen { get; set; }
    public bool IsOpen { get; set; }
    public string ParentName { get; set; } = string.Empty;
    public string StoreGroupTypeName { get; set; } = string.Empty;
    public string StoreGroupTypeCode { get; set; } = string.Empty;
    public List<IStoreGroupItemView> ChildGrids { get; set; }
}
