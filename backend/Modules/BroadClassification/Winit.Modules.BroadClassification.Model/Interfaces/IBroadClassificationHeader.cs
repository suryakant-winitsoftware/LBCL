using Winit.Modules.Common.Model.Classes.AuditTrail;

namespace Winit.Modules.BroadClassification.Model.Interfaces
{
    public interface IBroadClassificationHeader : Winit.Modules.Base.Model.IBaseModel
    {
        [AuditTrail("Broad Classification Name")]
        public string? Name { get; set; }
        public int ClassificationCount { get; set; }
        [AuditTrail("Active")]
        public bool IsActive { get; set; }
    }
}
