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
    public class MSSQLStoreAttributesDL : Base.DL.DBManager.SqlServerDBManager, Interfaces.IStoreAttributesDL
    {
        public MSSQLStoreAttributesDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<IEnumerable<Model.Interfaces.IStoreAttributes>> SelectStoreAttributesByName(string attributeName)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"name",  attributeName}
            };
                                        var sql = @"Select * From (SELECT 
                                sa.id AS Id, 
                                sa.uid AS UID, 
                                sa.created_by AS CreatedBy, 
                                sa.created_time AS CreatedTime, 
                                sa.modified_by AS ModifiedBy, 
                                sa.modified_time AS ModifiedTime,
                                sa.server_add_time AS ServerAddTime, 
                                sa.server_modified_time AS ServerModifiedTime, 
                                sa.company_uid AS CompanyUID, 
                                sa.org_uid AS OrgUID, 
                                sa.distribution_channel_uid AS DistributionChannelUID,
                                sa.store_uid AS StoreUID, 
                                sa.name AS Name, 
                                sa.code AS Code, 
                                sa.value AS Value, 
                                sa.parent_name AS ParentName 
                            FROM 
                                store_attributes sa 
                            WHERE 
                                sa.name = @name)As SubQuery";
            IEnumerable<Model.Interfaces.IStoreAttributes> storeAttributesList = await ExecuteQueryAsync< Model.Interfaces.IStoreAttributes>(sql, parameters);
            return await Task.FromResult(storeAttributesList);
        }
        public async Task<PagedResponse<Model.Interfaces.IStoreAttributes>> SelectAllStoreAttributes(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * 
                                                FROM (
                                                    SELECT 
                                                        sa.id AS Id, 
                                                        sa.uid AS UID, 
                                                        sa.created_by AS CreatedBy, 
                                                        sa.created_time AS CreatedTime, 
                                                        sa.modified_by AS ModifiedBy, 
                                                        sa.modified_time AS ModifiedTime,
                                                        sa.server_add_time AS ServerAddTime, 
                                                        sa.server_modified_time AS ServerModifiedTime, 
                                                        sa.company_uid AS CompanyUID, 
                                                        sa.org_uid AS OrgUID, 
                                                        sa.distribution_channel_uid AS DistributionChannelUID,
                                                        sa.store_uid AS StoreUID, 
                                                        sa.name AS Name, 
                                                        sa.code AS Code, 
                                                        sa.value AS Value, 
                                                        sa.parent_name AS ParentName 
                                                    FROM 
                                                        store_attributes sa
                                                ) AS subquery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt 
                                                        FROM 
                                                        (SELECT 
                                                        sa.id AS Id, 
                                                        sa.uid AS UID, 
                                                        sa.created_by AS CreatedBy, 
                                                        sa.created_time AS CreatedTime, 
                                                        sa.modified_by AS ModifiedBy, 
                                                        sa.modified_time AS ModifiedTime,
                                                        sa.server_add_time AS ServerAddTime, 
                                                        sa.server_modified_time AS ServerModifiedTime, 
                                                        sa.company_uid AS CompanyUID, 
                                                        sa.org_uid AS OrgUID, 
                                                        sa.distribution_channel_uid AS DistributionChannelUID,
                                                        sa.store_uid AS StoreUID, 
                                                        sa.name AS Name, 
                                                        sa.code AS Code, 
                                                        sa.value AS Value, 
                                                        sa.parent_name AS ParentName 
                                                    FROM 
                                                        store_attributes sa
                                                ) AS subquery");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Model.Interfaces.IStoreAttributes>(filterCriterias, sbFilterCriteria, parameters);

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
                IEnumerable<Model.Interfaces.IStoreAttributes> StoreAttributess = await ExecuteQueryAsync<Model.Interfaces.IStoreAttributes>(sql.ToString(), parameters);
                int totalCount = 0;
                if (isCountRequired)
                {
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
                                var sql = @"SELECT 
                        sa.id AS Id, 
                        sa.uid AS UID, 
                        sa.created_by AS CreatedBy, 
                        sa.created_time AS CreatedTime, 
                        sa.modified_by AS ModifiedBy, 
                        sa.modified_time AS ModifiedTime,
                        sa.server_add_time AS ServerAddTime, 
                        sa.server_modified_time AS ServerModifiedTime, 
                        sa.company_uid AS CompanyUID, 
                        sa.org_uid AS OrgUID, 
                        sa.distribution_channel_uid AS DistributionChannelUID,
                        sa.store_uid AS StoreUID, 
                        sa.name AS Name, 
                        sa.code AS Code, 
                        sa.value AS Value, 
                        sa.parent_name AS ParentName 
                    FROM 
                        store_attributes sa 
                    WHERE 
                        sa.uid = @storeUID";
            return await ExecuteSingleAsync<Model.Interfaces.IStoreAttributes>(sql, parameters);
             
        }
        public async Task<int> CreateStoreAttributes(Model.Interfaces.IStoreAttributes storeAttributes)
        {
            try
            {
                var sql = @"INSERT INTO StoreAttributes (
                                                        uid, 
                                                        orguid,
                                                        companyuid, 
                                                        distributionchanneluid, 
                                                        storeuid, 
                                                        name, 
                                                        code, 
                                                        value, 
                                                        parentname, 
                                                        createdby, 
                                                        createdtime, 
                                                        modifiedby, 
                                                        modifiedtime, 
                                                        serveraddtime, 
                                                        servermodifiedtime
                                                    )
                                                    VALUES (
                                                        @UID, 
                                                        @OrgUID, 
                                                        @CompanyUID, 
                                                        @DistributionChannelUID, 
                                                        @StoreUID, 
                                                        @Name, 
                                                        @Code, 
                                                        @Value, 
                                                        @ParentName, 
                                                        @CreatedBy, 
                                                        @CreatedTime, 
                                                        @ModifiedBy, 
                                                        @ModifiedTime, 
                                                        @ServerAddTime, 
                                                        @ServerModifiedTime)";
                
                return await ExecuteNonQueryAsync(sql, storeAttributes);
                 
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
                var sql = @"UPDATE store_attributes 
                                        SET 
                                            modified_by = @ModifiedBy, 
                                            modified_time = @ModifiedTime,
                                            server_modified_time = @ServerModifiedTime, 
                                            company_uid = @CompanyUID, 
                                            org_uid = @OrgUID, 
                                            distribution_channel_uid = @DistributionChannelUID,
                                            store_uid = @StoreUID, 
                                            name = @Name, 
                                            code = @Code, 
                                            value = @Value, 
                                            parent_name = @ParentName 
                                        WHERE 
                                            store_uid = @StoreUID;";
                
                return await ExecuteNonQueryAsync(sql, storeAttributes);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteStoreAttributes(string storeUID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"storeUID",storeUID}
            };
                var sql = "DELETE  FROM store_attributes WHERE store_uid = @storeUID";

                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch
            {
                throw;
            }
          
             
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








