using Winit.Modules.Base.Model;
using Winit.Modules.Common.Model.Classes.AuditTrail;
using Winit.Modules.Location.Model.Interfaces;

namespace Winit.Modules.Location.Model.Classes;

public class SalesOffice : BaseModel, ISalesOffice
{
    public string? BranchUID { get; set; }
    [AuditTrail]
    public string? Code { get; set; }
    [AuditTrail]
    public string? Name { get; set; }
    [AuditTrail("WareHouse")]
    public string? WareHouseUID { get; set; }

}

