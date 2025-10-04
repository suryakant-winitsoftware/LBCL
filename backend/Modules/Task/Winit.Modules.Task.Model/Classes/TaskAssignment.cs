using Winit.Modules.Base.Model;
using Winit.Modules.Task.Model.Interfaces;

namespace Winit.Modules.Task.Model.Classes
{
    public class TaskAssignment : BaseModel, ITaskAssignment
    {
        public int TaskId { get; set; }
        public string AssignedToType { get; set; } // "User" or "UserGroup"
        public int? UserId { get; set; }
        public int? UserGroupId { get; set; }
        public string Status { get; set; } // "Pending", "InProgress", "Completed", "Cancelled"
        public DateTime? AssignedDate { get; set; }
        public DateTime? StartedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Notes { get; set; }
        public int? Progress { get; set; } // 0-100 percentage
        
        // Navigation properties
        public virtual Task Task { get; set; }
    }
}