using Winit.Modules.Task.Model.Interfaces;
using Winit.Modules.Task.Model.DTOs;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Task.BL.Interfaces
{
    public interface ITaskBL
    {
        // Task CRUD Operations
        Task<ITask> GetTaskByUID(string UID);
        Task<ITask> GetTaskByCode(string code);
        Task<int> CreateTask(CreateTaskRequest request);
        Task<int> UpdateTask(ITask task);
        Task<int> CUDTask(ITask task);
        Task<PagedResponse<ITask>> GetAllTasks(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<int> DeleteTask(string uID);
        Task<List<TaskDTO>> GetTasksByFilter(TaskFilterRequest filter);

        // Task Type Operations
        Task<List<ITaskType>> GetAllTaskTypes();
        Task<ITaskType> GetTaskTypeByUID(string UID);
        Task<int> CreateTaskType(ITaskType taskType);
        Task<int> UpdateTaskType(ITaskType taskType);
        Task<int> DeleteTaskType(string uID);

        // Task Sub Type Operations
        Task<List<ITaskSubType>> GetTaskSubTypesByTaskType(int taskTypeId);
        Task<ITaskSubType> GetTaskSubTypeByUID(string UID);
        Task<int> CreateTaskSubType(ITaskSubType taskSubType);
        Task<int> UpdateTaskSubType(ITaskSubType taskSubType);
        Task<int> DeleteTaskSubType(string uID);

        // Task Assignment Operations
        Task<List<ITaskAssignment>> GetTaskAssignments(int taskId);
        Task<List<ITaskAssignment>> GetUserTaskAssignments(int userId);
        Task<List<ITaskAssignment>> GetUserGroupTaskAssignments(int userGroupId);
        Task<int> CreateTaskAssignment(ITaskAssignment assignment);
        Task<int> UpdateTaskAssignment(ITaskAssignment assignment);
        Task<int> DeleteTaskAssignment(string uID);
        Task<int> BulkAssignTasks(AssignTaskRequest request);

        // Advanced Operations
        Task<List<TaskDTO>> GetTasksDashboard(int? userId, int? userGroupId, int? salesOrgId);
        Task<Dictionary<string, int>> GetTaskStatusCounts(int? userId, int? salesOrgId);
        Task<bool> ValidateTaskDates(DateTime startDate, DateTime endDate);
        Task<bool> CanUserAccessTask(int taskId, int userId);
    }
}