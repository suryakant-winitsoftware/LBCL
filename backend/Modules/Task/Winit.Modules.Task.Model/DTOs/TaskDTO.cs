namespace Winit.Modules.Task.Model.DTOs
{
    public class TaskDTO
    {
        public int Id { get; set; }
        public string UID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int TaskTypeId { get; set; }
        public string TaskTypeName { get; set; }
        public int? TaskSubTypeId { get; set; }
        public string TaskSubTypeName { get; set; }
        public int SalesOrgId { get; set; }
        public string SalesOrgName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public string TaskData { get; set; }
        public DateTime? CreatedTime { get; set; }
        public string CreatedBy { get; set; }
        public List<TaskAssignmentDTO> Assignments { get; set; }
    }

    public class TaskAssignmentDTO
    {
        public int Id { get; set; }
        public string UID { get; set; }
        public int TaskId { get; set; }
        public string AssignedToType { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public int? UserGroupId { get; set; }
        public string UserGroupName { get; set; }
        public string Status { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? StartedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Notes { get; set; }
        public int? Progress { get; set; }
    }

    public class CreateTaskRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int TaskTypeId { get; set; }
        public int? TaskSubTypeId { get; set; }
        public int SalesOrgId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public string Priority { get; set; }
        public string TaskData { get; set; }
    }

    public class AssignTaskRequest
    {
        public int TaskId { get; set; }
        public string AssignedToType { get; set; } // "User" or "UserGroup"
        public List<int> UserIds { get; set; }
        public List<int> UserGroupIds { get; set; }
        public string Notes { get; set; }
    }

    public class TaskFilterRequest
    {
        public int? TaskTypeId { get; set; }
        public int? TaskSubTypeId { get; set; }
        public int? SalesOrgId { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? AssignedUserId { get; set; }
        public int? AssignedUserGroupId { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}