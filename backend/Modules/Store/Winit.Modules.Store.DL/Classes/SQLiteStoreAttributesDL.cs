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
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.DL.Classes
{
    public class SQLiteStoreAttributesDL : Base.DL.DBManager.SqliteDBManager, Interfaces.IStoreAttributesDL
    {
        public SQLiteStoreAttributesDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        public async Task<IEnumerable<Model.Interfaces.IStoreAttributes>> SelectStoreAttributesByName(string attributeName)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"name",  attributeName}
            };
            var sql = @"SELECT 
                id AS Id, 
                uid AS UID, 
                created_by AS CreatedBy, 
                created_time AS CreatedTime, 
                modified_by AS ModifiedBy, 
                modified_time AS ModifiedTime, 
                server_add_time AS ServerAddTime, 
                server_modified_time AS ServerModifiedTime, 
                company_uid AS CompanyUID, 
                org_uid AS OrgUID, 
                distribution_channel_uid AS DistributionChannelUID, 
                store_uid AS StoreUID, 
                name AS Name, 
                code AS Code, 
                value AS Value, 
                parent_name AS ParentName
            FROM 
                store_attributes;
            WHERE name = @name";
            IEnumerable<Model.Interfaces.IStoreAttributes> storeAttributesList = await ExecuteQueryAsync< Model.Interfaces.IStoreAttributes>(sql, parameters);
            return await Task.FromResult(storeAttributesList);
        }
        public async Task<PagedResponse<Model.Interfaces.IStoreAttributes>> SelectAllStoreAttributes(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder("SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, " +
                    "modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, company_uid AS CompanyUID, org_uid AS OrgUID, " +
                    "distribution_channel_uid AS DistributionChannelUID, store_uid AS StoreUID, name AS Name, code AS Code, value AS Value, parent_name AS ParentName " +
                    "FROM store_attributes");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder("SELECT COUNT(1) AS Cnt FROM store_attributes");
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
                    if (sortCriterias != null && sortCriterias.Count > 0)
                    {
                        sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                    else
                    {
                        sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }

                //Data
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStoreAttributes>().GetType();
                IEnumerable<Model.Interfaces.IStoreAttributes> StoreAttributess = await ExecuteQueryAsync<Model.Interfaces.IStoreAttributes>(sql.ToString(), parameters, type);
                //Count
                int totalCount = 0;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreAttributes> pagedResponse = new PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreAttributes>
                {
                    PagedData = StoreAttributess,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Model.Interfaces.IStoreAttributes> SelectStoreAttributesByStoreUID(string storeUID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"storeUID",  storeUID}
            };
            var sql = @"SELECT id AS Id, 
                uid AS UID, 
                created_by AS CreatedBy, 
                created_time AS CreatedTime, 
                modified_by AS ModifiedBy, 
                modified_time AS ModifiedTime, 
                server_add_time AS ServerAddTime, 
                server_modified_time AS ServerModifiedTime, 
                company_uid AS CompanyUID, 
                org_uid AS OrgUID, 
                distribution_channel_uid AS DistributionChannelUID, 
                store_uid AS StoreUID, 
                name AS Name, 
                code AS Code, 
                value AS Value, 
                parent_name AS ParentName 
            FROM store_attributes WHERE store_uid = @storeUID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStoreAttributes>().GetType();
            Model.Interfaces.IStoreAttributes IStoreAttributesList = await ExecuteSingleAsync<Model.Interfaces.IStoreAttributes>(sql, parameters, type);
            return IStoreAttributesList;
        }
        public async Task<int> CreateStoreAttributes(Model.Interfaces.IStoreAttributes storeAttributes)
        {
            try
            {
                var sql = @"INSERT INTO store_attributes (uid, org_uid,company_uid, distribution_channel_uid, store_uid, 
                            name, code, value, parent_name, created_by, created_time, modified_by, modified_time, server_add_time, 
                            server_modified_time)
                            VALUES (@UID,@OrgUID, @CompanyUID, @DistributionChannelUID, @StoreUID, @Name, @Code, @Value, 
                            @ParentName,@CreatedBy, @CreatedTime,@ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime)";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID", storeAttributes.UID},
                    {"CompanyUID", storeAttributes.CompanyUID},
                    {"OrgUID", storeAttributes.OrgUID},
                    {"DistributionChannelUID", storeAttributes.DistributionChannelUID},
                    {"StoreUID", storeAttributes.StoreUID},
                    {"Name", storeAttributes.Name},
                    {"Code", storeAttributes.Code},
                    {"Value", storeAttributes.Value},
                    {"ParentName", storeAttributes.ParentName},
                    {"CreatedBy", storeAttributes.CreatedBy},
                    {"CreatedTime", storeAttributes.CreatedTime},
                    {"ModifiedBy", storeAttributes.ModifiedBy},
                    {"ModifiedTime", storeAttributes.ModifiedTime},
                    {"ServerAddTime", storeAttributes.ServerAddTime},
                    {"ServerModifiedTime", storeAttributes.ServerModifiedTime},
                };
                return await ExecuteNonQueryAsync(sql, parameters);
                 
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateStoreAttributes(Model.Interfaces.IStoreAttributes storeAttributes)
        {
            try
            {
                var sql = @"UPDATE store_attributes SET name=@name, code = @Code, value = @Value,
                            parent_name = @ParentName,modified_by=@ModifiedBy,modified_time=@ModifiedTime,
                            server_modified_time=@ServerModifiedTime WHERE store_uid = @StoreUID;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"StoreUID", storeAttributes.StoreUID},
                    {"Name", storeAttributes.Name},
                    {"Code", storeAttributes.Code},
                    {"Value", storeAttributes.Value},
                    {"ParentName", storeAttributes.ParentName},
                    {"ModifiedBy", storeAttributes.ModifiedBy},
                    {"ModifiedTime", storeAttributes.ModifiedTime},
                    {"ServerModifiedTime", storeAttributes.ServerModifiedTime},
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteStoreAttributes(string storeUID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"storeUID",storeUID}
            };
            var sql = "DELETE  FROM store_attributes WHERE store_uid = @storeUID";

            return await ExecuteNonQueryAsync(sql, parameters);
        }
        public async Task<IEnumerable<Model.Interfaces.IStoreAttributes>> GetStoreAttributesFiltered(string Name, string Email)
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<Model.Interfaces.IStoreAttributes>> GetStoreAttributesPaged(int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<Model.Interfaces.IStoreAttributes>> GetStoreAttributesSorted(List<SortCriteria> sortCriterias)
        {
            throw new NotImplementedException();
        }
    }
}








