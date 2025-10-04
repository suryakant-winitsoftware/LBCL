using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Winit.Modules.Base.Model;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.Store.DL.Classes
{
    public class SQLiteStoreGroupTypeDL : Base.DL.DBManager.SqliteDBManager, Interfaces.IStoreGroupTypeDL
    {
        public SQLiteStoreGroupTypeDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        public async Task<int> CreateStoreGroupType(Winit.Modules.Store.Model.Interfaces.IStoreGroupType storeGroupType)
        {
            try
            {
                var sql = @"INSERT INTO store_group_type (uid,created_by,created_time,modified_by,modified_time,server_add_time,server_modified_time,company_uid,org_uid,
                          distribution_channel_uid,name,parent_uid,level_no,code) VALUES(@UID,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,
                          @CompanyUID,@OrgUID,@DistributionChannelUID,@Name,@ParentUID,@LevelNo,@Code)";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {

                    {"UID", storeGroupType.UID},
                    {"CreatedBy", storeGroupType.CreatedBy},
                    {"CreatedTime", storeGroupType.CreatedTime},
                    {"ModifiedBy", storeGroupType.ModifiedBy},
                    {"ModifiedTime", storeGroupType.ModifiedTime},
                    {"ServerAddTime", storeGroupType.ServerAddTime},
                    {"ServerModifiedTime", storeGroupType.ServerModifiedTime},
                    {"CompanyUID", storeGroupType.CompanyUID},
                    {"OrgUID", storeGroupType.OrgUID},
                    {"DistributionChannelUID", storeGroupType.DistributionChannelUID},
                    {"Name", storeGroupType.Name},
                    {"ParentUID", storeGroupType.ParentUID},
                    {"Code", storeGroupType.Code},
                    {"LevelNo", storeGroupType.LevelNo}
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreGroupType>> SelectAllStoreGroupType(List<SortCriteria> sortCriterias, int pageNumber,
 int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder("SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy," +
                    "modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, company_uid AS CompanyUID," +
                    "org_uid AS OrgUID, distribution_channel_uid AS DistributionChannelUID, name AS Name,parent_uid AS ParentUID, level_no AS LevelNo, code AS Code" +
                    "FROM store_group_type;");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder("SELECT COUNT(1) AS Cnt FROM store_group_type");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);

                    sql.Append(sbFilterCriteria);
                    // If count required then add filters to count
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
                    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }

                //Data
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStoreGroupType>().GetType();
                IEnumerable<Model.Interfaces.IStoreGroupType> storeGroupTypes = await ExecuteQueryAsync<Model.Interfaces.IStoreGroupType>(sql.ToString(), parameters, type);
                //Count
                int totalCount = 0;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreGroupType> pagedResponse = new PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreGroupType>
                {
                    PagedData = storeGroupTypes,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.Store.Model.Interfaces.IStoreGroupType> SelectStoreGroupTypeByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy,
                    modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, company_uid AS CompanyUID,
                    org_uid AS OrgUID, distribution_channel_uid AS DistributionChannelUID, name AS Name,parent_uid AS ParentUID, level_no AS LevelNo, code AS Code FROM store_group_type WHERE uid = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStoreGroupType>().GetType();
            Model.Interfaces.IStoreGroupType StoreGroupTypeList = await ExecuteSingleAsync<Model.Interfaces.IStoreGroupType>(sql, parameters, type);
            return StoreGroupTypeList;
        }
        public async Task<int> UpdateStoreGroupType(Winit.Modules.Store.Model.Interfaces.IStoreGroupType storeGroupType)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  storeGroupType.UID},
                {"ModifiedBy",  storeGroupType.ModifiedBy},
                {"ModifiedTime",  storeGroupType.ModifiedTime},
                {"ServerModifiedTime",  storeGroupType.ServerModifiedTime},
                {"CompanyUID",  storeGroupType.CompanyUID},
                {"OrgUID",  storeGroupType.OrgUID},
                {"Name",  storeGroupType.Name},
                {"ParentUID",  storeGroupType.ParentUID},
                {"Code",  storeGroupType.Code},
                {"LevelNo",  storeGroupType.LevelNo}
            };
            var sql = @"Update store_group_type Set modified_by = @ModifiedBy, modified_time=@ModifiedTime, server_modified_time=@ServerModifiedTime,
                        company_uid=@CompanyUID, org_uid=@OrgUID, name=@Name, parent_uid = @ParentUID, level_no=@LevelNo, code = @Code WHERE uid=@UID";
            return await ExecuteNonQueryAsync(sql, parameters);
             
        }
        public async Task<int> DeleteStoreGroupType(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",UID}
            };
            var sql = "DELETE  FROM store_group_type WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
    }
}








