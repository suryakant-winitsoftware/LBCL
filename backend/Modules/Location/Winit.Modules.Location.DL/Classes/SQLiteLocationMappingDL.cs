using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Location.DL.Interfaces;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Location.DL.Classes
{
    public class SQLiteLocationMappingDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, ILocationMappingDL
    {
        public SQLiteLocationMappingDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }
        public async Task<PagedResponse<Winit.Modules.Location.Model.Interfaces.ILocationMapping>> SelectAllLocationMappingDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (SELECT 
                    id AS Id,
                    uid AS Uid,
                    created_by AS CreatedBy,
                    created_time AS CreatedTime,
                    modified_by AS ModifiedBy,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime,
                    linked_item_uid AS LinkedItemUid,
                    linked_item_type AS LinkedItemType,
                    location_type_uid AS LocationTypeUid,
                    location_uid AS LocationUid
                FROM 
                    location_mapping) AS SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        linked_item_uid AS LinkedItemUid,
                        linked_item_type AS LinkedItemType,
                        location_type_uid AS LocationTypeUid,
                        location_uid AS LocationUid
                    FROM 
                        location_mapping) AS SubQuery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);
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
                //if (pageNumber > 0 && pageSize > 0)
                //{
                //    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                //}
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ILocationMapping>().GetType();

                IEnumerable<Winit.Modules.Location.Model.Interfaces.ILocationMapping> LocationMappingDetails = await ExecuteQueryAsync<Winit.Modules.Location.Model.Interfaces.ILocationMapping>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Location.Model.Interfaces.ILocationMapping> pagedResponse = new PagedResponse<Winit.Modules.Location.Model.Interfaces.ILocationMapping>
                {
                    PagedData = LocationMappingDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.Location.Model.Interfaces.ILocationMapping> GetLocationMappingByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        linked_item_uid AS LinkedItemUid,
                        linked_item_type AS LinkedItemType,
                        location_type_uid AS LocationTypeUid,
                        location_uid AS LocationUid
                    FROM 
                        location_mapping WHERE UID = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ILocationMapping>().GetType();

            Winit.Modules.Location.Model.Interfaces.ILocationMapping LocationMappingDetails = await ExecuteSingleAsync<Winit.Modules.Location.Model.Interfaces.ILocationMapping>(sql, parameters, type);
            return LocationMappingDetails;
        }
        public async Task<int> CreateLocationMapping(Winit.Modules.Location.Model.Interfaces.ILocationMapping createLocationMapping)
        {
            try
            {
                var sql = @"INSERT INTO LocationMapping (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, linked_item_uid, linked_item_type, location_type_uid, location_uid) " +
          "VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @LinkedItemUID, @LocationTypeUID, @LinkedItemType, @LocationUID)";


                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID", createLocationMapping.UID},
                   {"CreatedBy", createLocationMapping.CreatedBy},
                   {"ModifiedBy", createLocationMapping.ModifiedBy},
                   {"CreatedTime", createLocationMapping.CreatedTime},
                   {"LinkedItemUID", createLocationMapping.LinkedItemUID},
                   {"LinkedItemType", createLocationMapping.LinkedItemType},
                   {"LocationTypeUID", createLocationMapping.LocationTypeUID},
                   {"LocationUID",createLocationMapping.LocationUID },
                   {"ModifiedTime", createLocationMapping.ModifiedTime},
                   {"ServerAddTime", createLocationMapping.ServerAddTime},
                   {"ServerModifiedTime", createLocationMapping.ServerModifiedTime},
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> InsertLocationHierarchy(string type, string uid)
        {
            throw new NotImplementedException();
        }

        public async Task<int> UpdateLocationMappingDetails(Winit.Modules.Location.Model.Interfaces.ILocationMapping updateLocationMapping)
        {
            try
            {
                var sql = @"UPDATE location_mapping SET 
             created_by = @CreatedBy, 
             created_time = @CreatedTime, 
             modified_by = @ModifiedBy, 
             modified_time = @ModifiedTime, 
             server_modified_time = @ServerModifiedTime, 
             linked_item_uid = @LinkedItemUID, 
             location_type_uid = @LocationTypeUID, 
             linked_item_type = @LinkedItemType, 
             location_uid = @LocationUID 
             WHERE uid = @UID";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID", updateLocationMapping.UID},
                   {"CreatedBy", updateLocationMapping.CreatedBy},
                   {"ModifiedBy", updateLocationMapping.ModifiedBy},
                   {"CreatedTime", updateLocationMapping.CreatedTime},
                   {"LinkedItemUID", updateLocationMapping.LinkedItemUID},
                   {"LinkedItemType", updateLocationMapping.LinkedItemType},
                   {"LocationTypeUID", updateLocationMapping.LocationTypeUID},
                   {"LocationUID",updateLocationMapping.LocationUID },
                   {"ModifiedTime", updateLocationMapping.ModifiedTime},
                   {"ServerModifiedTime", updateLocationMapping.ServerModifiedTime},
                 };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteLocationMappingDetails(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE  FROM location_mapping WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        public async Task<List<ILocationData>> GetLocationMaster()
        {
            throw new NotImplementedException();
        }
    }
}
