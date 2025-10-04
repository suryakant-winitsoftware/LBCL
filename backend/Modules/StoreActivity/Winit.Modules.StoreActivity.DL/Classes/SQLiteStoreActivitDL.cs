using Microsoft.Extensions.DependencyInjection;
using Nest;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.StoreActivity.DL.Interfaces;
using Winit.Modules.StoreActivity.Model.Interfaces;

namespace Winit.Modules.StoreActivity.DL.Classes
{
    public class SQLiteStoreActivitDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, IStoreActivitDL
    {
        public SQLiteStoreActivitDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        //public async Task<IEnumerable<IStoreActivityItem>> GetAllStoreActivities(string RoleUID, string StoreHistoryUID)
        //{

        //    try
        //    {

        //        var sql = @"SELECT SA.uid , SA.code, SA.name, SA.icon_path as IconPath ,SA.nav_path as NavPath, SAH.serial_no as SerialNo, SAH.is_compulsory as IsCompulsory, SAH.is_locked as IsLocked, SAH.status  
        //             FROM store_activity_role_mapping SAR
        //         INNER JOIN store_activity SA ON SA.uid = SAR.store_activity_uid and SAR.role_uid = @RoleUID
        //         INNER JOIN store_activity_history SAH ON SAH.uid = @StoreHistoryUID || SAR.uid";

        //        Dictionary<string, object> parameters = new Dictionary<string, object>
        //        {
        //                {"RoleUID", RoleUID},{"StoreHistoryUID" ,StoreHistoryUID}
        //        };

        //        Type type = _serviceProvider.GetRequiredService<IStoreActivityItem>().GetType();
        //        IEnumerable<IStoreActivityItem> StoreActivities = await ExecuteQueryAsync<IStoreActivityItem>(sql, parameters, type);

        //        return StoreActivities;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }

        //}

        public async Task<IEnumerable<IStoreActivityItem>> GetAllStoreActivities(string RoleUID, string StoreHistoryUID)
        {
            try
            {
                var sqlCheck = @"SELECT COUNT(*) FROM store_activity_role_mapping SAR
                        INNER JOIN store_activity SA ON SA.uid = SAR.store_activity_uid AND SAR.role_uid = @RoleUID
                        INNER JOIN store_activity_history SAH ON SAH.uid = @StoreHistoryUID || SAR.uid";

                Dictionary<string, object> parametersCheck = new Dictionary<string, object>
        {
            {"RoleUID", RoleUID},
            {"StoreHistoryUID", StoreHistoryUID}
        };

                int count = await ExecuteScalarAsync<int>(sqlCheck, parametersCheck);

                if (count == 0)
                {
                    await InsertStoreActivityHistory(RoleUID, StoreHistoryUID);
                }

                var sqlGet = @"SELECT SA.uid, SA.code, SA.name, SA.icon_path AS IconPath, SA.nav_path AS NavPath, SAH.serial_no AS SerialNo, 
                      SAH.is_compulsory AS IsCompulsory, SAH.is_locked AS IsLocked, SAH.status,SAH.uid as store_activity_history_uid 
                      FROM store_activity_role_mapping SAR
                      INNER JOIN store_activity SA ON SA.uid = SAR.store_activity_uid AND SAR.role_uid = @RoleUID
                      INNER JOIN store_activity_history SAH ON SAH.uid = @StoreHistoryUID || SAR.uid and SA.Is_Active=1 ORDER BY SAH.serial_no ASC";

                Dictionary<string, object> parametersGet = new Dictionary<string, object>
        {
            {"RoleUID", RoleUID},
            {"StoreHistoryUID", StoreHistoryUID}
        };

                Type type = _serviceProvider.GetRequiredService<IStoreActivityItem>().GetType();
                IEnumerable<IStoreActivityItem> StoreActivities = await ExecuteQueryAsync<IStoreActivityItem>(sqlGet, parametersGet, type);

                return StoreActivities;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task InsertStoreActivityHistory(string RoleUID, string StoreHistoryUID)
        {
            try
            {
                var sqlInsert = @"INSERT INTO store_activity_history (id, uid, store_history_uid, store_activity_uid, serial_no,
                          is_compulsory, is_locked, status, ss, created_time, modified_time,
                          server_add_time, server_modified_time)
                          SELECT 0 id, @StoreHistoryUID || SAR.uid AS uid, @StoreHistoryUID store_history_uid, SAR.store_activity_uid, SA.serial_no,
                          SA.is_compulsory, SA.is_locked, 'Pending' status, 0 ss, '2024-04-19' created_time, '2024-04-19' modified_time,
                          '2024-04-19' server_add_time, '2024-04-19' server_modified_time
                          FROM store_activity_role_mapping SAR
                          INNER JOIN store_activity SA ON SA.uid = SAR.store_activity_uid AND SA.is_active = 1 AND SAR.role_uid = @RoleUID
                          LEFT JOIN store_activity_history SAH ON SAH.uid = @StoreHistoryUID || SAR.uid
                          WHERE SAH.ID IS NULL";

                Dictionary<string, object> parametersInsert = new Dictionary<string, object>
        {
            {"RoleUID", RoleUID},
            {"StoreHistoryUID", StoreHistoryUID}
        };

                await ExecuteNonQueryAsync(sqlInsert, parametersInsert);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> UpdateStoreActivityHistoryStatus(string storeActivityHistoryUID, string status)
        {
            try
            {
                var currentTime = DateTime.Now;

                string updateQuery = @"UPDATE store_activity_history SET Status = @Status, modified_time = @CurrentTime WHERE UID = @StoreActivityHistoryUID";

                Dictionary<string, object> parameters = new()
        {
            { "@Status", status },
            { "@CurrentTime", currentTime },
            { "@StoreActivityHistoryUID", storeActivityHistoryUID }
        };

                return await ExecuteNonQueryAsync(updateQuery, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating store activity history status.", ex);
            }
        }


    }
}
