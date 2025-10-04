namespace Winit.Modules.Store.Model.Interfaces;

public interface IStoreGroupItemView : IStoreGroup
{
    public string ParentStoreGroupTypeUID { get; set; }
    public bool IsCreatePopUpOpen { get; set; }
    public bool IsUpdatePopUpOpen { get; set; }
    public bool IsDeletePopUpOpen { get; set; }
    public bool IsOpen { get; set; }
    public string ParentName { get; set; }
    public string StoreGroupTypeName { get; set; }
    public string StoreGroupTypeCode{ get; set; }
    public List<IStoreGroupItemView> ChildGrids { get; set; }
}
