using Winit.Modules.Base.Model;
using Winit.Modules.Task.Model.Interfaces;

namespace Winit.Modules.Task.Model.Classes
{
    public class Task : BaseModel, ITask
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int TaskTypeId { get; set; }
        public string TaskTypeName { get; set; }
        public int? TaskSubTypeId { get; set; }
        public int SalesOrgId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public string TaskData { get; set; }
        
        // Navigation properties
        public virtual TaskType TaskType { get; set; }
        public virtual TaskSubType TaskSubType { get; set; }
        public virtual List<TaskAssignment> TaskAssignments { get; set; }
    }
}