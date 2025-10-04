using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.DL.DBManager;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.DL.Classes
{
    public class PGSQLSalesPromotionSchemeDL  : PostgresDBManager, ISalesPromotionSchemeDL
    {
        public PGSQLSalesPromotionSchemeDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<ISalesPromotionScheme>> SelectAllSalesPromotionScheme(
        List<SortCriteria> sortCriterias,
        int pageNumber,
        int pageSize,
        List<FilterCriteria> filterCriterias,
        bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (SELECT 
                                                 org_uid, franchisee_org_uid, contribution_level1, 
                                                 contribution_level2, available_provision2_amount, 
                                                 available_provision3_amount, standing_provision_amount, 
                                                 job_position_uid, emp_uid, status, activity_type, 
                                                 from_date, to_date, amount, description, po_number, 
                                                 po_date, remarks, uid, 
                                                        created_by, created_time, modified_by, modified_time, server_add_time, 
                                                        server_modified_time
                                             FROM 
                                                 sales_promotion_scheme
                                             ) As SubQuery");

                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM 
                                 (SELECT 
                                         org_uid, franchisee_org_uid, contribution_level1, 
                                         contribution_level2, available_provision2_amount, 
                                         available_provision3_amount, standing_provision_amount, 
                                         job_position_uid, emp_uid, status, activity_type, 
                                         from_date, to_date, amount, description, po_number, 
                                         po_date, remarks, uid, 
                                                        created_by, created_time, modified_by, modified_time, server_add_time, 
                                                        server_modified_time
                                     FROM 
                                         sales_promotion_scheme
                                 ) As SubQuery");
                }

                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<ISalesPromotionScheme>(filterCriterias, sbFilterCriteria, parameters);
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
                        sql.Append($" ORDER BY org_uid OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }

                IEnumerable<ISalesPromotionScheme> salesPromotionSchemes = await ExecuteQueryAsync<ISalesPromotionScheme>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<ISalesPromotionScheme> pagedResponse = new PagedResponse<ISalesPromotionScheme>
                {
                    PagedData = salesPromotionSchemes,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ISalesPromotionScheme> GetSalesPromotionSchemeByUID(string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "UID", UID }
            };

                var sql = @"SELECT 
                    org_uid, franchisee_org_uid, contribution_level1, contribution_level2, 
                    available_provision2_amount, available_provision3_amount, standing_provision_amount, 
                    job_position_uid, emp_uid, status, activity_type, from_date, to_date, 
                    amount, description, po_number, po_date, remarks, uid, 
                                                        created_by, created_time, modified_by, modified_time, server_add_time, 
                                                        server_modified_time
                FROM 
                    sales_promotion_scheme
                WHERE uid = @UID";

                return await ExecuteSingleAsync<ISalesPromotionScheme>(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> CreateSalesPromotionScheme(SalesPromotionSchemeApprovalDTO salesPromotionSchemeApprovalDTO)
        {
            try
            {
                var sql = @"
            INSERT INTO sales_promotion_scheme (
                    org_uid, franchisee_org_uid, contribution_level1, contribution_level2, 
                    available_provision2_amount, available_provision3_amount, standing_provision_amount, 
                    job_position_uid, emp_uid, status, activity_type, from_date, to_date, 
                    amount, description, po_number, po_date, remarks, uid, 
                                                        created_by, created_time, modified_by, modified_time, server_add_time, 
                                                        server_modified_time
                ) VALUES (
                    @OrgUid, @FranchiseeOrgUid, @ContributionLevel1, @ContributionLevel2, 
                    @AvailableProvision2Amount, @AvailableProvision3Amount, @StandingProvisionAmount, 
                    @JobPositionUid, @EmpUid, @Status, @ActivityType, @FromDate, @ToDate, 
                    @Amount, @Description, @PoNumber, @PoDate, @Remarks, @Uid, 
                                @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, 
                                @ServerModifiedTime);";

                return await ExecuteNonQueryAsync(sql, salesPromotionSchemeApprovalDTO.SalesPromotion);
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> UpdateSalesPromotionScheme(SalesPromotionSchemeApprovalDTO salesPromotionSchemeApprovalDTO)
        {
            var sql = @"
            UPDATE sales_promotion_scheme
                 SET
                     org_uid = @OrgUid,
                     franchisee_org_uid = @FranchiseeOrgUid,
                     contribution_level1 = @ContributionLevel1,
                     contribution_level2 = @ContributionLevel2,
                     available_provision2_amount = @AvailableProvision2Amount,
                     available_provision3_amount = @AvailableProvision3Amount,
                     standing_provision_amount = @StandingProvisionAmount,
                     job_position_uid = @JobPositionUid,
                     emp_uid = @EmpUid,
                     status = @Status,
                     activity_type = @ActivityType,
                     from_date = @FromDate,
                     to_date = @ToDate,
                     amount = @Amount,
                     description = @Description,
                     po_number = @PoNumber,
                     po_date = @PoDate,
                     remarks = @Remarks,
                     modified_by=@ModifiedBy, 
                     modified_time=@ModifiedTime,
                     server_modified_time=@ServerModifiedTime
             WHERE
                 uid = @Uid;";

            return await ExecuteNonQueryAsync(sql, salesPromotionSchemeApprovalDTO.SalesPromotion);
        }

        public async Task<int> DeleteSalesPromotionScheme(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            {"UID", UID}
        };
            var sql = @"DELETE FROM sales_promotion_scheme WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
    }
}
