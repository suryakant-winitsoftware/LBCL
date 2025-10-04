using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    public class PGSQLStoreAttributesDL : Base.DL.DBManager.PostgresDBManager, Interfaces.IStoreAttributesDL
    {
        public PGSQLStoreAttributesDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<IEnumerable<Model.Interfaces.IStoreAttributes>> SelectStoreAttributesByName(string attributeName)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"name",  attributeName}
            };

            var sql = @"SELECT * FROM ""StoreAttributes"" WHERE ""Name"" = @name";

            IEnumerable<Model.Interfaces.IStoreAttributes> storeAttributesList = await ExecuteQueryAsync<Model.Interfaces.IStoreAttributes>(sql, parameters);
            return await Task.FromResult(storeAttributesList);
        }
        public async Task<PagedResponse<Model.Interfaces.IStoreAttributes>> SelectAllStoreAttributes(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM ""StoreAttributes""");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM ""StoreAttributes""");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Model.Interfaces.IStoreAttributes>(filterCriterias, sbFilterCriteria, parameters);

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
        public async Task<Model.Interfaces.IStoreAttributes> SelectStoreAttributesByStoreUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };

            var sql = @"SELECT * FROM ""StoreAttributes"" WHERE ""UID"" = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStoreAttributes>().GetType();
            Model.Interfaces.IStoreAttributes IStoreAttributesList = await ExecuteSingleAsync<Model.Interfaces.IStoreAttributes>(sql, parameters, type);
            return IStoreAttributesList;
        }

        public async Task<int> CreateStoreAttributes(Model.Interfaces.IStoreAttributes storeAttributes)
        {
            try
            {
                var sql = @"INSERT INTO ""StoreAttributes"" (""UID"",""CreatedBy"",""CreatedTime"",""ModifiedBy"",""ModifiedTime"",""ServerAddTime"",
                            ""ServerModifiedTime"",""CompanyUID"",""OrgUID"",""DistributionChannelUID"",""StoreUID"",""Name"",""Code"",""Value"",""ParentName"")
                            VALUES (@UID ,@CreatedBy ,@CreatedTime ,@ModifiedBy ,@ModifiedTime ,@ServerAddTime ,@ServerModifiedTime ,@CompanyUID ,@OrgUID ,
                            @DistributionChannelUID ,@StoreUID ,@Name ,@Code ,@Value ,@ParentName)";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID",storeAttributes.UID},
                    {"CreatedBy",storeAttributes.CreatedBy},
                    {"CreatedTime",storeAttributes.CreatedTime},
                    {"ModifiedBy",storeAttributes.ModifiedBy},
                    {"ModifiedTime",storeAttributes.ModifiedTime},
                    {"ServerAddTime",storeAttributes.ServerAddTime},
                    {"ServerModifiedTime",storeAttributes.ServerModifiedTime},
                    {"CompanyUID",storeAttributes.CompanyUID},
                    {"OrgUID",storeAttributes.OrgUID},
                    {"DistributionChannelUID",storeAttributes.DistributionChannelUID},
                    {"StoreUID",storeAttributes.StoreUID},
                    {"Name",storeAttributes.Name},
                    {"Code",storeAttributes.Code},
                    {"Value",storeAttributes.Value},
                    {"ParentName",storeAttributes.ParentName}
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
                var sql = @"UPDATE ""StoreAttributes"" SET   
                           ""ModifiedBy""=@ModifiedBy
                          ,""ModifiedTime""	=@ModifiedTime
                          ,""ServerModifiedTime""=@ServerModifiedTime
                          ,""CompanyUID""=@CompanyUID
                          ,""OrgUID""=@OrgUID
                          ,""DistributionChannelUID""=@DistributionChannelUID
                          ,""StoreUID""	=@StoreUID
                          ,""Name""	=@Name
                          ,""Code""	=@Code
                          ,""Value""=@Value
                          ,""ParentName""=@ParentName
                          WHERE ""UID"" = @UID;";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID",storeAttributes.UID},
                    {"ModifiedBy",storeAttributes.ModifiedBy},
                    {"ModifiedTime",storeAttributes.ModifiedTime},
                    {"ServerModifiedTime",storeAttributes.ServerModifiedTime},
                    {"CompanyUID",storeAttributes.CompanyUID},
                    {"OrgUID",storeAttributes.OrgUID},
                    {"DistributionChannelUID",storeAttributes.DistributionChannelUID},
                    {"StoreUID",storeAttributes.StoreUID},
                    {"Name",storeAttributes.Name},
                    {"Code",storeAttributes.Code},
                    {"Value",storeAttributes.Value},
                    {"ParentName",storeAttributes.ParentName},
                };
                return await ExecuteNonQueryAsync(sql, parameters);
             
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteStoreAttributes(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",UID}
            };
            var sql = @"DELETE  FROM ""StoreAttributes"" WHERE ""UID"" = @UID";

            return await ExecuteNonQueryAsync(sql, parameters);
         
        }
        public async Task<IEnumerable<Model.Interfaces.IStoreAttributes>> GetStoreAttributesFiltered(string Name, String Email)
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








