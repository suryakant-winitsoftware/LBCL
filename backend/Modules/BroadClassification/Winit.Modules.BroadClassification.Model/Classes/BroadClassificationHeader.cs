using Winit.Modules.Base.Model;
using Winit.Modules.BroadClassification.Model.Interfaces;
using Winit.Modules.Common.Model.Classes.AuditTrail;

namespace Winit.Modules.BroadClassification.Model.Classes
{
    public class BroadClassificationHeader : BaseModel, IBroadClassificationHeader
    {
        [AuditTrail("Broad Classification Name")]
        public string? Name { get; set; }
        public int ClassificationCount { get; set; }
        [AuditTrail("Active")]
        public bool IsActive { get; set; }

    }
}
