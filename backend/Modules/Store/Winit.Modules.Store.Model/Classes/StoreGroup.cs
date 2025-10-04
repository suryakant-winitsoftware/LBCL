using Winit.Modules.Base.Model;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.Model.Classes;

public class StoreGroup : BaseModel, IStoreGroup
{
    public string StoreGroupTypeUID { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? ParentUID { get; set; }
    public int ItemLevel { get; set; }
    public bool HasChild { get; set; }
}
