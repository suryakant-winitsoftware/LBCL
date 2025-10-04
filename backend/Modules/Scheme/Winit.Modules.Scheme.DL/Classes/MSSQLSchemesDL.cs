using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Winit.Modules.ApprovalEngine.BL.Interfaces;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.Base.DL.DBManager;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common;

namespace Winit.Modules.Scheme.DL.Classes
{
    public class MSSQLSchemesDL : SqlServerDBManager, ISchemesDL
    {
        private readonly IApprovalEngineHelper _approvalEngineHelper;
        public MSSQLSchemesDL(IServiceProvider serviceProvider, IConfiguration config, IApprovalEngineHelper approvalEngineHelper) : base(serviceProvider, config)
        {
            _approvalEngineHelper = approvalEngineHelper;
        }
        public async Task<PagedResponse<IManageScheme>> SelectAllSchemes(List<SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string jobPositionUid, bool isAdmin)
        {
            try
            {
                bool isExpired = false;
                FilterCriteria? filterCriteria = filterCriterias.Find(p => p.Name.Equals(SchemeConstants.ShowInactive));
                //if (filterCriteria != null)
                //{
                //    filterCriterias.Remove(filterCriteria);
                //    isExpired = CommonFunctions.GetBooleanValue(filterCriteria.Value);
                //}
                string Expired = isExpired ? " where DATEDIFF(dd, SQ.ValidUpto, GETDATE()) > 0" : "where DATEDIFF(dd, GETDATE(), SQ.ValidUpto) >= 0";
                string subQuery = isAdmin ? string.Empty : @" and emp.uid in
														(SELECT DISTINCT emp_uid from my_team mt 
                                                  inner join 
                                                    job_position jp ON jp.uid = mt.team_job_position_uid 
                                                                                    where job_position_uid =@JobPositionUid)";

                string cte = @$"WITH Schemes AS (
                                                    SELECT 
                                                       /*'[' + s.code + ']' + ISNULL(s.name, '') AS ChannelPartner,
                                                        ISNULL('[' + br.code + ']' + ISNULL(br.name, ''), 'N/A') AS Branch,
                                                        e.name AS EmpName,br.uid BranchUID,
		                                                */
		                                                ''  AS ChannelPartner, '' AS Branch, e.name AS EmpName, '' AS BranchUID,
                                                            SQ.*
                                                        FROM (
                                                            SELECT sish.uid, '' AS ChannelPartnerUID, 'Sell In' AS SchemeType,
                                                  sish.created_by AS CreatedBy, sish.created_time AS CreatedOn,
                                                   sish.valid_from AS ValidFrom, sish.end_date AS ValidUpto, 
                                                   sish.modified_time AS LastUpdated, sish.Status,code,0 as HasHistory
                                            FROM sell_in_scheme_header sish
                                            
                                            UNION ALL
                                            
                                            SELECT sosh.uid, sosh.franchisee_org_uid AS ChannelPartnerUID, 'Sell Out' AS SchemeType,
                                                   sosh.created_by AS CreatedBy, sosh.created_time AS CreatedOn,
                                                   NULL AS ValidFrom, NULL AS ValidUpto, sosh.modified_time AS LastUpdated,
                                                   sosh.Status,code,0 as HasHistory
                                            FROM sell_out_scheme_header sosh
                                            
                                            UNION ALL
                                            
                                                   
                                            SELECT p.uid, p.org_uid AS ChannelPartnerUID, p.type AS SchemeType,
                                                   p.created_by_emp_uid AS CreatedBy, p.created_time AS CreatedOn,
                                                   p.valid_from AS ValidFrom, p.valid_upto AS ValidUpto, 
                                                   p.modified_time AS LastUpdated, p.status AS Status,code,has_history as HasHistory
                                            FROM promotion p
                                            WHERE p.type IN ('QPS', 'SellOutActualSecondary')
                                            
                                            UNION ALL
                                            
                                                  
                                            SELECT p.uid, p.franchisee_org_uid AS ChannelPartnerUID, 'Sales Promotion' AS SchemeType,
                                                   p.created_by AS CreatedBy, p.created_time AS CreatedOn,
                                                   p.from_date AS ValidFrom, p.to_date AS ValidUpto, p.modified_time AS LastUpdated,
                                                   p.status AS Status,code,0 as HasHistory
                                            FROM sales_promotion_scheme p where status not in ('{SchemeConstants.Approved}','{SchemeConstants.Executed}') ) AS SQ
                                                        /*
                                                        left JOIN org o ON o.uid = SQ.ChannelPartnerUID
                                                        left JOIN store s ON o.uid = s.uid
                                                        left JOIN (
                                                            -- Join for BranchWithAddress in subquery
                                                            SELECT br.code, br.uid, br.name, ad.linked_item_uid AS StoreUID
                                                            FROM address ad
                                                            INNER JOIN branch br ON br.uid = ad.branch_uid  and ad.is_default=1 and type='Shipping'
												
												


                                                        ) AS br ON br.StoreUID = s.uid
	                                                    */
                                                        INNER JOIN emp e ON SQ.CreatedBy = e.uid
														inner join job_position jp on e.uid =jp.emp_uid 
														AND (jp.branch_uid is null OR jp.branch_uid IN (
			                                                    select distinct br.uid from branch br 
			                                                    inner join address ad ON br.uid = ad.branch_uid  and ad.is_default=1 and type='Shipping'
			                                                    inner join store st on ad.linked_item_uid=st.uid
			                                                    inner join my_orgs mo on mo.org_uid=st.uid and mo.job_position_uid='Preetraj Bakshi' 
		                                                    )
	                                                    )
                                                      
                                                    )";
                var sql = new StringBuilder(@$"    
                                                {cte}

                                                    SELECT *
                                                    FROM Schemes");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@$" {cte}
                                                    
                                                    SELECT Count(1) as cnt
                                                    FROM Schemes");
                }
                var parameters = new Dictionary<string, object>()
                {
                    {"JobPositionUid",jobPositionUid }
                };
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
                        sql.Append($" ORDER BY LastUpdated desc OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }
                IEnumerable<IManageScheme> standingProvisionBranches = await ExecuteQueryAsync<IManageScheme>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<IManageScheme> pagedResponse = new PagedResponse<IManageScheme>
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

        //public async Task<List<IStore>> GetChannelPartner(string branchUID)
        //{
        //    var param = new Dictionary<string, object>
        //    {
        //        { "BranchUID",branchUID}
        //    };
        //    var sql = @"select s.*,a.branch_uid,a.uid as ShippingAddressUID from store s
        //                 inner join Org o on s.uid = o.uid and s.type = 'FR'
        //           inner join address a on a.linked_item_uid=s.uid and a.type='Shipping' and is_default=1 
        //            and a.branch_uid=@BranchUID ";
        //    IEnumerable<IStore> standingProvisionBranches = await ExecuteQueryAsync<IStore>(sql.ToString(), param);
        //    return standingProvisionBranches.ToList();
        //}

        //niranjan
        public async Task<List<IStore>> GetChannelPartner(string jobPositionUID, bool isAdmin)
        {
            var param = new Dictionary<string, object>
            {
                { "JobPositionUID", jobPositionUID }
            };
            string sql = @"select s.*, a.branch_uid, a.uid as ShippingAddressUID 
                   from store s
                   inner join Org o on s.uid = o.uid and s.type = 'FR'
                   inner join address a on a.linked_item_uid = s.uid and a.type = 'Shipping' and is_default = 1 ";


            if (!isAdmin)
            {
                sql += @" where a.branch_uid in(
				   select jp.branch_uid from job_position jp 
				   inner join my_team mt on mt.job_position_uid=jp.uid and jp.uid=@JobPositionUID ) ";
            }
            List<IStore> standingProvisionBranches = await ExecuteQueryAsync<IStore>(sql, param);
            return standingProvisionBranches;
        }

        public async Task<bool> CreateApproval(string linkedItemUID, string linkedItemType, ApprovalRequestItem approvalRequestItem)
        {
            try
            {
                IAllApprovalRequest approvalRequest = _serviceProvider.GetRequiredService<IAllApprovalRequest>();
                approvalRequest.LinkedItemType = linkedItemType;
                approvalRequest.LinkedItemUID = linkedItemUID;
                ApprovalApiResponse<ApprovalStatus> approvalRequestCreated = await _approvalEngineHelper.CreateApprovalRequest(approvalRequestItem, approvalRequest);
                return approvalRequestCreated.Success;
            }
            catch (Exception e)
            {
                throw;
            }
        }
        public async Task<bool> UpdateApproval(ApprovalStatusUpdate approvalStatusUpdate)
        {
            return await _approvalEngineHelper.UpdateApprovalStatus(approvalStatusUpdate);
        }
        public async Task<int> InsertSchemeExtendHistory(ISchemeExtendHistory schemeExtendHistory)
        {
            var sql = """
                                INSERT INTO scheme_extend_history (
                    scheme_type, 
                    scheme_uid, 
                    action_type, 
                    old_date, 
                    new_date, 
                    comments, 
                    updated_by_emp_uid, 
                    updated_on
                ) 
                VALUES (
                    @SchemeType, 
                    @SchemeUid, 
                    @ActionType, 
                    @OldDate, 
                    @NewDate, 
                    @Comments, 
                    @UpdatedByEmpUid, 
                    @UpdatedOn
                );
                """;

            return await ExecuteNonQueryAsync(sql, schemeExtendHistory);
        }
        public async Task<List<ISchemeExtendHistory>> GetschemeExtendHistoryBySchemeUID(string schemeUID)
        {
            var param = new Dictionary<string, object>()
         {
             {"SchemeUID",schemeUID}
            };
            var sql = """
                select seh.*,e.name as UpdatedBy from scheme_extend_history seh 
                inner join emp e on e.uid=seh.updated_by_emp_uid
                where seh.scheme_uid=@SchemeUID
                """;

            List<ISchemeExtendHistory> schemeEH = await ExecuteQueryAsync<ISchemeExtendHistory>(sql, param);
            return schemeEH;
        }
        public async Task<int> UpdateSchemeCustomerMappingData(string schemeType, string schemeUID, DateTime newEndDate, bool isActive)
        {
            var sql = """
                                UPDATE scheme_customer_mapping_data SET end_date = @newEndDate, is_active = @isActive 
                WHERE scheme_type = @schemeType AND scheme_uid = @schemeUID;
                """;
            var parameters = new
            {
                newEndDate = newEndDate,
                isActive = isActive,
                schemeType = schemeType,
                schemeUID = schemeUID
            };

            return await ExecuteNonQueryAsync(sql, parameters);
        }

    }
}
