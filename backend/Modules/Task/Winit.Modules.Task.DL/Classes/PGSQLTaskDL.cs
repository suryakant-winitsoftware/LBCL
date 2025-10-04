using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text;
using Winit.Modules.Task.DL.Interfaces;
using Winit.Modules.Task.Model.Interfaces;
using Winit.Modules.Task.Model.DTOs;
using Winit.Modules.Task.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using System.Collections.Specialized;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Winit.Modules.Task.DL.Classes
{
    public class PGSQLTaskDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, ITaskDL
    {
        public PGSQLTaskDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        #region Task Operations

        public async Task<PagedResponse<ITask>> GetAllTasks(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"Select * from (SELECT t.id AS Id, t.uid AS UID, t.created_by AS CreatedBy,
                                            t.created_time AS CreatedTime, t.modified_by AS ModifiedBy, t.modified_time AS ModifiedTime,
                                            t.server_add_time AS ServerAddTime, t.server_modified_time AS ServerModifiedTime, t.ss AS SS, 
                                            t.code AS Code, t.title AS Title, t.description AS Description, t.task_type_id AS TaskTypeId,
                                            t.task_sub_type_id AS TaskSubTypeId, t.sales_org_id AS SalesOrgId, t.start_date AS StartDate,
                                            t.end_date AS EndDate, t.is_active::bool AS IsActive, t.priority AS Priority, 
                                            t.status AS Status, t.task_data AS TaskData FROM task t)as subquery");

                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT t.id AS Id, t.uid AS UID, t.created_by AS CreatedBy,
                                            t.created_time AS CreatedTime, t.modified_by AS ModifiedBy, t.modified_time AS ModifiedTime,
                                            t.server_add_time AS ServerAddTime, t.server_modified_time AS ServerModifiedTime, t.ss AS SS, 
                                            t.code AS Code, t.title AS Title, t.description AS Description, t.task_type_id AS TaskTypeId,
                                            t.task_sub_type_id AS TaskSubTypeId, t.sales_org_id AS SalesOrgId, t.start_date AS StartDate,
                                            t.end_date AS EndDate, t.is_active::bool AS IsActive, t.priority AS Priority, 
                                            t.status AS Status, t.task_data AS TaskData FROM task t)as subquery");
                }

                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<ITask>(filterCriterias, sbFilterCriteria, parameters);

                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }

                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    AppendSortCriteria(sortCriterias, sql);
                }

                if (pageNumber > 0 && pageSize > 0)
                {
                    if (sortCriterias != null && sortCriterias.Count > 0)
                    {
                        sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                    else
                    {
                        sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }

                IEnumerable<ITask> taskList = await ExecuteQueryAsync<ITask>(sql.ToString(), parameters);
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<ITask> pagedResponse = new PagedResponse<ITask>
                {
                    PagedData = taskList,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ITask> GetTaskByUID(string UID)
        {
            try
            {
                var sql = @"SELECT t.id AS Id, t.uid AS UID, t.created_by AS CreatedBy,
                           t.created_time AS CreatedTime, t.modified_by AS ModifiedBy, t.modified_time AS ModifiedTime,
                           t.server_add_time AS ServerAddTime, t.server_modified_time AS ServerModifiedTime, t.ss AS SS, 
                           t.code AS Code, t.title AS Title, t.description AS Description, t.task_type_id AS TaskTypeId,
                           li.name AS TaskTypeName, t.task_sub_type_id AS TaskSubTypeId, '' AS TaskSubTypeName,
                           t.sales_org_id AS SalesOrgId, t.start_date AS StartDate,
                           t.end_date AS EndDate, t.is_active::bool AS IsActive, t.priority AS Priority, 
                           t.status AS Status, t.task_data AS TaskData 
                           FROM task t 
                           LEFT JOIN list_item li ON t.task_type_uid = li.uid
                           WHERE t.uid = @UID";

                var parameters = new Dictionary<string, object> { { "@UID", UID } };
                return await ExecuteSingleAsync<ITask>(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ITask> GetTaskByCode(string code)
        {
            try
            {
                var sql = @"SELECT t.id AS Id, t.uid AS UID, t.created_by AS CreatedBy,
                           t.created_time AS CreatedTime, t.modified_by AS ModifiedBy, t.modified_time AS ModifiedTime,
                           t.server_add_time AS ServerAddTime, t.server_modified_time AS ServerModifiedTime, t.ss AS SS, 
                           t.code AS Code, t.title AS Title, t.description AS Description, t.task_type_id AS TaskTypeId,
                           t.task_sub_type_id AS TaskSubTypeId, t.sales_org_id AS SalesOrgId, t.start_date AS StartDate,
                           t.end_date AS EndDate, t.is_active::bool AS IsActive, t.priority AS Priority, 
                           t.status AS Status, t.task_data AS TaskData 
                           FROM task t WHERE t.code = @Code";

                var parameters = new Dictionary<string, object> { { "@Code", code } };
                return await ExecuteSingleAsync<ITask>(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> CreateTask(ITask task)
        {
            try
            {
                var sql = @"INSERT INTO task (uid, code, title, description, task_type_id, task_sub_type_id, task_type_uid,
                           sales_org_id, start_date, end_date, is_active, priority, status, task_data, 
                           created_by, created_time, server_add_time) 
                           VALUES (@UID, @Code, @Title, @Description, @TaskTypeId, @TaskSubTypeId, 
                           (SELECT uid FROM list_item WHERE id = @TaskTypeId AND list_header_uid = (SELECT uid FROM list_header WHERE code = 'TaskType')),
                           @SalesOrgId, @StartDate, @EndDate, @IsActive, @Priority, @Status, @TaskData, 
                           @CreatedBy, @CreatedTime, @ServerAddTime) RETURNING id";

                var parameters = new Dictionary<string, object>
                {
                    { "@UID", task.UID },
                    { "@Code", task.Code },
                    { "@Title", task.Title },
                    { "@Description", task.Description },
                    { "@TaskTypeId", task.TaskTypeId },
                    { "@TaskSubTypeId", task.TaskSubTypeId },
                    { "@SalesOrgId", task.SalesOrgId },
                    { "@StartDate", task.StartDate },
                    { "@EndDate", task.EndDate },
                    { "@IsActive", task.IsActive },
                    { "@Priority", task.Priority },
                    { "@Status", task.Status },
                    { "@TaskData", task.TaskData },
                    { "@CreatedBy", task.CreatedBy },
                    { "@CreatedTime", task.CreatedTime },
                    { "@ServerAddTime", DateTime.UtcNow }
                };

                return await ExecuteScalarAsync<int>(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> UpdateTask(ITask task)
        {
            try
            {
                var sql = @"UPDATE task SET title = @Title, description = @Description, task_type_id = @TaskTypeId,
                           task_sub_type_id = @TaskSubTypeId, task_type_uid = (SELECT uid FROM list_item WHERE id = @TaskTypeId AND list_header_uid = (SELECT uid FROM list_header WHERE code = 'TaskType')), 
                           sales_org_id = @SalesOrgId, start_date = @StartDate,
                           end_date = @EndDate, is_active = @IsActive, priority = @Priority, status = @Status,
                           task_data = @TaskData, modified_by = @ModifiedBy, modified_time = @ModifiedTime,
                           server_modified_time = @ServerModifiedTime WHERE uid = @UID";

                var parameters = new Dictionary<string, object>
                {
                    { "@UID", task.UID },
                    { "@Title", task.Title },
                    { "@Description", task.Description },
                    { "@TaskTypeId", task.TaskTypeId },
                    { "@TaskSubTypeId", task.TaskSubTypeId },
                    { "@SalesOrgId", task.SalesOrgId },
                    { "@StartDate", task.StartDate },
                    { "@EndDate", task.EndDate },
                    { "@IsActive", task.IsActive },
                    { "@Priority", task.Priority },
                    { "@Status", task.Status },
                    { "@TaskData", task.TaskData },
                    { "@ModifiedBy", task.ModifiedBy },
                    { "@ModifiedTime", task.ModifiedTime },
                    { "@ServerModifiedTime", DateTime.UtcNow }
                };

                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> CUDTask(ITask task)
        {
            try
            {
                if (task.Id == 0)
                {
                    return await CreateTask(task);
                }
                else
                {
                    return await UpdateTask(task);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> DeleteTask(string uID)
        {
            try
            {
                var sql = "DELETE FROM task WHERE uid = @UID";
                var parameters = new Dictionary<string, object> { { "@UID", uID } };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region Task Type Operations - DEPRECATED: Now using list_header/list_item structure

        // Task types are now managed through list_header/list_item structure
        // These methods are deprecated and should not be used
        
        [Obsolete("Use list_item service instead. Task types are now managed through list_header/list_item structure.")]
        public async Task<List<ITaskType>> GetAllTaskTypes()
        {
            // Return empty list since we no longer use dedicated task_type table
            return new List<ITaskType>();
        }

        [Obsolete("Use list_item service instead. Task types are now managed through list_header/list_item structure.")]
        public async Task<ITaskType> GetTaskTypeByUID(string UID)
        {
            // Return null since we no longer use dedicated task_type table
            return null;
        }

        [Obsolete("Use list_item service instead. Task types are now managed through list_header/list_item structure.")]
        public async Task<int> CreateTaskType(ITaskType taskType)
        {
            throw new NotSupportedException("Task types are now managed through list_header/list_item structure. Use the appropriate list management service instead.");
        }

        [Obsolete("Use list_item service instead. Task types are now managed through list_header/list_item structure.")]
        public async Task<int> UpdateTaskType(ITaskType taskType)
        {
            throw new NotSupportedException("Task types are now managed through list_header/list_item structure. Use the appropriate list management service instead.");
        }

        [Obsolete("Use list_item service instead. Task types are now managed through list_header/list_item structure.")]
        public async Task<int> DeleteTaskType(string uID)
        {
            throw new NotSupportedException("Task types are now managed through list_header/list_item structure. Use the appropriate list management service instead.");
        }

        #endregion

        #region Task Sub Type Operations - DEPRECATED: Now using list_header/list_item structure

        // Task sub types are no longer supported in the simplified list_item structure
        // These methods are deprecated and should not be used
        
        [Obsolete("Task sub types are no longer supported. Use main task types through list_header/list_item structure.")]
        public async Task<List<ITaskSubType>> GetTaskSubTypesByTaskType(int taskTypeId)
        {
            // Return empty list since we no longer use task sub types
            return new List<ITaskSubType>();
        }

        [Obsolete("Task sub types are no longer supported. Use main task types through list_header/list_item structure.")]
        public async Task<ITaskSubType> GetTaskSubTypeByUID(string UID)
        {
            // Return null since we no longer use task sub types
            return null;
        }

        [Obsolete("Task sub types are no longer supported. Use main task types through list_header/list_item structure.")]
        public async Task<int> CreateTaskSubType(ITaskSubType taskSubType)
        {
            throw new NotSupportedException("Task sub types are no longer supported. Use main task types through list_header/list_item structure instead.");
        }

        [Obsolete("Task sub types are no longer supported. Use main task types through list_header/list_item structure.")]
        public async Task<int> UpdateTaskSubType(ITaskSubType taskSubType)
        {
            throw new NotSupportedException("Task sub types are no longer supported. Use main task types through list_header/list_item structure instead.");
        }

        [Obsolete("Task sub types are no longer supported. Use main task types through list_header/list_item structure.")]
        public async Task<int> DeleteTaskSubType(string uID)
        {
            throw new NotSupportedException("Task sub types are no longer supported. Use main task types through list_header/list_item structure instead.");
        }

        #endregion

        #region Task Assignment Operations

        public async Task<List<ITaskAssignment>> GetTaskAssignments(int taskId)
        {
            try
            {
                var sql = @"SELECT ta.id AS Id, ta.uid AS UID, ta.created_by AS CreatedBy,
                           ta.created_time AS CreatedTime, ta.modified_by AS ModifiedBy, ta.modified_time AS ModifiedTime,
                           ta.server_add_time AS ServerAddTime, ta.server_modified_time AS ServerModifiedTime, ta.ss AS SS,
                           ta.task_id AS TaskId, ta.assigned_to_type AS AssignedToType, ta.user_id AS UserId,
                           ta.user_group_id AS UserGroupId, ta.status AS Status, ta.assigned_date AS AssignedDate,
                           ta.started_date AS StartedDate, ta.completed_date AS CompletedDate, ta.notes AS Notes,
                           ta.progress AS Progress
                           FROM task_assignment ta WHERE ta.task_id = @TaskId ORDER BY ta.assigned_date";

                var parameters = new Dictionary<string, object> { { "@TaskId", taskId } };
                return (await ExecuteQueryAsync<ITaskAssignment>(sql, parameters)).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<ITaskAssignment>> GetUserTaskAssignments(int userId)
        {
            try
            {
                var sql = @"SELECT ta.id AS Id, ta.uid AS UID, ta.created_by AS CreatedBy,
                           ta.created_time AS CreatedTime, ta.modified_by AS ModifiedBy, ta.modified_time AS ModifiedTime,
                           ta.server_add_time AS ServerAddTime, ta.server_modified_time AS ServerModifiedTime, ta.ss AS SS,
                           ta.task_id AS TaskId, ta.assigned_to_type AS AssignedToType, ta.user_id AS UserId,
                           ta.user_group_id AS UserGroupId, ta.status AS Status, ta.assigned_date AS AssignedDate,
                           ta.started_date AS StartedDate, ta.completed_date AS CompletedDate, ta.notes AS Notes,
                           ta.progress AS Progress
                           FROM task_assignment ta WHERE ta.user_id = @UserId ORDER BY ta.assigned_date DESC";

                var parameters = new Dictionary<string, object> { { "@UserId", userId } };
                return (await ExecuteQueryAsync<ITaskAssignment>(sql, parameters)).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<ITaskAssignment>> GetUserGroupTaskAssignments(int userGroupId)
        {
            try
            {
                var sql = @"SELECT ta.id AS Id, ta.uid AS UID, ta.created_by AS CreatedBy,
                           ta.created_time AS CreatedTime, ta.modified_by AS ModifiedBy, ta.modified_time AS ModifiedTime,
                           ta.server_add_time AS ServerAddTime, ta.server_modified_time AS ServerModifiedTime, ta.ss AS SS,
                           ta.task_id AS TaskId, ta.assigned_to_type AS AssignedToType, ta.user_id AS UserId,
                           ta.user_group_id AS UserGroupId, ta.status AS Status, ta.assigned_date AS AssignedDate,
                           ta.started_date AS StartedDate, ta.completed_date AS CompletedDate, ta.notes AS Notes,
                           ta.progress AS Progress
                           FROM task_assignment ta WHERE ta.user_group_id = @UserGroupId ORDER BY ta.assigned_date DESC";

                var parameters = new Dictionary<string, object> { { "@UserGroupId", userGroupId } };
                return (await ExecuteQueryAsync<ITaskAssignment>(sql, parameters)).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> CreateTaskAssignment(ITaskAssignment assignment)
        {
            try
            {
                var sql = @"INSERT INTO task_assignment (uid, task_id, assigned_to_type, user_id, user_group_id, 
                           status, assigned_date, notes, progress, created_by, created_time, server_add_time) 
                           VALUES (@UID, @TaskId, @AssignedToType, @UserId, @UserGroupId, @Status, @AssignedDate, 
                           @Notes, @Progress, @CreatedBy, @CreatedTime, @ServerAddTime) RETURNING id";

                var parameters = new Dictionary<string, object>
                {
                    { "@UID", assignment.UID },
                    { "@TaskId", assignment.TaskId },
                    { "@AssignedToType", assignment.AssignedToType },
                    { "@UserId", assignment.UserId },
                    { "@UserGroupId", assignment.UserGroupId },
                    { "@Status", assignment.Status },
                    { "@AssignedDate", assignment.AssignedDate },
                    { "@Notes", assignment.Notes },
                    { "@Progress", assignment.Progress },
                    { "@CreatedBy", assignment.CreatedBy },
                    { "@CreatedTime", assignment.CreatedTime },
                    { "@ServerAddTime", DateTime.UtcNow }
                };

                return await ExecuteScalarAsync<int>(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> UpdateTaskAssignment(ITaskAssignment assignment)
        {
            try
            {
                var sql = @"UPDATE task_assignment SET status = @Status, started_date = @StartedDate, 
                           completed_date = @CompletedDate, notes = @Notes, progress = @Progress,
                           modified_by = @ModifiedBy, modified_time = @ModifiedTime, 
                           server_modified_time = @ServerModifiedTime WHERE uid = @UID";

                var parameters = new Dictionary<string, object>
                {
                    { "@UID", assignment.UID },
                    { "@Status", assignment.Status },
                    { "@StartedDate", assignment.StartedDate },
                    { "@CompletedDate", assignment.CompletedDate },
                    { "@Notes", assignment.Notes },
                    { "@Progress", assignment.Progress },
                    { "@ModifiedBy", assignment.ModifiedBy },
                    { "@ModifiedTime", assignment.ModifiedTime },
                    { "@ServerModifiedTime", DateTime.UtcNow }
                };

                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> DeleteTaskAssignment(string uID)
        {
            try
            {
                var sql = "DELETE FROM task_assignment WHERE uid = @UID";
                var parameters = new Dictionary<string, object> { { "@UID", uID } };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> BulkAssignTasks(AssignTaskRequest request)
        {
            try
            {
                int totalAssigned = 0;

                // Assign to individual users
                if (request.UserIds != null && request.UserIds.Any())
                {
                    foreach (var userId in request.UserIds)
                    {
                        var assignment = new TaskAssignment
                        {
                            UID = Guid.NewGuid().ToString(),
                            TaskId = request.TaskId,
                            AssignedToType = "User",
                            UserId = userId,
                            Status = "Assigned",
                            AssignedDate = DateTime.UtcNow,
                            Notes = request.Notes,
                            Progress = 0,
                            CreatedBy = "System", // This should come from the current user context
                            CreatedTime = DateTime.UtcNow
                        };

                        await CreateTaskAssignment(assignment);
                        totalAssigned++;
                    }
                }

                // Assign to user groups
                if (request.UserGroupIds != null && request.UserGroupIds.Any())
                {
                    foreach (var userGroupId in request.UserGroupIds)
                    {
                        var assignment = new TaskAssignment
                        {
                            UID = Guid.NewGuid().ToString(),
                            TaskId = request.TaskId,
                            AssignedToType = "UserGroup",
                            UserGroupId = userGroupId,
                            Status = "Assigned",
                            AssignedDate = DateTime.UtcNow,
                            Notes = request.Notes,
                            Progress = 0,
                            CreatedBy = "System", // This should come from the current user context
                            CreatedTime = DateTime.UtcNow
                        };

                        await CreateTaskAssignment(assignment);
                        totalAssigned++;
                    }
                }

                return totalAssigned;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region Advanced Query Operations

        public async Task<List<TaskDTO>> GetTasksByFilter(TaskFilterRequest filter)
        {
            try
            {
                var sql = new StringBuilder(@"
                    SELECT t.id AS Id, t.uid AS UID, t.code AS Code, t.title AS Title, t.description AS Description,
                           t.task_type_id AS TaskTypeId, li.name AS TaskTypeName, t.task_sub_type_id AS TaskSubTypeId, 
                           '' AS TaskSubTypeName, t.sales_org_id AS SalesOrgId, t.start_date AS StartDate,
                           t.end_date AS EndDate, t.is_active AS IsActive, t.priority AS Priority, t.status AS Status,
                           t.task_data AS TaskData, t.created_time AS CreatedTime, t.created_by AS CreatedBy
                    FROM task t
                    LEFT JOIN list_item li ON t.task_type_uid = li.uid
                    WHERE 1=1");

                var parameters = new Dictionary<string, object>();

                if (filter.TaskTypeId.HasValue)
                {
                    sql.Append(" AND t.task_type_id = @TaskTypeId");
                    parameters.Add("@TaskTypeId", filter.TaskTypeId.Value);
                }

                if (filter.TaskSubTypeId.HasValue)
                {
                    sql.Append(" AND t.task_sub_type_id = @TaskSubTypeId");
                    parameters.Add("@TaskSubTypeId", filter.TaskSubTypeId.Value);
                }

                if (filter.SalesOrgId.HasValue)
                {
                    sql.Append(" AND t.sales_org_id = @SalesOrgId");
                    parameters.Add("@SalesOrgId", filter.SalesOrgId.Value);
                }

                if (!string.IsNullOrEmpty(filter.Status))
                {
                    sql.Append(" AND t.status = @Status");
                    parameters.Add("@Status", filter.Status);
                }

                if (filter.StartDate.HasValue)
                {
                    sql.Append(" AND t.start_date >= @StartDate");
                    parameters.Add("@StartDate", filter.StartDate.Value);
                }

                if (filter.EndDate.HasValue)
                {
                    sql.Append(" AND t.end_date <= @EndDate");
                    parameters.Add("@EndDate", filter.EndDate.Value);
                }

                if (filter.IsActive.HasValue)
                {
                    sql.Append(" AND t.is_active = @IsActive");
                    parameters.Add("@IsActive", filter.IsActive.Value);
                }

                if (filter.AssignedUserId.HasValue || filter.AssignedUserGroupId.HasValue)
                {
                    sql.Append(" AND t.id IN (SELECT DISTINCT ta.task_id FROM task_assignment ta WHERE ");
                    if (filter.AssignedUserId.HasValue)
                    {
                        sql.Append("ta.user_id = @AssignedUserId");
                        parameters.Add("@AssignedUserId", filter.AssignedUserId.Value);
                    }
                    if (filter.AssignedUserGroupId.HasValue)
                    {
                        if (filter.AssignedUserId.HasValue) sql.Append(" OR ");
                        sql.Append("ta.user_group_id = @AssignedUserGroupId");
                        parameters.Add("@AssignedUserGroupId", filter.AssignedUserGroupId.Value);
                    }
                    sql.Append(")");
                }

                sql.Append(" ORDER BY t.created_time DESC");

                if (filter.PageSize > 0)
                {
                    sql.Append($" OFFSET {(filter.PageNumber - 1) * filter.PageSize} ROWS FETCH NEXT {filter.PageSize} ROWS ONLY");
                }

                var tasks = await ExecuteQueryAsync<TaskDTO>(sql.ToString(), parameters);
                return tasks.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<TaskDTO>> GetTasksDashboard(int? userId, int? userGroupId, int? salesOrgId)
        {
            try
            {
                var sql = new StringBuilder(@"
                    SELECT DISTINCT t.id AS Id, t.uid AS UID, t.code AS Code, t.title AS Title, t.description AS Description,
                           t.task_type_id AS TaskTypeId, li.name AS TaskTypeName, t.task_sub_type_id AS TaskSubTypeId, 
                           '' AS TaskSubTypeName, t.sales_org_id AS SalesOrgId, t.start_date AS StartDate,
                           t.end_date AS EndDate, t.is_active AS IsActive, t.priority AS Priority, t.status AS Status,
                           t.task_data AS TaskData, t.created_time AS CreatedTime, t.created_by AS CreatedBy
                    FROM task t
                    LEFT JOIN list_item li ON t.task_type_uid = li.uid
                    LEFT JOIN task_assignment ta ON t.id = ta.task_id
                    WHERE t.is_active = true");

                var parameters = new Dictionary<string, object>();

                if (userId.HasValue)
                {
                    sql.Append(" AND ta.user_id = @UserId");
                    parameters.Add("@UserId", userId.Value);
                }

                if (userGroupId.HasValue)
                {
                    sql.Append(" AND ta.user_group_id = @UserGroupId");
                    parameters.Add("@UserGroupId", userGroupId.Value);
                }

                if (salesOrgId.HasValue)
                {
                    sql.Append(" AND t.sales_org_id = @SalesOrgId");
                    parameters.Add("@SalesOrgId", salesOrgId.Value);
                }

                sql.Append(" ORDER BY t.start_date ASC, t.priority DESC");

                var tasks = await ExecuteQueryAsync<TaskDTO>(sql.ToString(), parameters);
                return tasks.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetTaskStatusCounts(int? userId, int? salesOrgId)
        {
            try
            {
                var sql = new StringBuilder(@"
                    SELECT t.status, COUNT(*) as Count
                    FROM task t
                    LEFT JOIN task_assignment ta ON t.id = ta.task_id
                    WHERE t.is_active = true");

                var parameters = new Dictionary<string, object>();

                if (userId.HasValue)
                {
                    sql.Append(" AND ta.user_id = @UserId");
                    parameters.Add("@UserId", userId.Value);
                }

                if (salesOrgId.HasValue)
                {
                    sql.Append(" AND t.sales_org_id = @SalesOrgId");
                    parameters.Add("@SalesOrgId", salesOrgId.Value);
                }

                sql.Append(" GROUP BY t.status");

                var result = await ExecuteQueryAsync<dynamic>(sql.ToString(), parameters);
                var statusCounts = new Dictionary<string, int>();

                foreach (var item in result)
                {
                    var itemDict = (IDictionary<string, object>)item;
                    statusCounts[itemDict["status"].ToString()] = Convert.ToInt32(itemDict["count"]);
                }

                return statusCounts;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
    }
}