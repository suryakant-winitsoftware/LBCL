using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Serilog;
using Winit.Modules.Task.Model.Interfaces;
using Winit.Modules.Task.Model.DTOs;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace WINITAPI.Controllers.TaskManagement
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TaskController : WINITBaseController
    {
        private readonly Winit.Modules.Task.BL.Interfaces.ITaskBL _taskBL;

        public TaskController(IServiceProvider serviceProvider,
            Winit.Modules.Task.BL.Interfaces.ITaskBL taskBL) : base(serviceProvider)
        {
            _taskBL = taskBL;
        }

        #region Task CRUD Operations

        [HttpPost]
        [Route("GetAllTasks")]
        public async Task<ActionResult> GetAllTasks(PagingRequest pagingRequest)
        {
            try
            {
                if (pagingRequest == null)
                {
                    return BadRequest("Invalid request data");
                }

                if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
                {
                    return BadRequest("Invalid page size or page number");
                }

                PagedResponse<ITask> pagedResponseTaskList = await _taskBL.GetAllTasks(
                    pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, 
                    pagingRequest.PageSize, 
                    pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);

                if (pagedResponseTaskList == null)
                {
                    return NotFound();
                }

                return CreateOkApiResponse(pagedResponseTaskList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Task Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetTaskByUID/{uID}")]
        public async Task<ActionResult> GetTaskByUID(string uID)
        {
            try
            {
                ITask task = await _taskBL.GetTaskByUID(uID);
                if (task != null)
                {
                    return CreateOkApiResponse(task);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Task with UID: {@UID}", uID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetTaskByCode/{code}")]
        public async Task<ActionResult> GetTaskByCode(string code)
        {
            try
            {
                ITask task = await _taskBL.GetTaskByCode(code);
                if (task != null)
                {
                    return CreateOkApiResponse(task);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Task with Code: {@Code}", code);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateTask")]
        public async Task<ActionResult> CreateTask(CreateTaskRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Invalid request data");
                }

                int result = await _taskBL.CreateTask(request);
                if (result > 0)
                {
                    return CreateOkApiResponse(result);
                }
                else
                {
                    return CreateErrorResponse("Failed to create task");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Task");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPut]
        [Route("UpdateTask")]
        public async Task<ActionResult> UpdateTask(ITask task)
        {
            try
            {
                if (task == null)
                {
                    return BadRequest("Invalid task data");
                }

                int result = await _taskBL.UpdateTask(task);
                if (result > 0)
                {
                    return CreateOkApiResponse(result);
                }
                else
                {
                    return CreateErrorResponse("Failed to update task");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to update Task with UID: {@UID}", task?.UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CUDTask")]
        public async Task<ActionResult> CUDTask(ITask task)
        {
            try
            {
                if (task == null)
                {
                    return BadRequest("Invalid task data");
                }

                int result = await _taskBL.CUDTask(task);
                if (result > 0)
                {
                    return CreateOkApiResponse(result);
                }
                else
                {
                    return CreateErrorResponse("Failed to save task");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save Task with UID: {@UID}", task?.UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteTask/{uID}")]
        public async Task<ActionResult> DeleteTask(string uID)
        {
            try
            {
                if (string.IsNullOrEmpty(uID))
                {
                    return BadRequest("UID is required");
                }

                int result = await _taskBL.DeleteTask(uID);
                if (result > 0)
                {
                    return CreateOkApiResponse(result);
                }
                else
                {
                    return CreateErrorResponse("Failed to delete task");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to delete Task with UID: {@UID}", uID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("GetTasksByFilter")]
        public async Task<ActionResult> GetTasksByFilter(TaskFilterRequest filter)
        {
            try
            {
                if (filter == null)
                {
                    return BadRequest("Invalid filter data");
                }

                var tasks = await _taskBL.GetTasksByFilter(filter);
                return CreateOkApiResponse(tasks);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Tasks by filter");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        #endregion

        #region Task Type Operations - DEPRECATED: Now using list_header/list_item structure

        [Obsolete("Task types are now managed through list_header/list_item structure. Use ListItemHeaderController instead.")]
        [HttpGet]
        [Route("GetAllTaskTypes")]
        public async Task<ActionResult> GetAllTaskTypes()
        {
            try
            {
                // Return empty list since task types are now managed through list_item structure
                return CreateOkApiResponse(new List<object>());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deprecated method called: GetAllTaskTypes");
                return CreateErrorResponse("This method is deprecated. Task types are now managed through list_header/list_item structure. Use ListItemHeaderController instead.");
            }
        }

        [Obsolete("Task types are now managed through list_header/list_item structure. Use ListItemHeaderController instead.")]
        [HttpGet]
        [Route("GetTaskTypeByUID/{uID}")]
        public async Task<ActionResult> GetTaskTypeByUID(string uID)
        {
            return CreateErrorResponse("This method is deprecated. Task types are now managed through list_header/list_item structure. Use ListItemHeaderController instead.");
        }

        [Obsolete("Task types are now managed through list_header/list_item structure. Use ListItemHeaderController instead.")]
        [HttpPost]
        [Route("CreateTaskType")]
        public async Task<ActionResult> CreateTaskType(ITaskType taskType)
        {
            return CreateErrorResponse("This method is deprecated. Task types are now managed through list_header/list_item structure. Use ListItemHeaderController instead.");
        }

        [Obsolete("Task types are now managed through list_header/list_item structure. Use ListItemHeaderController instead.")]
        [HttpPut]
        [Route("UpdateTaskType")]
        public async Task<ActionResult> UpdateTaskType(ITaskType taskType)
        {
            return CreateErrorResponse("This method is deprecated. Task types are now managed through list_header/list_item structure. Use ListItemHeaderController instead.");
        }

        [Obsolete("Task types are now managed through list_header/list_item structure. Use ListItemHeaderController instead.")]
        [HttpDelete]
        [Route("DeleteTaskType/{uID}")]
        public async Task<ActionResult> DeleteTaskType(string uID)
        {
            return CreateErrorResponse("This method is deprecated. Task types are now managed through list_header/list_item structure. Use ListItemHeaderController instead.");
        }

        #endregion

        #region Task Sub Type Operations - DEPRECATED: No longer supported

        [Obsolete("Task sub types are no longer supported. Use main task types through list_header/list_item structure.")]
        [HttpGet]
        [Route("GetTaskSubTypesByTaskType/{taskTypeId}")]
        public async Task<ActionResult> GetTaskSubTypesByTaskType(int taskTypeId)
        {
            return CreateOkApiResponse(new List<object>()); // Return empty list
        }

        [Obsolete("Task sub types are no longer supported. Use main task types through list_header/list_item structure.")]
        [HttpGet]
        [Route("GetTaskSubTypeByUID/{uID}")]
        public async Task<ActionResult> GetTaskSubTypeByUID(string uID)
        {
            return CreateErrorResponse("Task sub types are no longer supported. Use main task types through list_header/list_item structure instead.");
        }

        [Obsolete("Task sub types are no longer supported. Use main task types through list_header/list_item structure.")]
        [HttpPost]
        [Route("CreateTaskSubType")]
        public async Task<ActionResult> CreateTaskSubType(ITaskSubType taskSubType)
        {
            return CreateErrorResponse("Task sub types are no longer supported. Use main task types through list_header/list_item structure instead.");
        }

        [Obsolete("Task sub types are no longer supported. Use main task types through list_header/list_item structure.")]
        [HttpPut]
        [Route("UpdateTaskSubType")]
        public async Task<ActionResult> UpdateTaskSubType(ITaskSubType taskSubType)
        {
            return CreateErrorResponse("Task sub types are no longer supported. Use main task types through list_header/list_item structure instead.");
        }

        [Obsolete("Task sub types are no longer supported. Use main task types through list_header/list_item structure.")]
        [HttpDelete]
        [Route("DeleteTaskSubType/{uID}")]
        public async Task<ActionResult> DeleteTaskSubType(string uID)
        {
            return CreateErrorResponse("Task sub types are no longer supported. Use main task types through list_header/list_item structure instead.");
        }

        #endregion

        #region Task Assignment Operations

        [HttpGet]
        [Route("GetTaskAssignments/{taskId}")]
        public async Task<ActionResult> GetTaskAssignments(int taskId)
        {
            try
            {
                var assignments = await _taskBL.GetTaskAssignments(taskId);
                return CreateOkApiResponse(assignments);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Task Assignments for Task: {@TaskId}", taskId);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetUserTaskAssignments/{userId}")]
        public async Task<ActionResult> GetUserTaskAssignments(int userId)
        {
            try
            {
                var assignments = await _taskBL.GetUserTaskAssignments(userId);
                return CreateOkApiResponse(assignments);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve User Task Assignments for User: {@UserId}", userId);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetUserGroupTaskAssignments/{userGroupId}")]
        public async Task<ActionResult> GetUserGroupTaskAssignments(int userGroupId)
        {
            try
            {
                var assignments = await _taskBL.GetUserGroupTaskAssignments(userGroupId);
                return CreateOkApiResponse(assignments);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve User Group Task Assignments for Group: {@UserGroupId}", userGroupId);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateTaskAssignment")]
        public async Task<ActionResult> CreateTaskAssignment(ITaskAssignment assignment)
        {
            try
            {
                if (assignment == null)
                {
                    return BadRequest("Invalid assignment data");
                }

                int result = await _taskBL.CreateTaskAssignment(assignment);
                if (result > 0)
                {
                    return CreateOkApiResponse(result);
                }
                else
                {
                    return CreateErrorResponse("Failed to create task assignment");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Task Assignment");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("AssignTask")]
        public async Task<ActionResult> AssignTask(AssignTaskRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Invalid assignment request");
                }

                int result = await _taskBL.BulkAssignTasks(request);
                if (result > 0)
                {
                    return CreateOkApiResponse(new { AssignedCount = result });
                }
                else
                {
                    return CreateErrorResponse("Failed to assign task");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to assign Task");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPut]
        [Route("UpdateTaskAssignment")]
        public async Task<ActionResult> UpdateTaskAssignment(ITaskAssignment assignment)
        {
            try
            {
                if (assignment == null)
                {
                    return BadRequest("Invalid assignment data");
                }

                int result = await _taskBL.UpdateTaskAssignment(assignment);
                if (result > 0)
                {
                    return CreateOkApiResponse(result);
                }
                else
                {
                    return CreateErrorResponse("Failed to update task assignment");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to update Task Assignment with UID: {@UID}", assignment?.UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteTaskAssignment/{uID}")]
        public async Task<ActionResult> DeleteTaskAssignment(string uID)
        {
            try
            {
                if (string.IsNullOrEmpty(uID))
                {
                    return BadRequest("UID is required");
                }

                int result = await _taskBL.DeleteTaskAssignment(uID);
                if (result > 0)
                {
                    return CreateOkApiResponse(result);
                }
                else
                {
                    return CreateErrorResponse("Failed to delete task assignment");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to delete Task Assignment with UID: {@UID}", uID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        #endregion

        #region Dashboard and Reporting

        [HttpGet]
        [Route("GetTasksDashboard")]
        public async Task<ActionResult> GetTasksDashboard(int? userId = null, int? userGroupId = null, int? salesOrgId = null)
        {
            try
            {
                var tasks = await _taskBL.GetTasksDashboard(userId, userGroupId, salesOrgId);
                return CreateOkApiResponse(tasks);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Tasks Dashboard");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetTaskStatusCounts")]
        public async Task<ActionResult> GetTaskStatusCounts(int? userId = null, int? salesOrgId = null)
        {
            try
            {
                var statusCounts = await _taskBL.GetTaskStatusCounts(userId, salesOrgId);
                return CreateOkApiResponse(statusCounts);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Task Status Counts");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        #endregion

        #region Utility Methods

        [HttpPost]
        [Route("ValidateTaskDates")]
        public async Task<ActionResult> ValidateTaskDates(DateTime startDate, DateTime endDate)
        {
            try
            {
                bool isValid = await _taskBL.ValidateTaskDates(startDate, endDate);
                return CreateOkApiResponse(new { IsValid = isValid });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to validate Task dates");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("CanUserAccessTask/{taskId}/{userId}")]
        public async Task<ActionResult> CanUserAccessTask(int taskId, int userId)
        {
            try
            {
                bool canAccess = await _taskBL.CanUserAccessTask(taskId, userId);
                return CreateOkApiResponse(new { CanAccess = canAccess });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to check user access for Task");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        #endregion
    }
}