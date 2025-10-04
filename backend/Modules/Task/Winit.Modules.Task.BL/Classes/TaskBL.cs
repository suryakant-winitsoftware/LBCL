using Winit.Modules.Task.DL.Interfaces;
using Winit.Modules.Task.Model.Interfaces;
using Winit.Modules.Task.Model.DTOs;
using Winit.Modules.Task.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Task.BL.Classes
{
    public class TaskBL : Interfaces.ITaskBL
    {
        protected readonly ITaskDL _taskDL;
        
        public TaskBL(ITaskDL taskDL)
        {
            _taskDL = taskDL;
        }

        #region Task Operations

        public async Task<ITask> GetTaskByUID(string UID)
        {
            return await _taskDL.GetTaskByUID(UID);
        }

        public async Task<ITask> GetTaskByCode(string code)
        {
            return await _taskDL.GetTaskByCode(code);
        }

        public async Task<int> CreateTask(CreateTaskRequest request)
        {
            // Validate request
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(request.Title))
                throw new ArgumentException("Title is required", nameof(request.Title));

            if (!await ValidateTaskDates(request.StartDate, request.EndDate))
                throw new ArgumentException("End date must be greater than start date");

            // Create task entity
            var task = new Winit.Modules.Task.Model.Classes.Task
            {
                UID = Guid.NewGuid().ToString(),
                Code = await GenerateTaskCode(),
                Title = request.Title,
                Description = request.Description,
                TaskTypeId = request.TaskTypeId,
                TaskSubTypeId = request.TaskSubTypeId,
                SalesOrgId = request.SalesOrgId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsActive = request.IsActive,
                Priority = request.Priority ?? "Medium",
                Status = "Draft",
                TaskData = request.TaskData,
                CreatedBy = "System", // This should come from current user context
                CreatedTime = DateTime.UtcNow
            };

            return await _taskDL.CreateTask(task);
        }

        public async Task<int> UpdateTask(ITask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (!await ValidateTaskDates(task.StartDate, task.EndDate))
                throw new ArgumentException("End date must be greater than start date");

            task.ModifiedBy = "System"; // This should come from current user context
            task.ModifiedTime = DateTime.UtcNow;

            return await _taskDL.UpdateTask(task);
        }

        public async Task<int> CUDTask(ITask task)
        {
            return await _taskDL.CUDTask(task);
        }

        public async Task<PagedResponse<ITask>> GetAllTasks(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _taskDL.GetAllTasks(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }

        public async Task<int> DeleteTask(string uID)
        {
            if (string.IsNullOrEmpty(uID))
                throw new ArgumentException("UID is required", nameof(uID));

            // Check if task has assignments before deleting
            var task = await _taskDL.GetTaskByUID(uID);
            if (task != null)
            {
                var assignments = await _taskDL.GetTaskAssignments(task.Id);
                if (assignments.Any(a => a.Status == "InProgress" || a.Status == "Assigned"))
                {
                    throw new InvalidOperationException("Cannot delete task with active assignments");
                }
            }

            return await _taskDL.DeleteTask(uID);
        }

        public async Task<List<TaskDTO>> GetTasksByFilter(TaskFilterRequest filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            return await _taskDL.GetTasksByFilter(filter);
        }

        #endregion

        #region Task Type Operations

        public async Task<List<ITaskType>> GetAllTaskTypes()
        {
            return await _taskDL.GetAllTaskTypes();
        }

        public async Task<ITaskType> GetTaskTypeByUID(string UID)
        {
            return await _taskDL.GetTaskTypeByUID(UID);
        }

        public async Task<int> CreateTaskType(ITaskType taskType)
        {
            if (taskType == null)
                throw new ArgumentNullException(nameof(taskType));

            if (string.IsNullOrEmpty(taskType.Name))
                throw new ArgumentException("Name is required", nameof(taskType.Name));

            taskType.UID = Guid.NewGuid().ToString();
            taskType.CreatedBy = "System"; // This should come from current user context
            taskType.CreatedTime = DateTime.UtcNow;

            return await _taskDL.CreateTaskType(taskType);
        }

        public async Task<int> UpdateTaskType(ITaskType taskType)
        {
            if (taskType == null)
                throw new ArgumentNullException(nameof(taskType));

            taskType.ModifiedBy = "System"; // This should come from current user context
            taskType.ModifiedTime = DateTime.UtcNow;

            return await _taskDL.UpdateTaskType(taskType);
        }

        public async Task<int> DeleteTaskType(string uID)
        {
            if (string.IsNullOrEmpty(uID))
                throw new ArgumentException("UID is required", nameof(uID));

            return await _taskDL.DeleteTaskType(uID);
        }

        #endregion

        #region Task Sub Type Operations

        public async Task<List<ITaskSubType>> GetTaskSubTypesByTaskType(int taskTypeId)
        {
            return await _taskDL.GetTaskSubTypesByTaskType(taskTypeId);
        }

        public async Task<ITaskSubType> GetTaskSubTypeByUID(string UID)
        {
            return await _taskDL.GetTaskSubTypeByUID(UID);
        }

        public async Task<int> CreateTaskSubType(ITaskSubType taskSubType)
        {
            if (taskSubType == null)
                throw new ArgumentNullException(nameof(taskSubType));

            if (string.IsNullOrEmpty(taskSubType.Name))
                throw new ArgumentException("Name is required", nameof(taskSubType.Name));

            if (taskSubType.TaskTypeId <= 0)
                throw new ArgumentException("Valid TaskTypeId is required", nameof(taskSubType.TaskTypeId));

            taskSubType.UID = Guid.NewGuid().ToString();
            taskSubType.CreatedBy = "System"; // This should come from current user context
            taskSubType.CreatedTime = DateTime.UtcNow;

            return await _taskDL.CreateTaskSubType(taskSubType);
        }

        public async Task<int> UpdateTaskSubType(ITaskSubType taskSubType)
        {
            if (taskSubType == null)
                throw new ArgumentNullException(nameof(taskSubType));

            taskSubType.ModifiedBy = "System"; // This should come from current user context
            taskSubType.ModifiedTime = DateTime.UtcNow;

            return await _taskDL.UpdateTaskSubType(taskSubType);
        }

        public async Task<int> DeleteTaskSubType(string uID)
        {
            if (string.IsNullOrEmpty(uID))
                throw new ArgumentException("UID is required", nameof(uID));

            return await _taskDL.DeleteTaskSubType(uID);
        }

        #endregion

        #region Task Assignment Operations

        public async Task<List<ITaskAssignment>> GetTaskAssignments(int taskId)
        {
            return await _taskDL.GetTaskAssignments(taskId);
        }

        public async Task<List<ITaskAssignment>> GetUserTaskAssignments(int userId)
        {
            return await _taskDL.GetUserTaskAssignments(userId);
        }

        public async Task<List<ITaskAssignment>> GetUserGroupTaskAssignments(int userGroupId)
        {
            return await _taskDL.GetUserGroupTaskAssignments(userGroupId);
        }

        public async Task<int> CreateTaskAssignment(ITaskAssignment assignment)
        {
            if (assignment == null)
                throw new ArgumentNullException(nameof(assignment));

            if (assignment.TaskId <= 0)
                throw new ArgumentException("Valid TaskId is required", nameof(assignment.TaskId));

            if (string.IsNullOrEmpty(assignment.AssignedToType))
                throw new ArgumentException("AssignedToType is required", nameof(assignment.AssignedToType));

            if (assignment.AssignedToType == "User" && (!assignment.UserId.HasValue || assignment.UserId <= 0))
                throw new ArgumentException("Valid UserId is required for User assignment", nameof(assignment.UserId));

            if (assignment.AssignedToType == "UserGroup" && (!assignment.UserGroupId.HasValue || assignment.UserGroupId <= 0))
                throw new ArgumentException("Valid UserGroupId is required for UserGroup assignment", nameof(assignment.UserGroupId));

            assignment.UID = Guid.NewGuid().ToString();
            assignment.Status = "Assigned";
            assignment.AssignedDate = DateTime.UtcNow;
            assignment.Progress = 0;
            assignment.CreatedBy = "System"; // This should come from current user context
            assignment.CreatedTime = DateTime.UtcNow;

            return await _taskDL.CreateTaskAssignment(assignment);
        }

        public async Task<int> UpdateTaskAssignment(ITaskAssignment assignment)
        {
            if (assignment == null)
                throw new ArgumentNullException(nameof(assignment));

            assignment.ModifiedBy = "System"; // This should come from current user context
            assignment.ModifiedTime = DateTime.UtcNow;

            // Auto-set dates based on status
            if (assignment.Status == "InProgress" && !assignment.StartedDate.HasValue)
            {
                assignment.StartedDate = DateTime.UtcNow;
            }
            else if (assignment.Status == "Completed" && !assignment.CompletedDate.HasValue)
            {
                assignment.CompletedDate = DateTime.UtcNow;
                assignment.Progress = 100;
            }

            return await _taskDL.UpdateTaskAssignment(assignment);
        }

        public async Task<int> DeleteTaskAssignment(string uID)
        {
            if (string.IsNullOrEmpty(uID))
                throw new ArgumentException("UID is required", nameof(uID));

            return await _taskDL.DeleteTaskAssignment(uID);
        }

        public async Task<int> BulkAssignTasks(AssignTaskRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.TaskId <= 0)
                throw new ArgumentException("Valid TaskId is required", nameof(request.TaskId));

            if ((request.UserIds == null || !request.UserIds.Any()) && 
                (request.UserGroupIds == null || !request.UserGroupIds.Any()))
                throw new ArgumentException("At least one User or UserGroup must be specified for assignment");

            // Verify task exists
            var task = await _taskDL.GetTaskByUID(request.TaskId.ToString());
            if (task == null)
                throw new ArgumentException("Task not found", nameof(request.TaskId));

            return await _taskDL.BulkAssignTasks(request);
        }

        #endregion

        #region Advanced Operations

        public async Task<List<TaskDTO>> GetTasksDashboard(int? userId, int? userGroupId, int? salesOrgId)
        {
            return await _taskDL.GetTasksDashboard(userId, userGroupId, salesOrgId);
        }

        public async Task<Dictionary<string, int>> GetTaskStatusCounts(int? userId, int? salesOrgId)
        {
            return await _taskDL.GetTaskStatusCounts(userId, salesOrgId);
        }

        public async Task<bool> ValidateTaskDates(DateTime startDate, DateTime endDate)
        {
            return await System.Threading.Tasks.Task.FromResult(endDate > startDate);
        }

        public async Task<bool> CanUserAccessTask(int taskId, int userId)
        {
            // Check if user has assignment for this task
            var assignments = await _taskDL.GetUserTaskAssignments(userId);
            return assignments.Any(a => a.TaskId == taskId);
        }

        #endregion

        #region Helper Methods

        private async Task<string> GenerateTaskCode()
        {
            // Generate a unique task code
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return await System.Threading.Tasks.Task.FromResult($"TSK{timestamp}{random}");
        }

        #endregion
    }
}