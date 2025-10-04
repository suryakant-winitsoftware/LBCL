using Winit.Modules.Base.Model;
using Winit.Modules.Common.Model.Classes.AuditTrail;

namespace Winit.Modules.Location.Model.Interfaces
{
    public interface IBranch : IBaseModel
    {
        [AuditTrail]
        string Code { get; set; }
        [AuditTrail]
        string Name { get; set; }
        [AuditTrail("State Count")]
        int Level1Count { get; set; }
        [AuditTrail("City Count")]
        int Level2Count { get; set; }
        [AuditTrail("Locality Count")]
        int Level3Count { get; set; }
        int Level4Count { get; set; }
        [AuditTrail("Active")]
        bool IsActive { get; set; }
        [AuditTrail("State")]
        string Level1Data { get; set; }
        [AuditTrail("City")]
        string Level2Data { get; set; }
        [AuditTrail("Locality")]
        string Level3Data { get; set; }
        string Level4Data { get; set; }
        string SpecialState { get; set; }
    }
}
