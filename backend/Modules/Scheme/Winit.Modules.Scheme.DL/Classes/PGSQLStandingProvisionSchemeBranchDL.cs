using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.DL.DBManager;
using Winit.Modules.Scheme.DL.Interfaces;
using System.Threading.Tasks;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.DL.Classes
{
    public class PGSQLStandingProvisionSchemeBranchDL:PostgresDBManager, IStandingProvisionSchemeBranchDL
    {
        public PGSQLStandingProvisionSchemeBranchDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<IStandingProvisionSchemeBranch>> SelectAllStandingProvisionBranches(List<SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (SELECT 
                                    id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, ss, standing_provision_scheme_uid, branch_uid
                                FROM 
                                    public.standing_provision_scheme_branch
                                ) AS SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM 
                                (SELECT 
                                    id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, ss, standing_provision_scheme_uid, branch_uid
                                FROM 
                                    public.standing_provision_scheme_branch
                                ) AS SubQuery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<IStandingProvisionSchemeBranch>(filterCriterias, sbFilterCriteria, parameters);
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
                        sql.Append($" ORDER BY uid OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }
                IEnumerable<IStandingProvisionSchemeBranch> standingProvisionBranches = await ExecuteQueryAsync<IStandingProvisionSchemeBranch>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<IStandingProvisionSchemeBranch> pagedResponse = new PagedResponse<IStandingProvisionSchemeBranch>
                {
                    PagedData = standingProvisionBranches,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IStandingProvisionSchemeBranch> GetStandingProvisionBranchByUID(string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "UID", UID }
            };

                var sql = @"SELECT 
                            id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, ss, standing_provision_scheme_uid, branch_uid
                        FROM 
                            public.standing_provision_scheme_branch
                        WHERE uid = @UID";

                return await ExecuteSingleAsync<IStandingProvisionSchemeBranch>(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> CreateStandingProvisionBranch(IStandingProvisionSchemeBranch standingProvisionBranch)
        {
            try
            {
                var sql = @"
                INSERT INTO public.standing_provision_scheme_branch (
                    id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, ss, standing_provision_scheme_uid, branch_uid
                ) VALUES (
                    @Id, @Uid, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @Ss, @StandingProvisionSchemeUID, @BranchUid);";

                return await ExecuteNonQueryAsync(sql, standingProvisionBranch);
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> UpdateStandingProvisionBranch(IStandingProvisionSchemeBranch standingProvisionBranch)
        {
            var sql = @"
            UPDATE public.standing_provision_scheme_branch
                SET
                    id = @Id,
                    uid = @Uid,
                    created_by = @CreatedBy,
                    created_time = @CreatedTime,
                    modified_by = @ModifiedBy,
                    modified_time = @ModifiedTime,
                    server_add_time = @ServerAddTime,
                    server_modified_time = @ServerModifiedTime,
                    ss = @Ss,
                    standing_provision_scheme_uid = @StandingProvisionSchemeUID,
                    branch_uid = @BranchUid
                WHERE
                    uid = @Uid;";

            return await ExecuteNonQueryAsync(sql, standingProvisionBranch);
        }

        public async Task<int> DeleteStandingProvisionBranch(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "UID", UID }
        };
            var sql = @"DELETE FROM public.standing_provision_scheme_branch WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
    }
}
