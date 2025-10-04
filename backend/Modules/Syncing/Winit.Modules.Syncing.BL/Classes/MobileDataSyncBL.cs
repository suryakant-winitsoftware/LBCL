using Azure;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CaptureCompetitor.Model.Classes;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.JourneyPlan.Model.Classes;
using Winit.Modules.ReturnOrder.Model.Classes;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.StoreCheck.Model.Classes;
using Winit.Modules.Syncing.BL.Interfaces;
using Winit.Modules.Syncing.DL.Interfaces;
using Winit.Modules.Syncing.Model.Classes;
using Winit.Modules.Syncing.Model.Interfaces;
using Winit.Modules.WHStock.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Configuration;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Winit.Modules.Syncing.BL.Classes
{
    public class MobileDataSyncBL : MobileDataSyncBaseBL, Interfaces.IMobileDataSyncBL
    {
        private Winit.Modules.Base.BL.ApiService _apiService;
        protected Winit.Shared.Models.Common.IAppConfig _appConfigs;
        protected readonly DL.Interfaces.IMobileDataSyncDL _mobileDataSyncDL;
        private IAppRequestBL _appRequestBL;
        private IServiceProvider _serviceProvider;
        private readonly SyncSettings _syncSettings;

        public MobileDataSyncBL(Base.BL.ApiService apiService, IAppConfig appConfigs, DL.Interfaces.IMobileDataSyncDL mobileDataSyncDL, IServiceProvider serviceProvider,
            IAppRequestBL appRequestBL, IOptions<SyncSettings> syncSettings)
        {

            _apiService = apiService;
            _appConfigs = appConfigs;
            _mobileDataSyncDL = mobileDataSyncDL;
            _serviceProvider = serviceProvider;
            _appRequestBL = appRequestBL;
            _syncSettings = syncSettings.Value;
        }
        public async Task CreateDeleteDynamicTable(bool action, string jobPositionUID, string dynamicTableName)
        {
            try
            {
                int dynamicTable = await _mobileDataSyncDL.CreateDynamicTable(action, jobPositionUID, dynamicTableName);
            }
            catch (Exception)
            {

            }
        }
        public async Task<string> DownloadServerDataInSqlite(string groupName, string tableName, string employeeUID, string jobPositionUID, 
            string roleUID, string vehicleUID, string orgUID, string empCode)
        {
            string userSqliteFilePath = string.Empty;

            DateTime Next3DayString = DateTime.Now.AddDays(3);
            DateTime Next6DayString = DateTime.Now.AddDays(6);
            DateTime Next7DayString = DateTime.Now.AddDays(7);
            DateTime Last2DayString = DateTime.Now.AddDays(-2);
            DateTime Last31DayString = DateTime.Now.AddDays(-31);
            DateTime CurrentMonthFirstDay = DateTime.Now.AddDays(-1 * DateTime.Now.Day);
            var weekRange = GetCurrentWeekRange();
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                {"@EmployeeUID", employeeUID },
                {"@JobPositionUID", jobPositionUID },
                {"@RoleUID", roleUID },
                {"@VehicleUID", vehicleUID },
                {"@OrgUID", orgUID },
                {"@Next3DayString", Next3DayString },
                {"@Next6DayString", Next6DayString },
                {"@Next7DayString", Next7DayString },
                {"@Last2DayString", Last2DayString },
                {"@Last31DayString", Last31DayString },
                {"@CurrentMonthFirstDay", CurrentMonthFirstDay },
                {"@WeekStartDate", weekRange.StartDate.ToString("yyyy-MM-dd") },
                {"@WeekEndDate", weekRange.EndDate.ToString("yyyy-MM-dd") }
            };
            SyncLog syncLog = new SyncLog
            {
                MethodName = "DownloadServerDataInSqlite",
                EmployeeUID = employeeUID,
                JobPositionUID = jobPositionUID,
                RoleUID = roleUID,
                VehicleUID = vehicleUID,
                StartTime = DateTime.Now
            };

            try
            {
                List<ITableGroupEntityView> tableGroupEntityList = await _mobileDataSyncDL.GetTablesToSync(groupName, tableName);
                if(tableGroupEntityList.Any(p => p.TableName == DbTableName.SkuClass))
                {

                }
                if (tableGroupEntityList == null || tableGroupEntityList.Count == 0)
                {
                    return userSqliteFilePath;
                }
                userSqliteFilePath = CreateSQLIteBaseFile(employeeUID);

                //this will create dynamic table at login time
                string timestamp = DateTime.Now.ToString("ddMMyyHHmmss");
                string dynamicTableName = $"{empCode}_store_{timestamp}";
                await CreateDeleteDynamicTable(true, jobPositionUID, dynamicTableName);

                // Process tables - either parallel or sequential based on configuration
                if (_syncSettings.EnableParallelSync)
                {
                    await ProcessTablesInParallel(tableGroupEntityList, parameters, dynamicTableName, userSqliteFilePath, syncLog);
                }
                else
                {
                    await ProcessTablesSequentially(tableGroupEntityList, parameters, dynamicTableName, userSqliteFilePath, syncLog);
                }
                await CreateDeleteDynamicTable(false, null, dynamicTableName);
            }
            catch (Exception ex)
            {
                syncLog.Message = ex.Message + "\n" + ex.StackTrace;
                throw new Exception(syncLog.Message);
            }
            finally
            {
                syncLog.EndTime = DateTime.Now;
            }

            // Logging the Log
            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.None
            };
            string syncLogJson = JsonConvert.SerializeObject(syncLog, jsonSettings);
            CommonFunctions.LogSyncLogJsonToFile(syncLogJson, employeeUID, "SqliteLog");

            return userSqliteFilePath;
        }
        public static (DateTime StartDate, DateTime EndDate) GetCurrentWeekRange()
        {
            DateTime today = DateTime.Today;
            int diff = (int)today.DayOfWeek - (int)DayOfWeek.Monday;
            if (diff < 0) // If today is Sunday, move back to previous Monday
                diff += 7;

            DateTime startDate = today.AddDays(-diff);
            DateTime endDate = startDate.AddDays(6);

            return (startDate, endDate);
        }

        public async Task<Dictionary<string, object>> DownloadServerDataInSync(string groupName, string tableName, string employeeUID, string jobPositionUID,
            string roleUID, string vehicleUID, string orgUID, string EmpCode, DateTime lastSyncTime)
        {
            Dictionary<string, object> syncResponse = new Dictionary<string, object>();

            // Create an array of SqlParameter
            /*
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@EmployeeUID", employeeUID),
                new SqlParameter("@JobPositionUID", jobPositionUID),
                new SqlParameter("@RoleUID", roleUID),
                new SqlParameter("@VehicleUID", vehicleUID),
                new SqlParameter("@OrgUID", orgUID),
                new SqlParameter("@ServerModifiedTime", lastSyncTime),
            };
            */


            DateTime Next3DayString = DateTime.Now.AddDays(3);
            DateTime Next6DayString = DateTime.Now.AddDays(6);
            DateTime Next7DayString = DateTime.Now.AddDays(7);
            DateTime Last2DayString = DateTime.Now.AddDays(-2);
            DateTime Last31DayString = DateTime.Now.AddDays(-31);
            DateTime CurrentMonthFirstDay = DateTime.Now.AddDays(-1 * DateTime.Now.Day);

            // Example usage
            var weekRange = GetCurrentWeekRange();
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                {"@EmployeeUID", employeeUID },
                {"@JobPositionUID", jobPositionUID },
                {"@RoleUID", roleUID },
                {"@VehicleUID", vehicleUID },
                {"@OrgUID", orgUID },
                {"@ServerModifiedTime", lastSyncTime },
                {"@Next3DayString", Next3DayString },
                {"@Next6DayString", Next6DayString },
                {"@Next7DayString", Next7DayString },
                {"@Last2DayString", Last2DayString },
                {"@Last31DayString", Last31DayString },
                {"@CurrentMonthFirstDay", CurrentMonthFirstDay },
                {"@WeekStartDate", weekRange.StartDate },
                {"@WeekEndDate", weekRange.EndDate }
            };
            SyncLog syncLog = new SyncLog
            {
                MethodName = "DownloadServerDataInSync",
                EmployeeUID = employeeUID,
                JobPositionUID = jobPositionUID,
                RoleUID = roleUID,
                VehicleUID = vehicleUID,
                LastSyncTime = lastSyncTime,
                StartTime = DateTime.Now
            };

            try
            {
                List<ITableGroupEntityView> tableGroupEntityList = await _mobileDataSyncDL.GetTablesToSync(groupName, tableName);

                //this will create dynamic table at login time
                string timestamp = DateTime.Now.ToString("ddMMyyHHmmss");
                string dynamicTableName = $"{EmpCode}_store_{timestamp}";
                await CreateDeleteDynamicTable(true, jobPositionUID, dynamicTableName);

                foreach (var tableGroupEntity in tableGroupEntityList)
                {
                    TableSyncDetail syncDetail = new TableSyncDetail
                    {
                        TableName = tableGroupEntity.TableName
                    };
                    syncLog.TableSyncDetails.Add(syncDetail);

                    if (string.IsNullOrEmpty(tableGroupEntity.SyncDataQuery) || string.IsNullOrEmpty(tableGroupEntity.ModelName))
                    {
                        syncDetail.Status = "Failed";
                        syncDetail.Message = "No SyncDataQuery or ModelName exists";
                        continue;
                    }
                    TimeSpan timeTaken;
                    DateTime startTime = DateTime.Now;

                    try
                    {
                        //this will replace dynamic table name at login time

                        if (tableGroupEntity.SyncDataQuery.Contains("{{DynamicTable}}"))
                        {
                            tableGroupEntity.SyncDataQuery = tableGroupEntity.SyncDataQuery.Replace("{{DynamicTable}}", dynamicTableName);
                        }
                        // Read data from server database table
                        List<object> data = await GetDataFromDatabase(tableGroupEntity.SyncDataQuery, parameters, tableGroupEntity.ModelName);
                        if (data != null && data.Count > 0)
                        {
                            syncResponse[tableGroupEntity.TableName] = data;
                            syncDetail.RecordCount = data.Count;
                        }
                        syncDetail.Status = "Success";
                    }
                    catch (Exception ex)
                    {
                        syncDetail.Status = "Failed";
                        syncDetail.Message = ex.Message + "\n" + ex.StackTrace;
                        Console.WriteLine($"Error syncing data for table {tableGroupEntity.TableName}: {ex.Message}");
                    }
                    finally
                    {
                        DateTime endTime = DateTime.Now;
                        timeTaken = endTime - startTime;
                        syncDetail.TimeTaken = timeTaken;
                    }
                }
                await CreateDeleteDynamicTable(false, null, dynamicTableName);
            }
            catch (Exception ex)
            {
                syncLog.Message = ex.Message + "\n" + ex.StackTrace;
                Console.WriteLine($"Error syncing data: {ex.Message}");
            }
            finally
            {
                syncLog.EndTime = DateTime.Now;
            }

            // Logging the Log
            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.None
            };
            string syncLogJson = JsonConvert.SerializeObject(syncLog, jsonSettings);
            CommonFunctions.LogSyncLogJsonToFile(syncLogJson, employeeUID, "SyncLog");

            return syncResponse;
        }
        public async Task SyncDataForTableGroup(string groupName, string tableName, string employeeUID, string jobPositionUID, string roleUID, string vehicleUID, string orgUID, string EmpCode)
        {
            // Call overloaded method without progress reporting
            await SyncDataForTableGroup(groupName, tableName, employeeUID, jobPositionUID, roleUID, vehicleUID, orgUID, EmpCode, null);
        }

        public async Task SyncDataForTableGroup(string groupName, string tableName, string employeeUID, string jobPositionUID, string roleUID, string vehicleUID, string orgUID, string EmpCode, IProgress<Winit.Modules.Syncing.Model.Classes.SyncProgress> progress, bool showTableLevelErrors = false)
        {
            List<string> failedGroups = new List<string>();
            List<string> successfulGroups = new List<string>();
            List<string> allFailedTables = new List<string>();
            
            try
            {
                List<ITableGroup> tableGroups = await _mobileDataSyncDL.GetTableGroupToSync(groupName);
                
                for (int groupIndex = 0; groupIndex < tableGroups.Count; groupIndex++)
                {
                    ITableGroup tableGroup = tableGroups[groupIndex];
                    
                    try
                    {
                        // Report group start progress
                        progress?.Report(new Winit.Modules.Syncing.Model.Classes.SyncProgress
                        {
                            Stage = "Starting Group Sync",
                            GroupName = tableGroup.GroupName,
                            CurrentGroupIndex = groupIndex,
                            TotalGroups = tableGroups.Count,
                            PercentageComplete = (groupIndex * 100) / tableGroups.Count,
                            Message = $"Starting sync for group {tableGroup.GroupName}"
                        });

                        ISyncRequest syncRequest = _serviceProvider.CreateInstance<ISyncRequest>();
                        syncRequest.GroupName = tableGroup.GroupName;
                        syncRequest.TableName = tableName;
                        syncRequest.EmployeeUID = employeeUID;
                        syncRequest.EmployeeCode = EmpCode;
                        syncRequest.JobPositionUID = jobPositionUID;
                        syncRequest.RoleUID = roleUID;
                        syncRequest.VehicleUID = vehicleUID;
                        syncRequest.LastSyncTime = tableGroup.LastDownloadTime ?? DateTime.Now;
                        syncRequest.OrgUID = orgUID;

                        List<string> groupFailedTables = await SyncDataFromServerToSQLite(syncRequest, progress, groupIndex, tableGroups.Count, showTableLevelErrors);
                        
                        // Collect failed tables from this group
                        if (groupFailedTables != null && groupFailedTables.Count > 0)
                        {
                            allFailedTables.AddRange(groupFailedTables.Select(table => $"{tableGroup.GroupName}.{table}"));
                            
                            // Mark group as failed if any table failed
                            failedGroups.Add(tableGroup.GroupName);
                            Console.WriteLine($"Group: {tableGroup.GroupName} completed with {groupFailedTables.Count} failed table(s)");
                        }
                        else
                        {
                            // Track successful group (no table failures)
                            successfulGroups.Add(tableGroup.GroupName);
                            Console.WriteLine($"Successfully completed sync for group '{tableGroup.GroupName}'");
                        }
                    }
                    catch (Exception groupEx)
                    {
                        // Track failed group and continue with others
                        failedGroups.Add(tableGroup.GroupName);
                        string errorMessage = $"Failed to sync group: {tableGroup.GroupName}: {groupEx.Message}";
                        
                        Console.WriteLine($"Group sync error: {errorMessage}");
                        
                        // Report group failure but continue
                        progress?.Report(new Winit.Modules.Syncing.Model.Classes.SyncProgress
                        {
                            Stage = "Group Failed",
                            GroupName = tableGroup.GroupName,
                            CurrentGroupIndex = groupIndex,
                            TotalGroups = tableGroups.Count,
                            ErrorMessage = errorMessage,
                            Message = $"Failed to sync group{tableGroup.GroupName}' - continuing with other groups"
                        });
                        
                        // Continue with the next group
                        continue;
                    }
                }

                // Report overall completion with summary
                string completionMessage;
                string stage;
                if (failedGroups.Count == 0)
                {
                    completionMessage = $"Sync completed successfully - All {successfulGroups.Count} groups synced";
                    stage = "Completed";
                }
                else if (successfulGroups.Count == 0)
                {
                    completionMessage = $"Sync completed with errors - All {failedGroups.Count} groups failed";
                    stage = "Completed with Errors";
                }
                else
                {
                    completionMessage = $"Sync partially completed - {successfulGroups.Count} groups successful, {failedGroups.Count} groups failed";
                    stage = "Completed with Errors";
                }
                
                progress?.Report(new Winit.Modules.Syncing.Model.Classes.SyncProgress
                {
                    Stage = stage,
                    IsCompleted = true,
                    PercentageComplete = 100,
                    Message = completionMessage,
                    ErrorMessage = failedGroups.Count > 0 ? $"Failed groups: {string.Join(", ", failedGroups)}" : null
                });
            }
            catch (Exception ex)
            {
                // Report error
                progress?.Report(new Winit.Modules.Syncing.Model.Classes.SyncProgress
                {
                    Stage = "Error",
                    ErrorMessage = ex.Message,
                    PercentageComplete = 0,
                    Message = $"Sync failed: {ex.Message}"
                });
                throw;
            }
        }

        /// <summary>
        /// Syncs data for multiple table groups efficiently.
        /// Provides better performance when syncing multiple specific groups compared to calling SyncDataForTableGroup multiple times.
        /// </summary>
        /// <param name="groupNames">List of group names to sync (empty list for all groups)</param>
        /// <param name="tableName">Table name to sync (empty string for all tables in groups)</param>
        /// <param name="employeeUID">Employee UID</param>
        /// <param name="jobPositionUID">Job Position UID</param>
        /// <param name="roleUID">Role UID</param>
        /// <param name="vehicleUID">Vehicle UID</param>
        /// <param name="orgUID">Organization UID</param>
        public async Task SyncDataForTableGroups(List<string> groupNames, string tableName, string employeeUID, string jobPositionUID, string roleUID, string vehicleUID, string orgUID, string EmpCode)
        {
            // Call overloaded method without progress reporting
            await SyncDataForTableGroups(groupNames, tableName, employeeUID, jobPositionUID, roleUID, vehicleUID, orgUID, EmpCode, null);
        }

        /// <summary>
        /// Syncs data for multiple table groups efficiently with progress reporting.
        /// Provides better performance when syncing multiple specific groups compared to calling SyncDataForTableGroup multiple times.
        /// </summary>
        /// <param name="groupNames">List of group names to sync (empty list for all groups)</param>
        /// <param name="tableName">Table name to sync (empty string for all tables in groups)</param>
        /// <param name="employeeUID">Employee UID</param>
        /// <param name="jobPositionUID">Job Position UID</param>
        /// <param name="roleUID">Role UID</param>
        /// <param name="vehicleUID">Vehicle UID</param>
        /// <param name="orgUID">Organization UID</param>
        /// <param name="progress">Progress callback for reporting sync progress</param>
        public async Task SyncDataForTableGroups(List<string> groupNames, string tableName, string employeeUID, string jobPositionUID, string roleUID, string vehicleUID, string orgUID, string EmpCode, IProgress<Winit.Modules.Syncing.Model.Classes.SyncProgress> progress, bool showTableLevelErrors = false)
        {
            List<string> failedGroups = new List<string>();
            List<string> successfulGroups = new List<string>();
            
            try
            {
                if (groupNames == null || groupNames.Count == 0)
                {
                    // If no specific groups provided, sync all groups
                    await SyncDataForTableGroup("", tableName, employeeUID, jobPositionUID, roleUID, vehicleUID, orgUID, EmpCode, progress, showTableLevelErrors);
                    return;
                }

                int totalGroupsToProcess = groupNames.Count;
                int processedGroups = 0;

                // Process each group name in the list
                foreach (string groupName in groupNames)
                {
                    if (string.IsNullOrWhiteSpace(groupName))
                        continue;

                    List<ITableGroup> tableGroups = await _mobileDataSyncDL.GetTableGroupToSync(groupName);
                    
                    foreach (ITableGroup tableGroup in tableGroups)
                    {
                        try
                        {
                            // Report group start progress
                            progress?.Report(new Winit.Modules.Syncing.Model.Classes.SyncProgress
                            {
                                Stage = "Processing Group",
                                GroupName = tableGroup.GroupName,
                                CurrentGroupIndex = processedGroups,
                                TotalGroups = totalGroupsToProcess,
                                PercentageComplete = (processedGroups * 100) / totalGroupsToProcess,
                                Message = $"Processing group '{tableGroup.GroupName}' ({processedGroups + 1} of {totalGroupsToProcess})"
                            });

                            ISyncRequest syncRequest = _serviceProvider.CreateInstance<ISyncRequest>();
                            syncRequest.GroupName = tableGroup.GroupName;
                            syncRequest.TableName = tableName;
                            syncRequest.EmployeeUID = employeeUID;
                            syncRequest.EmployeeCode = EmpCode;
                            syncRequest.JobPositionUID = jobPositionUID;
                            syncRequest.RoleUID = roleUID;
                            syncRequest.VehicleUID = vehicleUID;
                            syncRequest.LastSyncTime = tableGroup.LastDownloadTime ?? DateTime.Now;
                            syncRequest.OrgUID = orgUID;
                            
                            List<string> groupFailedTables = await SyncDataFromServerToSQLite(syncRequest, progress, processedGroups, totalGroupsToProcess, showTableLevelErrors);
                            
                            // Check if group has any failed tables
                            if (groupFailedTables != null && groupFailedTables.Count > 0)
                            {
                                // Mark group as failed if any table failed
                                failedGroups.Add(tableGroup.GroupName);
                                Console.WriteLine($"Group '{tableGroup.GroupName}' completed with {groupFailedTables.Count} failed table(s)");
                            }
                            else
                            {
                                // Track successful group (no table failures)
                                successfulGroups.Add(tableGroup.GroupName);
                            }
                        }
                        catch (Exception groupEx)
                        {
                            // Track failed group and continue with others
                            failedGroups.Add(tableGroup.GroupName);
                            string errorMessage = $"Failed to sync group '{tableGroup.GroupName}': {groupEx.Message}";
                            
                            Console.WriteLine($"Group sync error: {errorMessage}");
                            
                            // Report group failure but continue
                            progress?.Report(new Winit.Modules.Syncing.Model.Classes.SyncProgress
                            {
                                Stage = "Group Failed",
                                GroupName = tableGroup.GroupName,
                                CurrentGroupIndex = processedGroups,
                                TotalGroups = totalGroupsToProcess,
                                ErrorMessage = errorMessage,
                                Message = $"Failed to sync group '{tableGroup.GroupName}' - continuing with other groups"
                            });
                            
                            // Continue with the next group
                            continue;
                        }
                    }
                    
                    processedGroups++;
                }

                // Report completion with summary
                string completionMessage;
                string stage;
                if (failedGroups.Count == 0)
                {
                    completionMessage = $"Multiple groups sync completed successfully - All {successfulGroups.Count} groups synced";
                    stage = "Completed";
                }
                else if (successfulGroups.Count == 0)
                {
                    completionMessage = $"Multiple groups sync completed with errors - All {failedGroups.Count} groups failed";
                    stage = "Completed with Errors";
                }
                else
                {
                    completionMessage = $"Multiple groups sync partially completed - {successfulGroups.Count} successful, {failedGroups.Count} failed";
                    stage = "Completed with Errors";
                }
                
                progress?.Report(new Winit.Modules.Syncing.Model.Classes.SyncProgress
                {
                    Stage = stage,
                    IsCompleted = true,
                    PercentageComplete = 100,
                    Message = completionMessage,
                    ErrorMessage = failedGroups.Count > 0 ? $"Failed groups: {string.Join(", ", failedGroups)}" : null
                });
            }
            catch (Exception ex)
            {
                // Report error
                progress?.Report(new Winit.Modules.Syncing.Model.Classes.SyncProgress
                {
                    Stage = "Error",
                    ErrorMessage = ex.Message,
                    PercentageComplete = 0,
                    Message = $"Multiple groups sync failed: {ex.Message}"
                });
                throw;
            }
        }
        private async Task SyncDataFromServerToSQLite(ISyncRequest requestData)
        {
            await SyncDataFromServerToSQLite(requestData, null, 0, 1);
        }

        private async Task<List<string>> SyncDataFromServerToSQLite(ISyncRequest requestData, IProgress<Winit.Modules.Syncing.Model.Classes.SyncProgress> progress, int currentGroupIndex, int totalGroups, bool showTableLevelErrors = false)
        {
            List<string> failedTables = new List<string>();
            List<string> successfulTables = new List<string>();
            
            try
            {
                // Report API call start
                progress?.Report(new Winit.Modules.Syncing.Model.Classes.SyncProgress
                {
                    Stage = "Fetching Data",
                    GroupName = requestData.GroupName,
                    CurrentGroupIndex = currentGroupIndex,
                    TotalGroups = totalGroups,
                    Message = $"Fetching data from server for group '{requestData.GroupName}'"
                });

                ApiResponse<Dictionary<string, List<dynamic>>> apiResponse = await _apiService.FetchDataAsync
                <Dictionary<string, List<dynamic>>>(
                $"{_appConfigs.ApiBaseUrl}MobileDataSync/DownloadServerDataInSync",
                HttpMethod.Post, requestData);

                if (apiResponse == null)
                {
                    throw new Exception("No response from API");
                }
                
                if (apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    List<ITableGroupEntityView> tableGroupEntityList = await _mobileDataSyncDL.GetTablesToSync(requestData.GroupName, string.Empty);
                    
                    for (int tableIndex = 0; tableIndex < tableGroupEntityList.Count; tableIndex++)
                    {
                        ITableGroupEntityView tableGroupEntityView = tableGroupEntityList[tableIndex];
                        
                        try
                        {
                            if (!apiResponse.Data.ContainsKey(tableGroupEntityView.TableName))
                            {
                                continue;
                            }
                            
                            List<dynamic> tableData = apiResponse.Data[tableGroupEntityView.TableName];
                            if (tableData == null || tableData.Count == 0)
                            {
                                continue;
                            }
                            
                            List<dynamic>? convertedList = CommonFunctions.ConvertJTokenListToType(_serviceProvider, tableData, tableGroupEntityView.ModelName);
                            if (convertedList == null || convertedList.Count == 0)
                            {
                                continue;
                            }

                            // Report table processing progress (only if showTableLevelErrors is true)
                            if (showTableLevelErrors)
                            {
                                progress?.Report(new Winit.Modules.Syncing.Model.Classes.SyncProgress
                                {
                                    Stage = "Processing Table",
                                    GroupName = requestData.GroupName,
                                    TableName = tableGroupEntityView.TableName,
                                    RecordCount = convertedList.Count,
                                    CurrentGroupIndex = currentGroupIndex,
                                    TotalGroups = totalGroups,
                                    CurrentTableIndex = tableIndex,
                                    TotalTablesInGroup = tableGroupEntityList.Count,
                                    PercentageComplete = ((currentGroupIndex * tableGroupEntityList.Count + tableIndex) * 100) / (totalGroups * tableGroupEntityList.Count),
                                    Message = $"Syncing group: {requestData.GroupName}, table: {tableGroupEntityView.TableName} - {convertedList.Count} records"
                                });
                                await Task.Delay(100); // Added delay to show message
                            }
                            
                            int retValue = await _mobileDataSyncDL.UpsertTableAsync(tableGroupEntityView.TableName, convertedList,
                                apiResponse.CurrentServerTime ?? DateTime.Now,
                                tableGroupEntityView.SqliteInsertQuery,
                                tableGroupEntityView.SqliteUpdateQuery);
                            
                            // Track successful table
                            successfulTables.Add(tableGroupEntityView.TableName);
                            
                            Console.WriteLine($"Successfully synced table: {tableGroupEntityView.TableName} with {convertedList.Count} records");
                        }
                        catch (Exception tableEx)
                        {
                            // Log table-specific error and continue with other tables
                            failedTables.Add(tableGroupEntityView.TableName);
                            string errorMessage = $"Failed to sync table: {tableGroupEntityView.TableName}, Detail: {tableEx.Message}";
                            
                            Console.WriteLine($"Table sync error: {errorMessage}");
                            
                            // Report table failure but continue (only if showTableLevelErrors is true)
                            if (showTableLevelErrors)
                            {
                                progress?.Report(new Winit.Modules.Syncing.Model.Classes.SyncProgress
                                {
                                    Stage = "Processing Table",
                                    GroupName = requestData.GroupName,
                                    TableName = tableGroupEntityView.TableName,
                                    CurrentGroupIndex = currentGroupIndex,
                                    TotalGroups = totalGroups,
                                    CurrentTableIndex = tableIndex,
                                    TotalTablesInGroup = tableGroupEntityList.Count,
                                    ErrorMessage = errorMessage,
                                    Message = $"Failed to process table: {tableGroupEntityView.TableName} - continuing with other tables"
                                });
                            }
                            
                            // Continue with the next table
                            continue;
                        }
                    }
                    
                    // Update LastDownloadTimeTime For Group (only if at least one table succeeded)
                    if (successfulTables.Count > 0)
                    {
                        try
                        {
                            await _mobileDataSyncDL.UpdateLastDownloadTimeForTableGroup(requestData.GroupName, apiResponse.CurrentServerTime ?? DateTime.Now);
                        }
                        catch (Exception updateEx)
                        {
                            Console.WriteLine($"Failed to update last download time for group: {requestData.GroupName}, Detail: {updateEx.Message}");
                        }
                    }
                    
                    // Report group completion with summary
                    string completionMessage;
                    if (failedTables.Count == 0)
                    {
                        completionMessage = $"Completed sync for group: {requestData.GroupName}. - All {successfulTables.Count} tables synced successfully";
                    }
                    else if (successfulTables.Count == 0)
                    {
                        completionMessage = $"Group: {requestData.GroupName} sync completed with errors - All {failedTables.Count} tables failed";
                    }
                    else
                    {
                        completionMessage = $"Group {requestData.GroupName} partially synced - {successfulTables.Count} successful, {failedTables.Count} failed";
                    }
                    
                    progress?.Report(new Winit.Modules.Syncing.Model.Classes.SyncProgress
                    {
                        Stage = failedTables.Count == 0 ? "Group Completed" : "Group Completed with Errors",
                        GroupName = requestData.GroupName,
                        CurrentGroupIndex = currentGroupIndex,
                        TotalGroups = totalGroups,
                        PercentageComplete = ((currentGroupIndex + 1) * 100) / totalGroups,
                        Message = completionMessage,
                        ErrorMessage = failedTables.Count > 0 ? $"Failed tables: {string.Join(", ", failedTables)}" : null
                    });
                }
                else
                {
                    // Handle unsuccessful response
                    throw new Exception($"Failed to fetch data from server. StatusCode: {apiResponse.StatusCode}, Error Message:{apiResponse.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                // Only throw for API-level errors, not table-level errors
                if (failedTables.Count > 0 && successfulTables.Count > 0)
                {
                    // Partial success - don't throw, just log
                    Console.WriteLine($"Group '{requestData.GroupName}' sync completed with mixed results: {ex.Message}");
                }
                else
                {
                    // Complete failure - re-throw
                    throw;
                }
            }
            
            // Return list of failed tables for this group
            return failedTables;
        }
        #region Sync Data From SQLite to Server
        public async Task SyncDataFromSQLiteToServer(string orgUID, string groupName, string empUID, string jobpositionUID)
        {
            List<IAppRequest>? appRequestList = null;
            switch (groupName)
            {
                case DbTableGroup.Sales:
                    appRequestList = await PrepareInsertUpdateData_SalesOrder(orgUID, empUID, jobpositionUID);
                    break;
                case DbTableGroup.Merchandiser:
                    appRequestList = await PrepareInsertUpdateData_Merchandiser(orgUID, empUID, jobpositionUID);
                    break;
                case DbTableGroup.StoreCheck:
                    appRequestList = await PrepareInsertUpdateData_StoreCheck(orgUID, empUID, jobpositionUID);
                    break;
                case DbTableGroup.StockRequest:
                    appRequestList = await PrepareInsertUpdateData_WHStockRequest(orgUID, empUID, jobpositionUID);
                    break;
                case DbTableGroup.Return:
                    appRequestList = await PrepareInsertUpdateData_Return(orgUID, empUID, jobpositionUID);
                    break;
                case DbTableGroup.Collection:
                    appRequestList = await PrepareInsertUpdateData_Collection(orgUID, empUID, jobpositionUID);
                    break;
                case DbTableGroup.CollectionDeposit:
                    appRequestList = await PrepareInsertUpdateData_CollectionDeposit(orgUID, empUID, jobpositionUID);
                    break;
                case DbTableGroup.Master:
                    appRequestList = await PrepareInsertUpdateData_Master(orgUID, empUID, jobpositionUID);
                    break;
                //case DbTableGroup.SurveyResponse:
                //    appRequestList = await PrepareInsertUpdateData_SurveyResponse(orgUID, empUID, jobpositionUID);

                //    break;
                case DbTableGroup.FileSys:
                    appRequestList = await PrepareInsertUpdateData_FileSys(orgUID, empUID, jobpositionUID);

                    break;
                case DbTableGroup.Address:
                    appRequestList = await PrepareInsertUpdateData_Address(orgUID, empUID, jobpositionUID);

                    break;
                default:
                    break;
            }
            if (appRequestList == null || appRequestList.Count == 0)
            {
                return;
            }
            //await UploadDataToServer(groupName, appRequestList);
            await UploadDataToServer_latest(groupName, appRequestList);
        }



        private async Task UploadDataToServer(string groupName, List<IAppRequest>? appRequestList)
        {
            try
            {
                if (appRequestList == null || appRequestList.Count == 0)
                {
                    return;
                }
                // Insert data to database
                bool IsInserted = await _appRequestBL.PostAppRequest(appRequestList);

                // Call service to post data
                ApiResponse<bool> apiResponse = await _apiService.FetchDataAsync<bool>(
                    $"{_appConfigs.ApiBaseUrl}AppRequest/PostAppRequest",
                    HttpMethod.Post, appRequestList);

                if (apiResponse == null)
                {
                    throw new Exception("No response from API");
                }
                if (apiResponse.IsSuccess && apiResponse.Data)
                {
                    // Update ss column for the data posted
                    foreach (AppRequest appRequest in appRequestList)
                    {
                        Dictionary<string, List<string>>? requestUIDDictionary = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(appRequest.RequestUIDs);
                        if (requestUIDDictionary == null || requestUIDDictionary.Count == 0) { continue; }
                        await _mobileDataSyncDL.UpdateSSForUIDs(requestUIDDictionary, SSValues.Three);
                    }

                    // update RequestPostedToAPITime for appRequestList
                    List<string> appRequestUIDs = appRequestList.Select(e => e.UID).ToList();
                    int updateCount = await _appRequestBL.UpdateAppRequest_RequestPostedToAPITime(appRequestUIDs, apiResponse.CurrentServerTime ?? DateTime.Now);

                    // Update LastDownloadTimeTime For Group
                    await _mobileDataSyncDL.UpdateLastUploadTimeForTableGroup(groupName, apiResponse.CurrentServerTime ?? DateTime.Now);
                }
                else
                {
                    // Handle unsuccessful response
                    throw new Exception($"Failed to upload data to server. StatusCode: {apiResponse.StatusCode}, Error Message:{apiResponse.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #region checking rabbitMqueue
        private async Task UploadDataToServer_latest(string groupName, List<IAppRequest>? appRequestList)
        {
            try
            {
                if (appRequestList == null || appRequestList.Count == 0)
                {
                    return;
                }
                // Insert data to database
                //bool IsInserted = await _appRequestBL.PostAppRequest(appRequestList);

                // Call service to post data
                var apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.RabbitMqueueBaseUrl}Sync/PublishSyncMessages",
                    HttpMethod.Post, appRequestList);

                if (apiResponse == null)
                {
                    throw new Exception("No response from API");
                }
                if (apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    // Update ss column for the data posted
                    foreach (AppRequest appRequest in appRequestList)
                    {
                        Dictionary<string, List<string>>? requestUIDDictionary = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(appRequest.RequestUIDs);
                        if (requestUIDDictionary == null || requestUIDDictionary.Count == 0) { continue; }
                        await _mobileDataSyncDL.UpdateSSForUIDs(requestUIDDictionary, SSValues.Zero);
                    }

                    // update RequestPostedToAPITime for appRequestList
                    List<string> appRequestUIDs = appRequestList.Select(e => e.UID).ToList();
                    int updateCount = await _appRequestBL.UpdateAppRequest_RequestPostedToAPITime(appRequestUIDs, apiResponse.CurrentServerTime ?? DateTime.Now);

                    // Update LastDownloadTimeTime For Group
                    await _mobileDataSyncDL.UpdateLastUploadTimeForTableGroup(groupName, apiResponse.CurrentServerTime ?? DateTime.Now);
                }
                else
                {
                    // Handle unsuccessful response
                    throw new Exception($"Failed to upload data to server. StatusCode: {apiResponse.StatusCode}, Error Message:{apiResponse.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion
        #endregion

        #region Supporting methods
        private async Task<List<object>> GetDataFromDatabase(string query, Dictionary<string, object> parameters, string modelName)
        {
            // Get the Type object for the class name
            Type? type = Type.GetType(modelName);

            // Check if the type is null
            if (type == null)
            {
                // Handle the case where the type is not found
                // You can throw an exception or return null
                return null;
            }

            // Make the method generic using reflection
            MethodInfo? method = typeof(IMobileDataSyncDL)
                .GetMethod(nameof(IMobileDataSyncDL.GetDataFromDatabase))?
                .MakeGenericMethod(type);

            // Check if the method is null
            if (method == null)
            {
                // Handle the case where the method is not found
                // You can throw an exception or return null
                return null;
            }

            // Invoke the generic method asynchronously
            var resultTask = (Task)method.Invoke(_mobileDataSyncDL, new object[] { query, parameters });

            // Await the result
            await resultTask;

            // Retrieve the result property
            var resultProperty = resultTask.GetType().GetProperty("Result");

            // Cast the result to List<T>
            var resultList = (IEnumerable)resultProperty.GetValue(resultTask);

            // Convert List<T> to List<object>
            var objectList = resultList.Cast<object>().ToList();

            // Return the list of objects
            return objectList;
        }
        private async Task<List<object>> GetDataFromDatabaseOld(string query, SqlParameter[] parameters, string modelName)
        {
            // Replace parameters in the query
            string finalQuery = ReplaceParametersInQuery(query, parameters);
            // Perform database query to get data from the specified table
            // Return list of objects representing the data
            // Get the Type object for the class name
            Type type = Type.GetType(modelName);

            // Check if the type is null
            if (type == null)
            {
                // Handle the case where the type is not found
                // You can throw an exception or return null
                return null;
            }

            // Make the method generic using reflection
            MethodInfo? method = typeof(IMobileDataSyncDL)
                .GetMethod(nameof(IMobileDataSyncDL.GetDataFromDatabase))?
                .MakeGenericMethod(type);

            // Check if the method is null
            if (method == null)
            {
                // Handle the case where the method is not found
                // You can throw an exception or return null
                return null;
            }

            //// Invoke the generic method asynchronously
            //Task<List<object>> task = (Task<List<object>>)method.Invoke(_mobileDataSyncDL, new object[] { finalQuery });

            //// Await the task and return the result
            //return await task;

            // Invoke the generic method asynchronously
            var resultTask = (Task)method.Invoke(_mobileDataSyncDL, new object[] { finalQuery });

            // Await the result
            await resultTask;

            // Retrieve the result property
            var resultProperty = resultTask.GetType().GetProperty("Result");

            // Cast the result to List<T>
            var resultList = (IEnumerable)resultProperty.GetValue(resultTask);

            // Convert List<T> to List<object>
            var objectList = resultList.Cast<object>().ToList();

            // Return the list of objects
            return objectList;
        }
        private string CreateSQLIteBaseFile(string identifier)
        {
            DateTime now = DateTime.Now;
            string baseSqlitePath = "Data\\Sqlite\\Base\\WINITSQLite.db";
            string userSqliteFolder = Path.Combine("Data", "Sqlite\\User", now.Year.ToString(), now.Month.ToString(), now.Day.ToString(), identifier);
            string userSqliteFileName = $"{identifier}_{now:yyyyMMddHHmmssfff}.db";
            string userSqliteFilePath = Path.Combine(userSqliteFolder, userSqliteFileName);
            try
            {
                // Copy SQLite database file

                // Ensure that the user folder exists
                Directory.CreateDirectory(userSqliteFolder);

                // Copy the base SQLite database file to the user-specific folder with the desired filename
                File.Copy(baseSqlitePath, userSqliteFilePath, true);
            }
            catch (Exception ex)
            {
                throw;
            }
            return userSqliteFilePath;
        }

        private bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }
            return false;
        }

        private async Task WriteDataToSQLite(string insertQuery, List<object> data, string userSqliteFilePath, int batchSize = 100)
        {
            try
            {
                // Write data to SQLite database in batches

                // Loop through the data list in batches
                for (int i = 0; i < data.Count; i += batchSize)
                {
                    // Get a chunk of data for this batch
                    var batchData = data.Skip(i).Take(batchSize).ToList();

                    // Write Logic to execute the batch insert query in SQLite
                    await ExecuteSQLiteQuery(insertQuery, batchData, userSqliteFilePath);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task WriteDataToSQLiteOld(string insertQuery, List<object> data, List<string> propertyNames, string userSqliteFilePath, int batchSize = 100)
        {
            try
            {
                // Write data to SQLite database in batches

                // Loop through the data list in batches
                for (int i = 0; i < data.Count; i += batchSize)
                {
                    // Get a chunk of data for this batch
                    var batchData = data.Skip(i).Take(batchSize).ToList();

                    // Build the parameterized insert query
                    List<string> valuePlaceholders = new List<string>();
                    foreach (var item in batchData)
                    {
                        // Construct a single row of values for insertion
                        string rowValues = GetValuePlaceholdersFromObject(item, propertyNames);
                        valuePlaceholders.Add(rowValues);
                    }

                    // Join all rows of values into a single string
                    string valuesString = string.Join(",", valuePlaceholders);

                    // Concatenate the insert query with the values string
                    string batchInsertQuery = insertQuery + " VALUES " + valuesString;

                    // Write Logic to execute the batch insert query in SQLite
                    await ExecuteSQLiteQuery(batchInsertQuery, null, userSqliteFilePath);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task ExecuteSQLiteQuery(string query, List<object>? data, string userSqliteFilePath)
        {
            try
            {
                await _mobileDataSyncDL.ExecuteQuery(query, data, userSqliteFilePath);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private string ReplaceParametersInQuery(string query, SqlParameter[] parameters)
        {
            foreach (var parameter in parameters)
            {
                string parameterValueString = GetParameterValueString(parameter);

                // Replace parameter placeholders in the query with parameter values
                query = query.Replace(parameter.ParameterName, parameterValueString);
            }

            return query;
        }

        private string GetParameterValueString(SqlParameter parameter)
        {
            if (parameter.Value == null || parameter.Value == DBNull.Value)
            {
                return "NULL";
            }
            else if (parameter.Value is string)
            {
                // Surround string parameter with single quotes and escape single quotes within the string
                return $"'{((string)parameter.Value).Replace("'", "''")}'";
            }
            else if (parameter.Value is DateTime)
            {
                // Format DateTime parameter properly and surround it with single quotes
                return $"'{((DateTime)parameter.Value).ToString("yyyy-MM-dd HH:mm:ss")}'";
            }
            else if (parameter.Value is int || parameter.Value is long || parameter.Value is decimal || parameter.Value is double || parameter.Value is float)
            {
                // Numeric types don't need surrounding single quotes
                return parameter.Value.ToString();
            }
            else
            {
                // For other types, just use their string representation
                return parameter.Value.ToString();
            }
        }

        private string GetValuePlaceholdersFromObject(object data, List<string> propertyNames)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            Type type = data.GetType();

            //// Filter properties based on the provided list of property names
            //PropertyInfo[] properties = type.GetProperties()
            //                                 .Where(prop => propertyNames.Contains(prop.Name))
            //                                 .ToArray();

            // Get properties based on the provided list of property names and preserve the order
            PropertyInfo[] properties = propertyNames
                .Select(name => type.GetProperty(name))
                .Where(prop => prop != null) // Filter out properties that are not found
                .ToArray();

            // Initialize a list to store the string representations of values
            List<string> values = new List<string>();

            // Collect the values of properties
            foreach (var property in properties)
            {
                // Get the value of the property
                object value = property.GetValue(data);

                // Convert the value to its string representation and add to the list
                values.Add(GetValueString(value));
            }

            // Join the values with commas
            string valuesString = string.Join(", ", values);

            // Wrap the values string in parentheses
            return "(" + valuesString + ")";
        }


        // Helper method to get string representation of a value
        private string GetValueString(object value)
        {
            if (value == null)
            {
                return "NULL";
            }
            else if (value is string)
            {
                // If the value is a string, surround it with single quotes
                return $"'{((string)value).Replace("'", "''")}'";
            }
            else if (value is bool)
            {
                // If the value is a boolean, convert it to 0 or 1
                return (bool)value ? "1" : "0";
            }
            else if (value is DateTime)
            {
                // If the value is a DateTime, format it properly and surround it with single quotes
                return $"'{((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss")}'";
            }
            else if (value is int || value is double || value is float || value is decimal)
            {
                // If the value is a numeric type, return it as is
                return value.ToString();
            }
            else if (value is byte[])
            {
                // If the value is a byte array, return it as a blob literal
                return $"X'{BitConverter.ToString((byte[])value).Replace("-", "")}'";
            }
            else
            {
                // For other types, just use their string representation
                return value.ToString();
            }
        }
        private string UnescapeSingleQuotes(string value)
        {
            // Replace double single quotes with a single single quote
            return value.Replace("''", "'");
        }
        #endregion
        #region Prepare Insert/Update Data for Upload
        private async Task<List<IAppRequest>?> PrepareInsertUpdateData_SalesOrder(string orgUID, string empUID, string jobpositionUID)
        {
            List<SalesOrderViewModelDCO>? salesOrderViewModelDCOList = await _mobileDataSyncDL.PrepareInsertUpdateData_SalesOrder();

            if (salesOrderViewModelDCOList == null || salesOrderViewModelDCOList.Count == 0)
            {
                return null;
            }
            List<IAppRequest> appRequestList = new List<IAppRequest>();
            foreach (SalesOrderViewModelDCO salesOrderViewModelDCO in salesOrderViewModelDCOList)
            {

                IAppRequest appRequest = _serviceProvider.CreateInstance<IAppRequest>();
                appRequest.Id = 0;
                appRequest.UID = Guid.NewGuid().ToString();
                appRequest.OrgUID = orgUID;
                appRequest.LinkedItemType = DbTableGroup.Sales;
                appRequest.YearMonth = CommonFunctions.GetIntValue(CommonFunctions.GetDateTimeInFormat(DateTime.Now, "yyMM"));
                appRequest.LinkedItemUID = salesOrderViewModelDCO.SalesOrder.UID;
                appRequest.EmpUID = empUID;
                appRequest.JobPositionUID = jobpositionUID;
                appRequest.RequestCreatedTime = DateTime.Now;
                appRequest.RequestPostedToAPITime = null;
                appRequest.RequestReceivedByAPITime = null;
                appRequest.NextUID = null;
                appRequest.RequestBody = JsonConvert.SerializeObject(salesOrderViewModelDCO);
                appRequest.RequestUIDs = JsonConvert.SerializeObject(salesOrderViewModelDCO.RequestUIDDictionary);
                appRequestList.Add(appRequest);
            }
            if (appRequestList.Count == 0)
            {
                return null;
            }
            return appRequestList;
        }
        
        private async Task<List<IAppRequest>?> PrepareInsertUpdateData_Merchandiser(string orgUID, string empUID, string jobpositionUID)
        {
            List<MerchandiserDTO>? merchandiserDCOList = await _mobileDataSyncDL.PrepareInsertUpdateData_Merchandiser();

            if (merchandiserDCOList == null || merchandiserDCOList.Count == 0)
            {
                return null;
            }
            List<IAppRequest> appRequestList = new List<IAppRequest>();
            foreach (MerchandiserDTO merchandiserDCO in merchandiserDCOList)
            {

                IAppRequest appRequest = _serviceProvider.CreateInstance<IAppRequest>();
                appRequest.Id = 0;
                appRequest.UID = Guid.NewGuid().ToString();
                appRequest.OrgUID = orgUID;
                appRequest.LinkedItemType = DbTableGroup.Merchandiser;
                appRequest.YearMonth = CommonFunctions.GetIntValue(CommonFunctions.GetDateTimeInFormat(DateTime.Now, "yyMM"));
                appRequest.LinkedItemUID = DbTableGroup.Merchandiser;
                appRequest.EmpUID = empUID;
                appRequest.JobPositionUID = jobpositionUID;
                appRequest.RequestCreatedTime = DateTime.Now;
                appRequest.RequestPostedToAPITime = null;
                appRequest.RequestReceivedByAPITime = null;
                appRequest.NextUID = null;
                appRequest.RequestBody = JsonConvert.SerializeObject(merchandiserDCO);
                appRequest.RequestUIDs = JsonConvert.SerializeObject(merchandiserDCO.RequestUIDDictionary);
                appRequestList.Add(appRequest);
            }
            if (appRequestList.Count == 0)
            {
                return null;
            }
            return appRequestList;
        }
        private async Task<List<IAppRequest>?> PrepareInsertUpdateData_WHStockRequest(string orgUID, string empUID, string jobpositionUID)
        {
            List<WHRequestTempleteModel>? wHRequestTempleteModelList = await _mobileDataSyncDL.PrepareInsertUpdateData_WHStockRequest();
            if (wHRequestTempleteModelList == null || wHRequestTempleteModelList.Count == 0)
            {
                return null;
            }
            List<IAppRequest> appRequestList = new List<IAppRequest>();
            foreach (WHRequestTempleteModel wHRequestTempleteModel in wHRequestTempleteModelList)
            {
                IAppRequest appRequest = _serviceProvider.CreateInstance<IAppRequest>();
                appRequest.Id = 0;
                appRequest.UID = Guid.NewGuid().ToString();
                appRequest.OrgUID = orgUID;
                appRequest.LinkedItemType = DbTableGroup.StockRequest;
                appRequest.YearMonth = CommonFunctions.GetIntValue(CommonFunctions.GetDateTimeInFormat(DateTime.Now, "yyMM"));
                appRequest.LinkedItemUID = wHRequestTempleteModel.WHStockRequest.UID;
                appRequest.EmpUID = empUID;
                appRequest.JobPositionUID = jobpositionUID;
                appRequest.RequestCreatedTime = DateTime.Now;
                appRequest.RequestPostedToAPITime = null;
                appRequest.RequestReceivedByAPITime = null;
                appRequest.NextUID = null;
                appRequest.RequestBody = JsonConvert.SerializeObject(wHRequestTempleteModel);
                appRequest.RequestUIDs = JsonConvert.SerializeObject(wHRequestTempleteModel.RequestUIDDictionary);
                appRequestList.Add(appRequest);
            }
            if (appRequestList.Count == 0)
            {
                return null;
            }
            return appRequestList;
        }

        private async Task<List<IAppRequest>?> PrepareInsertUpdateData_StoreCheck(string orgUID, string empUID, string jobpositionUID)
        {
            List<StoreCheckMaster>? storeCheckMasterList = await _mobileDataSyncDL.PrepareInsertUpdateData_StoreCheck();
            if (storeCheckMasterList == null || storeCheckMasterList.Count == 0)
            {
                return null;
            }
            List<IAppRequest> appRequestList = new List<IAppRequest>();
            foreach (StoreCheckMaster storeCheckMaster in storeCheckMasterList)
            {
                IAppRequest appRequest = _serviceProvider.CreateInstance<IAppRequest>();
                appRequest.Id = 0;
                appRequest.UID = Guid.NewGuid().ToString();
                appRequest.OrgUID = orgUID;
                appRequest.LinkedItemType = DbTableGroup.StoreCheck;
                appRequest.YearMonth = CommonFunctions.GetIntValue(CommonFunctions.GetDateTimeInFormat(DateTime.Now, "yyMM"));
                appRequest.LinkedItemUID = storeCheckMaster.StoreCheckHistory.UID;
                appRequest.EmpUID = empUID;
                appRequest.JobPositionUID = jobpositionUID;
                appRequest.RequestCreatedTime = DateTime.Now;
                appRequest.RequestPostedToAPITime = null;
                appRequest.RequestReceivedByAPITime = null;
                appRequest.NextUID = null;
                appRequest.RequestBody = JsonConvert.SerializeObject(storeCheckMaster);
                appRequest.RequestUIDs = JsonConvert.SerializeObject(storeCheckMaster.RequestUIDDictionary);
                appRequestList.Add(appRequest);
            }
            if (appRequestList.Count == 0)
            {
                return null;
            }
            return appRequestList;
        }
        private async Task<List<IAppRequest>?> PrepareInsertUpdateData_Return(string orgUID, string empUID, string jobpositionUID)
        {
            List<ReturnOrderMasterDTO>? returnOrderMasterDTOList = await _mobileDataSyncDL.PrepareInsertUpdateData_Return();
            if (returnOrderMasterDTOList == null || returnOrderMasterDTOList.Count == 0)
            {
                return null;
            }
            List<IAppRequest> appRequestList = new List<IAppRequest>();
            foreach (ReturnOrderMasterDTO returnOrderMasterDTO in returnOrderMasterDTOList)
            {
                IAppRequest appRequest = _serviceProvider.CreateInstance<IAppRequest>();
                appRequest.Id = 0;
                appRequest.UID = Guid.NewGuid().ToString();
                appRequest.OrgUID = orgUID;
                appRequest.LinkedItemType = DbTableGroup.Return;
                appRequest.YearMonth = CommonFunctions.GetIntValue(CommonFunctions.GetDateTimeInFormat(DateTime.Now, "yyMM"));
                appRequest.LinkedItemUID = returnOrderMasterDTO?.ReturnOrder?.UID;
                appRequest.EmpUID = empUID;
                appRequest.JobPositionUID = jobpositionUID;
                appRequest.RequestCreatedTime = DateTime.Now;
                appRequest.RequestPostedToAPITime = null;
                appRequest.RequestReceivedByAPITime = null;
                appRequest.NextUID = null;
                appRequest.RequestBody = JsonConvert.SerializeObject(returnOrderMasterDTO);
                appRequest.RequestUIDs = JsonConvert.SerializeObject(returnOrderMasterDTO.RequestUIDDictionary);
                appRequestList.Add(appRequest);
            }
            if (appRequestList.Count == 0)
            {
                return null;
            }
            return appRequestList;
        }
        private async Task<List<IAppRequest>?> PrepareInsertUpdateData_Collection(string orgUID, string empUID, string jobpositionUID)
        {
            List<CollectionDTO>? collectionDTOList = await _mobileDataSyncDL.PrepareInsertUpdateData_Collection();
            if (collectionDTOList == null || collectionDTOList.Count == 0)
            {
                return null;
            }
            List<IAppRequest> appRequestList = new List<IAppRequest>();
            foreach (CollectionDTO collectionDTO in collectionDTOList)
            {
                IAppRequest appRequest = _serviceProvider.CreateInstance<IAppRequest>();
                appRequest.Id = 0;
                appRequest.UID = Guid.NewGuid().ToString();
                appRequest.OrgUID = orgUID;
                appRequest.LinkedItemType = DbTableGroup.Collection;
                appRequest.YearMonth = CommonFunctions.GetIntValue(CommonFunctions.GetDateTimeInFormat(DateTime.Now, "yyMM"));
                appRequest.LinkedItemUID = collectionDTO?.AccCollection?.UID;
                appRequest.EmpUID = empUID;
                appRequest.JobPositionUID = jobpositionUID;
                appRequest.RequestCreatedTime = DateTime.Now;
                appRequest.RequestPostedToAPITime = null;
                appRequest.RequestReceivedByAPITime = null;
                appRequest.NextUID = null;
                appRequest.RequestBody = JsonConvert.SerializeObject(collectionDTO);
                appRequest.RequestUIDs = JsonConvert.SerializeObject(collectionDTO.RequestUIDDictionary);
                appRequestList.Add(appRequest);
            }
            if (appRequestList.Count == 0)
            {
                return null;
            }
            return appRequestList;
        }
        private async Task<List<IAppRequest>?> PrepareInsertUpdateData_CollectionDeposit(string orgUID, string empUID, string jobpositionUID)
        {
            List<Winit.Modules.CollectionModule.Model.Classes.AccCollectionDeposit>? accCollectionDepositList = await _mobileDataSyncDL.PrepareInsertUpdateData_CollectionDeposit();
            if (accCollectionDepositList == null || accCollectionDepositList.Count == 0)
            {
                return null;
            }
            List<IAppRequest> appRequestList = new List<IAppRequest>();
            foreach (AccCollectionDeposit accCollectionDeposit in accCollectionDepositList)
            {
                IAppRequest appRequest = _serviceProvider.CreateInstance<IAppRequest>();
                appRequest.Id = 0;
                appRequest.UID = Guid.NewGuid().ToString();
                appRequest.OrgUID = orgUID;
                appRequest.LinkedItemType = DbTableGroup.CollectionDeposit;
                appRequest.YearMonth = CommonFunctions.GetIntValue(CommonFunctions.GetDateTimeInFormat(DateTime.Now, "yyMM"));
                appRequest.LinkedItemUID = accCollectionDeposit?.UID;
                appRequest.EmpUID = empUID;
                appRequest.JobPositionUID = jobpositionUID;
                appRequest.RequestCreatedTime = DateTime.Now;
                appRequest.RequestPostedToAPITime = null;
                appRequest.RequestReceivedByAPITime = null;
                appRequest.NextUID = null;
                appRequest.RequestBody = JsonConvert.SerializeObject(accCollectionDeposit);
                appRequest.RequestUIDs = JsonConvert.SerializeObject(accCollectionDeposit.RequestUIDDictionary);
                appRequestList.Add(appRequest);
            }
            if (appRequestList.Count == 0)
            {
                return null;
            }
            return appRequestList;
        }

        private async Task<List<IAppRequest>?> PrepareInsertUpdateData_Master(string orgUID, string empUID, string jobpositionUID)
        {
            List<Winit.Modules.JourneyPlan.Model.Classes.MasterDTO>? masterDTOList = await _mobileDataSyncDL.PrepareInsertUpdateData_Master();
            if (masterDTOList == null || masterDTOList.Count == 0)
            {
                return null;
            }
            List<IAppRequest> appRequestList = new List<IAppRequest>();
            foreach (MasterDTO masterDTO in masterDTOList)
            {
                IAppRequest appRequest = _serviceProvider.CreateInstance<IAppRequest>();
                appRequest.Id = 0;
                appRequest.UID = Guid.NewGuid().ToString();
                appRequest.OrgUID = orgUID;
                appRequest.LinkedItemType = DbTableGroup.Master;
                appRequest.YearMonth = CommonFunctions.GetIntValue(CommonFunctions.GetDateTimeInFormat(DateTime.Now, "yyMM"));
                appRequest.LinkedItemUID = empUID;
                appRequest.EmpUID = empUID;
                appRequest.JobPositionUID = jobpositionUID;
                appRequest.RequestCreatedTime = DateTime.Now;
                appRequest.RequestPostedToAPITime = null;
                appRequest.RequestReceivedByAPITime = null;
                appRequest.NextUID = null;
                appRequest.RequestBody = JsonConvert.SerializeObject(masterDTO);
                appRequest.RequestUIDs = JsonConvert.SerializeObject(masterDTO.RequestUIDDictionary);
                appRequestList.Add(appRequest);
            }
            if (appRequestList.Count == 0)
            {
                return null;
            }
            return appRequestList;
        }
        
        //private async Task<List<IAppRequest>?> PrepareInsertUpdateData_Merchandiser(string orgUID, string empUID, string jobpositionUID)
        //{
        //    List<Winit.Modules.JourneyPlan.Model.Classes.MasterDTO>? masterDTOList = await _mobileDataSyncDL.PrepareInsertUpdateData_Merchandiser();
        //    if (masterDTOList == null || masterDTOList.Count == 0)
        //    {
        //        return null;
        //    }
        //    List<IAppRequest> appRequestList = new List<IAppRequest>();
        //    foreach (MasterDTO masterDTO in masterDTOList)
        //    {
        //        IAppRequest appRequest = _serviceProvider.CreateInstance<IAppRequest>();
        //        appRequest.Id = 0;
        //        appRequest.UID = Guid.NewGuid().ToString();
        //        appRequest.OrgUID = orgUID;
        //        appRequest.LinkedItemType = DbTableGroup.Master;
        //        appRequest.YearMonth = CommonFunctions.GetIntValue(CommonFunctions.GetDateTimeInFormat(DateTime.Now, "yyMM"));
        //        appRequest.LinkedItemUID = empUID;
        //        appRequest.EmpUID = empUID;
        //        appRequest.JobPositionUID = jobpositionUID;
        //        appRequest.RequestCreatedTime = DateTime.Now;
        //        appRequest.RequestPostedToAPITime = null;
        //        appRequest.RequestReceivedByAPITime = null;
        //        appRequest.NextUID = null;
        //        appRequest.RequestBody = JsonConvert.SerializeObject(masterDTO);
        //        appRequest.RequestUIDs = JsonConvert.SerializeObject(masterDTO.RequestUIDDictionary);
        //        appRequestList.Add(appRequest);
        //    }
        //    if (appRequestList.Count == 0)
        //    {
        //        return null;
        //    }
        //    return appRequestList;
        //}


        //private async Task<List<IAppRequest>?> PrepareInsertUpdateData_SurveyResponse(string orgUID, string empUID, string jobpositionUID)
        //{
        //    List<Winit.Modules.Survey.Model.Classes.SurveyResponseModel>? surveyResponseList = await _mobileDataSyncDL.PrepareInsertUpdateData_SurveyResponse();
        //    if (surveyResponseList == null || surveyResponseList.Count == 0)
        //    {
        //        return null;
        //    }
        //    List<IAppRequest> appRequestList = new List<IAppRequest>();
        //    foreach (Winit.Modules.Survey.Model.Classes.SurveyResponseModel surveyResponse in surveyResponseList)
        //    {
        //        IAppRequest appRequest = _serviceProvider.CreateInstance<IAppRequest>();
        //        appRequest.Id = 0;
        //        appRequest.UID = Guid.NewGuid().ToString();
        //        appRequest.OrgUID = orgUID;
        //        appRequest.LinkedItemType = DbTableGroup.SurveyResponse;
        //        appRequest.YearMonth = CommonFunctions.GetIntValue(CommonFunctions.GetDateTimeInFormat(DateTime.Now, "yyMM"));
        //        appRequest.LinkedItemUID = empUID;
        //        appRequest.EmpUID = empUID;
        //        appRequest.JobPositionUID = jobpositionUID;
        //        appRequest.RequestCreatedTime = DateTime.Now;
        //        appRequest.RequestPostedToAPITime = null;
        //        appRequest.RequestReceivedByAPITime = null;
        //        appRequest.NextUID = null;
        //        appRequest.RequestBody = JsonConvert.SerializeObject(surveyResponse);
        //        appRequest.RequestUIDs = JsonConvert.SerializeObject(surveyResponse.RequestUIDDictionary);
        //        appRequestList.Add(appRequest);
        //    }
        //    if (appRequestList.Count == 0)
        //    {
        //        return null;
        //    }
        //    return appRequestList;
        //}

        private async Task<List<IAppRequest>?> PrepareInsertUpdateData_FileSys(string orgUID, string empUID, string jobpositionUID)
        {
            List<Winit.Modules.FileSys.Model.Classes.FileSys>? fileSysList = await _mobileDataSyncDL.PrepareInsertUpdateData_FileSys();
            if (fileSysList == null || fileSysList.Count == 0)
            {
                return null;
            }
            List<IAppRequest> appRequestList = new List<IAppRequest>();
            foreach (Winit.Modules.FileSys.Model.Classes.FileSys fileSys in fileSysList)
            {
                IAppRequest appRequest = _serviceProvider.CreateInstance<IAppRequest>();
                appRequest.Id = 0;
                appRequest.UID = Guid.NewGuid().ToString();
                appRequest.OrgUID = orgUID;
                appRequest.LinkedItemType = DbTableGroup.FileSys;
                appRequest.YearMonth = CommonFunctions.GetIntValue(CommonFunctions.GetDateTimeInFormat(DateTime.Now, "yyMM"));
                appRequest.LinkedItemUID = empUID;
                appRequest.EmpUID = empUID;
                appRequest.JobPositionUID = jobpositionUID;
                appRequest.RequestCreatedTime = DateTime.Now;
                appRequest.RequestPostedToAPITime = null;
                appRequest.RequestReceivedByAPITime = null;
                appRequest.NextUID = null;
                appRequest.RequestBody = JsonConvert.SerializeObject(fileSys);
                appRequest.RequestUIDs = JsonConvert.SerializeObject(fileSys.RequestUIDDictionary);
                appRequestList.Add(appRequest);
            }
            if (appRequestList.Count == 0)
            {
                return null;
            }
            return appRequestList;
        }

        private async Task<List<IAppRequest>?> PrepareInsertUpdateData_Address(string orgUID, string empUID, string jobpositionUID)
        {
            List<Winit.Modules.Address.Model.Classes.Address>? addressList = await _mobileDataSyncDL.PrepareInsertUpdateData_Address();
            if (addressList == null || addressList.Count == 0)
            {
                return null;
            }

            List<IAppRequest> appRequestList = new List<IAppRequest>();

            foreach (Winit.Modules.Address.Model.Classes.Address address in addressList)
            {
                IAppRequest appRequest = _serviceProvider.CreateInstance<IAppRequest>();
                appRequest.Id = 0;
                appRequest.UID = Guid.NewGuid().ToString();
                appRequest.OrgUID = orgUID;
                appRequest.LinkedItemType = DbTableGroup.Address;
                appRequest.YearMonth = CommonFunctions.GetIntValue(CommonFunctions.GetDateTimeInFormat(DateTime.Now, "yyMM"));
                appRequest.LinkedItemUID = empUID;
                appRequest.EmpUID = empUID;
                appRequest.JobPositionUID = jobpositionUID;
                appRequest.RequestCreatedTime = DateTime.Now;
                appRequest.RequestPostedToAPITime = null;
                appRequest.RequestReceivedByAPITime = null;
                appRequest.NextUID = null;
                appRequest.RequestBody = JsonConvert.SerializeObject(address);
                appRequest.RequestUIDs = JsonConvert.SerializeObject(address.RequestUIDDictionary);

                appRequestList.Add(appRequest);
            }

            if (appRequestList.Count == 0)
            {
                return null;
            }

            return appRequestList;
        }

        #endregion
        
        #region Parallel Processing Methods

        /// <summary>
        /// Process tables in parallel with configurable concurrency
        /// </summary>
        private async Task ProcessTablesInParallel(List<ITableGroupEntityView> tableGroupEntityList, 
            Dictionary<string, object> parameters, string dynamicTableName, string userSqliteFilePath, SyncLog syncLog)
        {
            // Create thread-safe collection for sync details
            var syncDetails = new ConcurrentBag<TableSyncDetail>();
            
            // Create semaphore to limit concurrent table processing
            using var semaphore = new SemaphoreSlim(_syncSettings.MaxConcurrentTables, _syncSettings.MaxConcurrentTables);
            
            // Create tasks for parallel processing
            var tasks = tableGroupEntityList.Select(async tableGroupEntity =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var syncDetail = await ProcessSingleTable(tableGroupEntity, parameters, dynamicTableName, userSqliteFilePath);
                    syncDetails.Add(syncDetail);
                }
                finally
                {
                    semaphore.Release();
                }
            }).ToArray();

            // Wait for all tables to complete
            await Task.WhenAll(tasks);

            // Add all sync details to the main sync log (order doesn't matter for results)
            foreach (var detail in syncDetails)
            {
                syncLog.TableSyncDetails.Add(detail);
            }

            if (_syncSettings.EnableSyncLogging)
            {
                var successCount = syncDetails.Count(d => d.Status == "Success");
                var failedCount = syncDetails.Count(d => d.Status == "Failed");
                Console.WriteLine($"Parallel sync completed: {successCount} succeeded, {failedCount} failed");
            }
        }

        /// <summary>
        /// Process tables sequentially (original behavior)
        /// </summary>
        private async Task ProcessTablesSequentially(List<ITableGroupEntityView> tableGroupEntityList, 
            Dictionary<string, object> parameters, string dynamicTableName, string userSqliteFilePath, SyncLog syncLog)
        {
            foreach (var tableGroupEntity in tableGroupEntityList)
            {
                var syncDetail = await ProcessSingleTable(tableGroupEntity, parameters, dynamicTableName, userSqliteFilePath);
                syncLog.TableSyncDetails.Add(syncDetail);
            }
        }

        /// <summary>
        /// Process a single table (shared logic for both parallel and sequential processing)
        /// </summary>
        private async Task<TableSyncDetail> ProcessSingleTable(ITableGroupEntityView tableGroupEntity, 
            Dictionary<string, object> parameters, string dynamicTableName, string userSqliteFilePath)
        {
            var syncDetail = new TableSyncDetail
            {
                TableName = tableGroupEntity.TableName
            };
            if(tableGroupEntity.TableName == "survey")
            {
                Console.WriteLine("Survey");
            }
            if (string.IsNullOrEmpty(tableGroupEntity.MasterDataQuery) || string.IsNullOrEmpty(tableGroupEntity.ModelName))
            {
                syncDetail.Status = "Failed";
                syncDetail.Message = "No MasterDataQuery or ModelName exists";
                return syncDetail;
            }

            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Replace dynamic table name if needed
                if (tableGroupEntity.MasterDataQuery.Contains("{{DynamicTable}}"))
                {
                    tableGroupEntity.MasterDataQuery = tableGroupEntity.MasterDataQuery.Replace("{{DynamicTable}}", dynamicTableName);
                }

                // Read data from server database table
                List<object> data = await GetDataFromDatabase(tableGroupEntity.MasterDataQuery, parameters, tableGroupEntity.ModelName);

                // Write data to sqlite
                if (data != null && data.Count > 0)
                {
                    await WriteDataToSQLite(tableGroupEntity.SqliteInsertQuery, data, userSqliteFilePath, _syncSettings.BatchSize);
                    syncDetail.RecordCount = data.Count;
                }

                syncDetail.Status = "Success";

                if (_syncSettings.EnableSyncLogging)
                {
                    Console.WriteLine($"Table {tableGroupEntity.TableName}: {data?.Count ?? 0} records synced in {stopwatch.ElapsedMilliseconds}ms");
                }
            }
            catch (Exception ex)
            {
                syncDetail.Status = "Failed";
                syncDetail.Message = ex.Message + "\n" + ex.StackTrace;
                
                if (_syncSettings.EnableSyncLogging)
                {
                    Console.WriteLine($"Error syncing table {tableGroupEntity.TableName}: {ex.Message}");
                }
            }
            finally
            {
                stopwatch.Stop();
                syncDetail.TimeTaken = stopwatch.Elapsed;
            }

            return syncDetail;
        }

        #endregion
        
        #region Helper Methods

        /// <summary>
        /// Combines failed groups and failed tables into a comprehensive error message.
        /// </summary>
        /// <param name="failedGroups">List of failed group names</param>
        /// <param name="allFailedTables">List of failed tables (format: GroupName.TableName)</param>
        /// <returns>Combined error message or null if no failures</returns>
        private string GetCombinedErrorMessage(List<string> failedGroups, List<string> allFailedTables)
        {
            if ((failedGroups == null || failedGroups.Count == 0) && 
                (allFailedTables == null || allFailedTables.Count == 0))
            {
                return null;
            }

            List<string> errorParts = new List<string>();

            if (failedGroups != null && failedGroups.Count > 0)
            {
                errorParts.Add($"Failed groups: {string.Join(", ", failedGroups)}");
            }

            if (allFailedTables != null && allFailedTables.Count > 0)
            {
                errorParts.Add($"Failed tables: {string.Join(", ", allFailedTables)}");
            }

            return string.Join(" | ", errorParts);
        }
        
        #endregion
    }
}
