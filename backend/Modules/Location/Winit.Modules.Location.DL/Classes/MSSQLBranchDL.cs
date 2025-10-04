using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Location.DL.Interfaces;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Location.DL.Classes
{
    public class MSSQLBranchDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IBranchDL
    {
        public MSSQLBranchDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<PagedResponse<Winit.Modules.Location.Model.Interfaces.IBranch>> SelectAllBranchDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"select  * from 
                                            (SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, ss, code, 
                                             name, level1_count, level2_count, level3_count, level4_count, is_active, level1_data, level2_data,level3_data, level4_data, 
                                            special_state FROM branch) 
                                             as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM 
                                                (SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, ss, code, 
                                                 name, level1_count, level2_count, level3_count, level4_count, is_active, level1_data, level2_data,level3_data, level4_data, 
                                                special_state   FROM branch) 
                                              as SubQuery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Location.Model.Interfaces.IBranch>(filterCriterias, sbFilterCriteria, parameters);
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
                IEnumerable<Winit.Modules.Location.Model.Interfaces.IBranch> branchDetails = await ExecuteQueryAsync<Winit.Modules.Location.Model.Interfaces.IBranch>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Location.Model.Interfaces.IBranch> pagedResponse = new PagedResponse<Winit.Modules.Location.Model.Interfaces.IBranch>
                {
                    PagedData = branchDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.Location.Model.Interfaces.IBranch?> GetBranchByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time,
                             server_modified_time, ss, code,name, level1_count, level2_count, level3_count, level4_count, 
                             is_active, level1_data, level2_data,level3_data, level4_data, special_state  FROM branch
                             WHERE uid = @UID";

            return await ExecuteSingleAsync<Winit.Modules.Location.Model.Interfaces.IBranch>(sql, parameters);
        }
        public async Task<List<Winit.Modules.Location.Model.Interfaces.IBranch>> GetBranchsByJobPositionUid(string jobPositionUid)
        { 
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"JobPositionUid",  jobPositionUid}
            };
            var sql = @"select bh.* from branch bh
							inner join (select distinct a.branch_uid 
                        from store s
                        inner join Org o on s.uid = o.uid and s.type = 'FR' and s.is_available_to_use = 1
                        left join (
                        select branch_uid, linked_item_uid, type, is_default,
                        ROW_NUMBER() over (partition by linked_item_uid order by type desc) as rownum
                        from address
                        ) a on a.linked_item_uid = o.uid and a.rownum = 1
                          WHERE 
                              o.uid IN (
                        SELECT DISTINCT org_uid FROM my_orgs WHERE job_position_uid  = @JobPositionUid
                        )) sq on sq.branch_uid=bh.uid";

            return await ExecuteQueryAsync<Winit.Modules.Location.Model.Interfaces.IBranch>(sql, parameters);
        }
        public async Task<int> CreateBranch(Winit.Modules.Location.Model.Interfaces.IBranch createbranch)
        {
            int retVal = -1;
            try
            {
                var sql = @"INSERT INTO branch (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                           ss, code, name, level1_count, level2_count, level3_count, level4_count, is_active, level1_data, level2_data, 
                          level3_data, level4_data, special_state) 
                          VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @SS, 
                          @Code, @Name, @Level1Count, @Level2Count, @Level3Count, @Level4Count, @IsActive, @Level1Data, @Level2Data, @Level3Data, 
                          @Level4Data, @SpecialState)";
                retVal = await ExecuteNonQueryAsync(sql, createbranch);
            }
            catch (Exception ex)
            {
                throw;
            }
            return retVal;

        }
        public async Task<int> UpdateBranch(Winit.Modules.Location.Model.Interfaces.IBranch updatebranch)
        {
            try
            {
                var sql = @"UPDATE Branch
                            SET 
                                modified_by = @ModifiedBy,
                                modified_time = @ModifiedTime,
                                server_add_time = @ServerAddTime,
                                server_modified_time = @ServerModifiedTime,
                                ss = @SS,
                                code = @Code,
                                name = @Name,
                                level1_count = @Level1Count,
                                level2_count = @Level2Count,
                                level3_count = @Level3Count,
                                level4_count = @Level4Count,
                                is_active = @IsActive,
                                level1_data = @Level1Data,
                                level2_data = @Level2Data,
                                level3_data = @Level3Data,
                                level4_data = @Level4Data,
                                special_state = @SpecialState
                            WHERE 
                                uid = @UID;";
                return await ExecuteNonQueryAsync(sql, updatebranch);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<int> DeleteBranch(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE  FROM branch WHERE UID = @UID";

            return await ExecuteNonQueryAsync(sql, parameters);

        }
        public async Task<List<Winit.Modules.Location.Model.Interfaces.IBranch>> GetBranchByLocationHierarchy(string state, string city, string locality)
        {
            var sql = string.Empty;
            var parameters = new DynamicParameters();
            try
            {
                if (!string.IsNullOrEmpty(locality))
                {
                    sql = @"SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time,
                        server_modified_time, ss, code, name, level1_count, level2_count, level3_count, level4_count, 
                        is_active, level1_data, level2_data, level3_data, level4_data, special_state  
                FROM branch
                WHERE level3_count > 0 AND @locality IN (SELECT value FROM dbo.split(level3_data, ','))";

                    parameters.Add("locality", locality);
                }
                else if (!string.IsNullOrEmpty(city))
                {
                    sql = @"SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time,
                        server_modified_time, ss, code, name, level1_count, level2_count, level3_count, level4_count, 
                        is_active, level1_data, level2_data, level3_data, level4_data, special_state  
                FROM branch
                WHERE level2_count > 0 AND @city IN (SELECT value FROM dbo.split(level2_data, ','))";

                    parameters.Add("city", city);
                }
                else if (!string.IsNullOrEmpty(state))
                {
                    List<IBranch> branchDetailsState;
                    /*
                    sql = @"SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time,
                            server_modified_time, ss, code, name, level1_count, level2_count, level3_count, level4_count, 
                            is_active, level1_data, level2_data, level3_data, level4_data, special_state  
                            FROM branch
                            WHERE level1_count > 0 AND @state IN (SELECT value FROM dbo.split(special_state, ','))";
                    parameters.Add("state", state);

                    branchDetailsState = await ExecuteQueryAsync<Winit.Modules.Location.Model.Interfaces.IBranch>(sql, parameters);
                    if (!branchDetailsState.Any())
                    {
                        sql = @"SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time,
                                server_modified_time, ss, code, name, level1_count, level2_count, level3_count, level4_count, 
                                is_active, level1_data, level2_data, level3_data, level4_data, special_state  
                                FROM branch
                                WHERE level2_count = 0 AND level1_count > 0 AND @state IN (SELECT value FROM dbo.split(level1_data, ','))";

                        branchDetailsState = await ExecuteQueryAsync<Winit.Modules.Location.Model.Interfaces.IBranch>(sql, parameters);
                    }
                    */
                    sql = @"SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time,
                                server_modified_time, ss, code, name, level1_count, level2_count, level3_count, level4_count, 
                                is_active, level1_data, level2_data, level3_data, level4_data, special_state  
                                FROM branch
                                WHERE level2_count = 0 AND level1_count > 0 AND @state IN (SELECT value FROM dbo.split(level1_data, ','))";
                    parameters.Add("state", state);

                    branchDetailsState = await ExecuteQueryAsync<Winit.Modules.Location.Model.Interfaces.IBranch>(sql, parameters);
                    return branchDetailsState;
                }
                var branchDetails = await ExecuteQueryAsync<Winit.Modules.Location.Model.Interfaces.IBranch>(sql, parameters);
                return branchDetails.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }


    }
}
