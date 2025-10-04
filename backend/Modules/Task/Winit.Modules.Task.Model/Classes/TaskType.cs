using Winit.Modules.Base.Model;
using Winit.Modules.Task.Model.Interfaces;

namespace Winit.Modules.Task.Model.Classes
{
    public class TaskType : BaseModel, ITaskType
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        
        // Navigation properties
        public virtual List<TaskSubType> TaskSubTypes { get; set; }
        public virtual List<Task> Tasks { get; set; }
    }
}