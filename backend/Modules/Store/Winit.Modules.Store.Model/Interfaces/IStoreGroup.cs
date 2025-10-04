using Winit.Modules.Base.Model;

namespace Winit.Modules.Store.Model.Interfaces;

public interface IStoreGroup : IBaseModel
{
    public string StoreGroupTypeUID { get; set; }
    public string Code { get; set; }
    public string? Name { get; set; }
    public string? ParentUID { get; set; }
    public int ItemLevel { get; set; }
    public bool HasChild { get; set; }
}
