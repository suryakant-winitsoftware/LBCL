using Winit.Modules.Base.Model;
using Winit.Modules.Common.Model.Classes.AuditTrail;
using Winit.Modules.Location.Model.Interfaces;

namespace Winit.Modules.Location.Model.Classes;

public class Branch : BaseModel, IBranch
{
    [AuditTrail]
    public string Code { get; set; }
    [AuditTrail]
    public string Name { get; set; }
    [AuditTrail("State Count")]
    public int Level1Count { get; set; }
    [AuditTrail("City Count")]
    public int Level2Count { get; set; }
    [AuditTrail("Locality Count")]
    public int Level3Count { get; set; }
    public int Level4Count { get; set; }
    [AuditTrail("Active")]
    public bool IsActive { get; set; }
    [AuditTrail("State")]
    public string Level1Data { get; set; }
    [AuditTrail("City")]
    public string Level2Data { get; set; }
    [AuditTrail("Locality")]
    public string Level3Data { get; set; }
    public string Level4Data { get; set; }
    public string SpecialState { get; set; }

}

