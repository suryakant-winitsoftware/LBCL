using Winit.Modules.Base.Model;

namespace Winit.Modules.Task.Model.Interfaces
{
    public interface ITaskAssignment : IBaseModel
    {
        int TaskId { get; set; }
        string AssignedToType { get; set; }
        int? UserId { get; set; }
        int? UserGroupId { get; set; }
        string Status { get; set; }
        DateTime? AssignedDate { get; set; }
        DateTime? StartedDate { get; set; }
        DateTime? CompletedDate { get; set; }
        string Notes { get; set; }
        int? Progress { get; set; }
    }
}