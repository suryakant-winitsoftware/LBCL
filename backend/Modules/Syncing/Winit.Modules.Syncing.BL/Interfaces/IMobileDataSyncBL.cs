using Microsoft.Data.SqlClient;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.Syncing.Model.Interfaces;

namespace Winit.Modules.Syncing.BL.Interfaces
{
    public interface IMobileDataSyncBL
    {
        Task<string> DownloadServerDataInSqlite(string groupName, string tableName, string employeeUID, string jobPositionUID, string roleUID, string vehicleUID, string orgUID, string empCode);
        Task<Dictionary<string, object>> DownloadServerDataInSync(string groupName, string tableName, string employeeUID, string jobPositionUID, string roleUID, string vehicleUID, string orgUID, string EmpCode, DateTime serverModifiedTime);
        
        /// <summary>
        /// Syncs data for a single table group.
        /// </summary>
        /// <param name="groupName">Group name to sync (empty string for all groups)</param>
        /// <param name="tableName">Table name to sync (empty string for all tables in group)</param>
        /// <param name="employeeUID">Employee UID</param>
        /// <param name="jobPositionUID">Job Position UID</param>
        /// <param name="roleUID">Role UID</param>
        /// <param name="vehicleUID">Vehicle UID</param>
        /// <param name="orgUID">Organization UID</param>
        Task SyncDataForTableGroup(string groupName, string tableName, string employeeUID, string jobPositionUID, string roleUID, string vehicleUID, string orgUID, string EmpCode);

        /// <summary>
        /// Syncs data for a single table group with progress reporting.
        /// </summary>
        /// <param name="groupName">Group name to sync (empty string for all groups)</param>
        /// <param name="tableName">Table name to sync (empty string for all tables in group)</param>
        /// <param name="employeeUID">Employee UID</param>
        /// <param name="jobPositionUID">Job Position UID</param>
        /// <param name="roleUID">Role UID</param>
        /// <param name="vehicleUID">Vehicle UID</param>
        /// <param name="orgUID">Organization UID</param>
        /// <param name="progress">Progress callback for reporting sync progress</param>
        Task SyncDataForTableGroup(string groupName, string tableName, string employeeUID, string jobPositionUID, string roleUID, string vehicleUID, string orgUID, string EmpCode, IProgress<Winit.Modules.Syncing.Model.Classes.SyncProgress> progress, bool showTableLevelErrors = false);
        
        /// <summary>
        /// Syncs data for multiple table groups efficiently.
        /// </summary>
        /// <param name="groupNames">List of group names to sync (empty list for all groups)</param>
        /// <param name="tableName">Table name to sync (empty string for all tables in groups)</param>
        /// <param name="employeeUID">Employee UID</param>
        /// <param name="jobPositionUID">Job Position UID</param>
        /// <param name="roleUID">Role UID</param>
        /// <param name="vehicleUID">Vehicle UID</param>
        /// <param name="orgUID">Organization UID</param>
        Task SyncDataForTableGroups(List<string> groupNames, string tableName, string employeeUID, string jobPositionUID, string roleUID, string vehicleUID, string orgUID, string EmpCode);

        /// <summary>
        /// Syncs data for multiple table groups efficiently with progress reporting.
        /// </summary>
        /// <param name="groupNames">List of group names to sync (empty list for all groups)</param>
        /// <param name="tableName">Table name to sync (empty string for all tables in groups)</param>
        /// <param name="employeeUID">Employee UID</param>
        /// <param name="jobPositionUID">Job Position UID</param>
        /// <param name="roleUID">Role UID</param>
        /// <param name="vehicleUID">Vehicle UID</param>
        /// <param name="orgUID">Organization UID</param>
        /// <param name="progress">Progress callback for reporting sync progress</param>
        Task SyncDataForTableGroups(List<string> groupNames, string tableName, string employeeUID, string jobPositionUID, string roleUID, string vehicleUID, string orgUID, string EmpCode, IProgress<Winit.Modules.Syncing.Model.Classes.SyncProgress> progress, bool showTableLevelErrors = false);
        
        Task SyncDataFromSQLiteToServer(string orgUID, string groupName,string empUID=null,string jobpositionUID=null);
    }
}
