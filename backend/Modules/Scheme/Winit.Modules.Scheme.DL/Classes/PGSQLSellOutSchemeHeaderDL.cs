using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.DL.DBManager;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.DL.Classes
{
    public class PGSQLSellOutSchemeHeaderDL:PostgresDBManager, ISellOutSchemeHeaderDL
    {
        public PGSQLSellOutSchemeHeaderDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<ISellOutSchemeHeader>> SelectAllSellOutSchemeHeader(List<SortCriteria> sortCriterias, int pageNumber,
   int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (SELECT 
                                                org_uid, franchisee_org_uid, contribution_level1, contribution_level2, 
                                                total_credit_note, available_provision2_amount, available_provision3_amount, 
                                                standing_provision_amount, job_position_uid, emp_uid, line_count, status, 
                                                created_by, created_time, modified_by, modified_time, server_add_time, 
                                                server_modified_time,UID
                                            FROM 
                                                sell_out_scheme_header
                                            ) As SubQuery");

                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM 
                                    (SELECT 
                                            org_uid, franchisee_org_uid, contribution_level1, contribution_level2, 
                                            total_credit_note, available_provision2_amount, available_provision3_amount, 
                                            standing_provision_amount, job_position_uid, emp_uid, line_count, status, 
                                            created_by, created_time, modified_by, modified_time, server_add_time, 
                                            server_modified_time,UID
                                        FROM 
                                            sell_out_scheme_header
                                    ) As SubQuery");
                }

                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<ISellOutSchemeHeader>(filterCriterias, sbFilterCriteria, parameters);
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

                IEnumerable<ISellOutSchemeHeader> sellOutSchemeHeaders = await ExecuteQueryAsync<ISellOutSchemeHeader>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<ISellOutSchemeHeader> pagedResponse = new PagedResponse<ISellOutSchemeHeader>
                {
                    PagedData = sellOutSchemeHeaders,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ISellOutSchemeHeader> GetSellOutSchemeHeaderByUID(string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "UID", UID }
        };

                var sql = @"SELECT 
                        org_uid, franchisee_org_uid, contribution_level1, contribution_level2, 
                        total_credit_note, available_provision2_amount, available_provision3_amount, 
                        standing_provision_amount, job_position_uid, emp_uid, line_count, status, 
                        created_by, created_time, modified_by, modified_time, server_add_time, 
                        server_modified_time,UID
                    FROM 
                        sell_out_scheme_header
                    WHERE uid = @UID";

                return await ExecuteSingleAsync<ISellOutSchemeHeader>(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> CreateSellOutSchemeHeader(ISellOutSchemeHeader sellOutSchemeHeader)
        {
            try
            {
                var sql = @"
                INSERT INTO sell_out_scheme_header (
                        org_uid, franchisee_org_uid, contribution_level1, contribution_level2, 
                        total_credit_note, available_provision2_amount, available_provision3_amount, 
                        standing_provision_amount, job_position_uid, emp_uid, line_count, status, 
                        created_by, created_time, modified_by, modified_time, server_add_time, 
                        server_modified_time,UID
                    ) VALUES (
                        @OrgUid, @FranchiseeOrgUid, @ContributionLevel1, @ContributionLevel2, 
                        @TotalCreditNote, @AvailableProvision2Amount, @AvailableProvision3Amount, 
                        @StandingProvisionAmount, @JobPositionUid, @EmpUid, @LineCount, @Status, 
                        @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, 
                        @ServerModifiedTime,@UID);";

                return await ExecuteNonQueryAsync(sql, sellOutSchemeHeader);
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> UpdateSellOutSchemeHeader(ISellOutSchemeHeader sellOutSchemeHeader)
        {
            var sql = @"
               UPDATE sell_out_scheme_header
                    SET
                        org_uid = @OrgUid,
                        franchisee_org_uid = @FranchiseeOrgUid,
                        contribution_level1 = @ContributionLevel1,
                        contribution_level2 = @ContributionLevel2,
                        total_credit_note = @TotalCreditNote,
                        available_provision2_amount = @AvailableProvision2Amount,
                        available_provision3_amount = @AvailableProvision3Amount,
                        standing_provision_amount = @StandingProvisionAmount,
                        job_position_uid = @JobPositionUid,
                        emp_uid = @EmpUid,
                        line_count = @LineCount,
                        status = @Status,
                        created_by = @CreatedBy,
                        created_time = @CreatedTime,
                        modified_by = @ModifiedBy,
                        modified_time = @ModifiedTime,
                        server_add_time = @ServerAddTime,
                        server_modified_time = @ServerModifiedTime
                    WHERE
                        uid = @Uid;";

            return await ExecuteNonQueryAsync(sql, sellOutSchemeHeader);
        }

        public async Task<int> DeleteSellOutSchemeHeader(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
    {
        {"UID", UID}
    };
            var sql = @"DELETE FROM sell_out_scheme_header WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<bool> CrudSellOutMaster(ISellOutMasterScheme sellOutMasterScheme)
        {
            throw new NotImplementedException();
        }

        public Task<ISellOutMasterScheme> GetSellOutMasterByUID(string UID)
        {
            throw new NotImplementedException();
        }
    }
}
