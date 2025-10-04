using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Text;
using Winit.Modules.Base.DL.DBManager;
using Winit.Modules.Store.DL.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.DL.Classes
{
    public class MSSQLStoreAsmMappingDL : SqlServerDBManager, IStoreAsmMappingDL
    {
        public MSSQLStoreAsmMappingDL(IServiceProvider serviceProvider, IConfiguration config)
            : base(serviceProvider, config)
        {

        }
        public async Task<PagedResponse<IAsmDivisionMapping>> SelectAllStoreAsmMapping(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (SELECT 
                                              e.name AS AsmEmpName, 
                                              adm.created_time AS CreatedTime,
                                              adm.asm_emp_uid AS AsmEmpUID,
                                              o.name AS DivisionName, 
                                              adm.division_uid AS DivisionUID,
                                              adm.linked_item_type AS LinkedItemType, 
                                              adm.linked_item_uid AS LinkedItemUID,
                                              COALESCE(s1.name, s2.name) AS StoreName 
                                              FROM asm_division_mapping adm
                                              LEFT JOIN emp e ON e.uid = adm.asm_emp_uid
                                              LEFT JOIN org o ON o.uid = adm.division_uid
                                              LEFT JOIN store s1 ON s1.uid = adm.linked_item_uid  
                                              LEFT JOIN address a ON a.uid = adm.linked_item_uid 
                                              LEFT JOIN store s2 ON s2.uid = a.linked_item_uid) AS SubQuery");

                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                                              e.name AS AsmEmpName, 
                                              adm.created_time AS CreatedTime,
                                              adm.asm_emp_uid AS AsmEmpUID,
                                              o.name AS DivisionName, 
                                              adm.division_uid AS DivisionUID,
                                              adm.linked_item_type AS LinkedItemType, 
                                              adm.linked_item_uid AS LinkedItemUID,
                                              COALESCE(s1.name, s2.name) AS StoreName 
                                              FROM asm_division_mapping adm
                                              LEFT JOIN emp e ON e.uid = adm.asm_emp_uid
                                              LEFT JOIN org o ON o.uid = adm.division_uid
                                              LEFT JOIN store s1 ON s1.uid = adm.linked_item_uid  
                                              LEFT JOIN address a ON a.uid = adm.linked_item_uid 
                                              LEFT JOIN store s2 ON s2.uid = a.linked_item_uid) AS SubQuery");
                }

                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<IAsmDivisionMapping>(filterCriterias, sbFilterCriteria, parameters);
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
                else
                {
                    sql.Append(" ORDER BY CreatedTime");
                }

                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }

                IEnumerable<IAsmDivisionMapping> SchemeExcludeDetails = await ExecuteQueryAsync<IAsmDivisionMapping>(sql.ToString(), parameters);
                int totalCount = -1;

                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<IAsmDivisionMapping> pagedResponse = new PagedResponse<IAsmDivisionMapping>
                {
                    PagedData = SchemeExcludeDetails,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<List<IStoreAsmMapping>> GetExistingCustomersList(List<IStoreAsmMapping> storeAsmMappings)
        {
            try
            {
                var storeAsmMappingList = new List<StoreAsmMapping>();

                var table = new DataTable();
                table.Columns.Add("CustomerCode", typeof(string));
                table.Columns.Add("SiteCode", typeof(string));
                table.Columns.Add("Division", typeof(string));

                foreach (var data in storeAsmMappings)
                {
                    table.Rows.Add(data.CustomerCode, (string.IsNullOrEmpty(data.SiteCode) ? DBNull.Value : data.SiteCode), data.Division);
                }

                StringBuilder sql = new StringBuilder($@"
                                    -- Case when SiteCode is NULL
                                    SELECT DISTINCT 
                                        s.code AS CustomerCode, 
                                        '' AS SiteCode,  -- Use s.uid when SiteCode is NULL
                                        o.uid AS Division, 
                                        s.uid AS StoreUID, 
                                        s.uid AS SiteUID,  -- Using s.uid as SiteUID
                                        s.is_asm_mapped_by_customer AS IsAsmMappedByCustomer
                                    FROM store s 
                                    INNER JOIN address a ON s.uid = a.linked_item_uid  
                                    INNER JOIN org o ON o.parent_uid = a.org_unit_uid
                                    INNER JOIN @StoreAsmMappingTable tvp 
                                        ON s.code = tvp.CustomerCode 
                                        AND o.uid = tvp.Division
                                    WHERE tvp.SiteCode IS NULL

                                    UNION

                                    -- Case when SiteCode is NOT NULL
                                    SELECT DISTINCT 
                                        s.code AS CustomerCode, 
                                        a.custom_field3 AS SiteCode,  -- Use a.custom_field3 when SiteCode is NOT NULL
                                        o.uid AS Division, 
                                        s.uid AS StoreUID, 
                                        a.uid AS SiteUID,  -- Using a.uid as SiteUID
                                        s.is_asm_mapped_by_customer AS IsAsmMappedByCustomer
                                    FROM store s 
                                    INNER JOIN address a ON s.uid = a.linked_item_uid  
                                    INNER JOIN org o ON o.parent_uid = a.org_unit_uid
                                    INNER JOIN @StoreAsmMappingTable tvp 
                                        ON s.code = tvp.CustomerCode 
                                        AND o.uid = tvp.Division
                                        AND tvp.SiteCode IS NOT NULL 
                                        AND a.custom_field3 = tvp.SiteCode;");

                var parameters = new DynamicParameters();
                parameters.Add("@StoreAsmMappingTable", table.AsTableValuedParameter("StoreAsmMappingType"));

                return await ExecuteQueryAsync<IStoreAsmMapping>(sql.ToString(), parameters);

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<IStoreAsmMapping>> GetExistingEmpList(List<string> EmpCodes)
        {
            try
            {
                List<IStoreAsmMapping> storeAsmMappingList = new();

                // Create a DataTable for TVP
                var table = new DataTable();
                table.Columns.Add("EmpCode", typeof(string));

                foreach (var data in EmpCodes)
                {
                    table.Rows.Add(data);
                }

                // SQL Query using TVP
                const string sql = @"SELECT e.code EmpCode, e.uid EmpUID 
                                    FROM emp e 
                                    INNER JOIN @EmpCodeTable t ON e.code = t.EmpCode;";

                var parameters = new DynamicParameters();
                parameters.Add("@EmpCodeTable", table.AsTableValuedParameter("EmpCodeType")); // Pass TVP

                return await ExecuteQueryAsync<IStoreAsmMapping>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        
        public async Task<List<IStoreAsmMapping>> CheckBranchSameOrNot(List<IStoreAsmMapping> storeAsmMappings)
        {
            try
            {
                // Create a DataTable for TVP
                var table = new DataTable();
                table.Columns.Add("CustomerCode", typeof(string));
                table.Columns.Add("EmpCode", typeof(string));

                foreach (var data in storeAsmMappings)
                {
                    table.Rows.Add(data.CustomerCode, data.EmpCode);
                }

                // SQL Query using TVP
                const string sql = @"SELECT DISTINCT 
                                    s.code AS CustomerCode, 
                                    s.name AS CustomerName, 
                                    a.branch_uid AS Branch, 
                                    e.code AS EmpCode, 
                                    e.name AS EmpName
                                    FROM address a
                                    INNER JOIN store s ON s.uid = a.linked_item_uid
                                    INNER JOIN job_position j ON j.branch_uid = a.branch_uid
                                    INNER JOIN emp e ON e.uid = j.emp_uid
                                    INNER JOIN @BranchCheckTable b ON s.code = b.CustomerCode AND e.code = b.EmpCode
                                    WHERE a.branch_uid IS NOT NULL 
                                    AND j.branch_uid IS NOT NULL
                                    AND a.branch_uid = j.branch_uid;";

                var parameters = new DynamicParameters();
                parameters.Add("@BranchCheckTable", table.AsTableValuedParameter("BranchCheckType")); // Pass TVP

                return await ExecuteQueryAsync<IStoreAsmMapping>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<int> CUAsmMapping(List<IAsmDivisionMapping> asmDivisionMapping)
        {
            int count = -1;
            try
            {
                foreach (var asmMapping in asmDivisionMapping)
                {
                    string? existingUID = await CheckIfDataExistsInDB(DbTableName.AsmDivisionMapping, asmMapping.LinkedItemUID, asmMapping.DivisionUID);
                    if (existingUID != null)
                    {
                        count = await UpdateAsmDivisionMapping(asmMapping);
                    }
                    else
                    {
                        count = await CreateAsmDivisionMapping(asmMapping);
                    }
                }
                return count;
            }
            catch
            {
                throw;
            }
        }
        public async Task<int> CreateAsmDivisionMapping(IAsmDivisionMapping asmDivisionMapping)
        {
            try
            {
                var sql =
                    @"INSERT INTO dbo.asm_division_mapping (uid, ss, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                            linked_item_type, linked_item_uid, division_uid, asm_emp_uid)
                            VALUES (@UID, @Ss, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @LinkedItemType, @LinkedItemUID
                            , @DivisionUID, @AsmEmpUID);";

                return await ExecuteNonQueryAsync(sql, asmDivisionMapping);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateAsmDivisionMapping(Model.Interfaces.IAsmDivisionMapping AsmDivisionMapping)
        {
            try
            {
                var sql = @"UPDATE asm_division_mapping 
                            SET
                                modified_by = @ModifiedBy, 
                                modified_time = @ModifiedTime, 
                                server_modified_time = @ServerModifiedTime,
                                linked_item_type = @LinkedItemType, 
                                linked_item_uid = @LinkedItemUID, 
                                division_uid = @DivisionUID, 
                                asm_emp_uid = @AsmEmpUID
                            WHERE 
                                linked_item_uid = @LinkedItemUID
                                And division_uid = @DivisionUID";

                return await ExecuteNonQueryAsync(sql, AsmDivisionMapping);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<string?> CheckIfDataExistsInDB(string tableName, string LinkedItemUID, string DivisionUID)
        {
            try
            {
                string sql = $@"SELECT uid AS UID FROM {tableName} WHERE linked_item_uid = @LinkedItemUID AND division_uid = @DivisionUID";
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {
                        "LinkedItemUID", LinkedItemUID

                    },
                    {
                        "DivisionUID", DivisionUID

                    }
                };
                return await ExecuteSingleAsync<string>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
