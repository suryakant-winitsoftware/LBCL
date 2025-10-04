using Winit.Modules.ApprovalEngine.Model.Classes;

namespace Winit.Modules.JobPosition.Model.Classes
{
    public class JobPositionApprovalDTO
    {
        public Winit.Modules.JobPosition.Model.Interfaces.IJobPosition? JobPosition { get; set; }
        public ApprovalRequestItem? ApprovalRequestItem { get; set; }
        public ApprovalStatusUpdate? ApprovalStatusUpdate { get; set; }
    }
}
