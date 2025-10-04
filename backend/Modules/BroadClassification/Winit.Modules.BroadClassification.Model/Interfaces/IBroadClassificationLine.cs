using Winit.Modules.Common.Model.Classes.AuditTrail;

namespace Winit.Modules.BroadClassification.Model.Interfaces
{
    public interface IBroadClassificationLine : Winit.Modules.Base.Model.IBaseModel
    {
        public string? BroadClassificationHeaderUID { get; set; }
        public int LineNumber { get; set; }
        [AuditTrail("Classification Code")]
        public string ClassificationCode { get; set; }
    }
}
