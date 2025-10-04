using Winit.Modules.Base.Model;
using Winit.Modules.BroadClassification.Model.Interfaces;
using Winit.Modules.Common.Model.Classes.AuditTrail;

namespace Winit.Modules.BroadClassification.Model.Classes
{
    public class BroadClassificationLine : BaseModel, IBroadClassificationLine
    {
        public string? BroadClassificationHeaderUID { get; set; }
        public int LineNumber { get; set; }
        [AuditTrail("Classification Code")]
        public string ClassificationCode { get; set; }

    }
}
