using Winit.Modules.Base.Model;
using Winit.Modules.Common.Model.Classes.AuditTrail;

namespace Winit.Modules.Location.Model.Interfaces
{
    public interface ISalesOffice : IBaseModel
    {
        public string? BranchUID { get; set; }
        [AuditTrail]
        public string? Code { get; set; }
        [AuditTrail]
        public string? Name { get; set; }
        [AuditTrail("WareHouse")]
        public string? WareHouseUID { get; set; }
    }
}
