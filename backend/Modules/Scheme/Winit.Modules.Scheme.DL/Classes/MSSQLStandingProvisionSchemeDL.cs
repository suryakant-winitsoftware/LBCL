using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Text;
using Winit.Modules.ApprovalEngine.BL.Interfaces;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.Base.DL.DBManager;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common;

namespace Winit.Modules.Scheme.DL.Classes
{
    public class MSSQLStandingProvisionSchemeDL : SqlServerDBManager, IStandingProvisionSchemeDL
    {
        private readonly IApprovalEngineHelper _approvalEngineHelper;
        public MSSQLStandingProvisionSchemeDL(IApprovalEngineHelper approvalEngineHelper, IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
            _approvalEngineHelper = approvalEngineHelper;
        }
        public async Task<PagedResponse<IStandingProvisionScheme>> SelectAllStandingConfiguration(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (SELECT 
                                                        sps.org_uid orguid, sps.job_position_uid jobpositionuid, sps.sku_category_code SkuCategoryCode,
														sps.sku_type_code SkuTypeCode, sps.star_rating_code StarRatingCode, 
                                                        sps.sku_tonnage_code SkuTonnageCode, sps.start_date StartDate, sps.end_date EndDate, sps.Amount, 
                                                        sps.description, sps.excluded_models ExcludedModels,  sps.uid, 
                                                        sps.created_by CreatedBy, sps.created_time CreatedTime, sps.modified_by ModifiedBy, 
														sps.modified_time ModifiedTime, sps.server_add_time ServerAddTime,  sps.server_modified_time ServerModifiedTime,
														sps.star_capacity_code StarCapacityCode,sps.sku_series_code SkuSeriesCode,emp.name CreatedByEmpName,sps.code,sps.sku_product_group SKUProductGroup,
                                                        sps.status,sps.has_history as HasHistory
                                                    FROM 
                                                        standing_provision_scheme sps
														left join emp on emp.uid=sps.created_by
														
                                                            )As SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM 
                                            (SELECT 
                                                        sps.org_uid orguid, sps.job_position_uid jobpositionuid, sps.sku_category_code SkuCategoryCode,
														sps.sku_type_code SkuTypeCode, sps.star_rating_code StarRatingCode, 
                                                        sps.sku_tonnage_code SkuTonnageCode, sps.start_date StartDate, sps.end_date EndDate, sps.Amount, 
                                                        sps.description, sps.excluded_models ExcludedModels,  sps.uid, 
                                                        sps.created_by CreatedBy, sps.created_time CreatedTime, sps.modified_by ModifiedBy, 
														sps.modified_time ModifiedTime, sps.server_add_time ServerAddTime,  sps.server_modified_time ServerModifiedTime,
														sps.star_capacity_code StarCapacityCode,sps.sku_series_code SkuSeriesCode,emp.name CreatedByEmpName,sps.code,sps.sku_product_group SKUProductGroup, sps.status,
                                                        sps.has_history as HasHistory
                                                    FROM 
                                                        standing_provision_scheme sps
														left join emp on emp.uid=sps.created_by
														
                                                            )As SubQuery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<IStandingProvisionScheme>(filterCriterias, sbFilterCriteria, parameters);
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
                IEnumerable<IStandingProvisionScheme> AwayPeriodDetails = await ExecuteQueryAsync<IStandingProvisionScheme>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<IStandingProvisionScheme> pagedResponse = new PagedResponse<IStandingProvisionScheme>
                {
                    PagedData = AwayPeriodDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<IStandingProvisionSchemeMaster> GetStandingConfigurationMasterByUID(string UID)
        {
            IStandingProvisionSchemeMaster standingProvisionSchemeMaster = new StandingProvisionSchemeMaster();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "UID", UID }
                };

                var sqlStandingProvisionScheme = @"SELECT 
                                sps.org_uid, sps.job_position_uid, sps.sku_category_code, sps.sku_type_code, sps.star_rating_code, 
                                sps.sku_tonnage_code,  sps.start_date, sps.end_date, sps.amount, 
                                sps.description, sps.excluded_models,  sps.uid, 
                                sps.created_by, sps.created_time, sps.modified_by, sps.modified_time, sps.server_add_time, 
                                sps.server_modified_time,e.name as CreatedByEmpName,sps.star_capacity_code,sps.sku_series_code,sps.code,sps.sku_product_group SKUProductGroup,sps.status
                            FROM 
                                standing_provision_scheme sps
								inner join emp e on e.uid=sps.created_by
                              WHERE sps.UID = @UID";

                var sqlStandingProvisionSchemeBranches = @"SELECT 
                                    id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, ss, standing_provision_scheme_uid, branch_code
                                FROM 
                                    standing_provision_scheme_branch where standing_provision_scheme_uid=@UID";

                var sqlBroadClassifications = @"select  uid, created_by, created_time, modified_by, modified_time, server_add_time,
														server_modified_time, ss, standing_provision_uid, broad_classification_code from standing_provision_scheme_broad_classification
                                         where standing_provision_uid=@UID";
                var sqlApplicableOrgs = @"select  uid, created_by, created_time, modified_by, modified_time, server_add_time,
														server_modified_time, ss, standing_provision_uid, org_uid from standing_provision_scheme_applicable_org where standing_provision_uid=@UID";
                var sqlSchemeDivisions = @"SELECT 
                                    id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, ss, standing_provision_uid, division_org_uid
                                FROM 
                                    standing_provision_scheme_division where standing_provision_uid=@UID";
                standingProvisionSchemeMaster.StandingProvisionScheme = await ExecuteSingleAsync<IStandingProvisionScheme>(sqlStandingProvisionScheme, parameters);

                //standingProvisionSchemeMaster.StandingProvisionSchemeBranches = await ExecuteQueryAsync<IStandingProvisionSchemeBranch>(sqlStandingProvisionSchemeBranches, parameters);
                //standingProvisionSchemeMaster.StandingProvisionSchemeBroadClassifications = await ExecuteQueryAsync<IStandingProvisionSchemeBroadClassification>(sqlBroadClassifications, parameters);
                //standingProvisionSchemeMaster.StandingProvisionSchemeApplicableOrgs = await ExecuteQueryAsync<IStandingProvisionSchemeApplicableOrg>(sqlApplicableOrgs, parameters);
                standingProvisionSchemeMaster.StandingProvisionSchemeDivisions = await ExecuteQueryAsync<IStandingProvisionSchemeDivision>(sqlSchemeDivisions, parameters);

                return standingProvisionSchemeMaster;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<IStandingProvisionScheme> GetExistingStandingConfigurationMasterByUID(string UID, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {

            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "UID", UID }
                };

                var sql = @"SELECT UID
                            FROM 
                                standing_provision_scheme
                              WHERE UID = @UID";


                IStandingProvisionScheme standingProvisionScheme = await ExecuteSingleAsync<IStandingProvisionScheme>(sql, parameters, null, connection, transaction);

                return standingProvisionScheme;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> CUStandingProvisionScheme(IStandingProvisionSchemeMaster standingProvisionSchemeMaster)
        {
            int cnt = 0;
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();
                var transaction = connection.BeginTransaction();
                try
                {
                    var exRecord = await GetExistingStandingConfigurationMasterByUID(standingProvisionSchemeMaster.StandingProvisionScheme.UID);


                    cnt = exRecord == null ? await CreateStandingConfiguration(standingProvisionSchemeMaster.StandingProvisionScheme, connection: connection, transaction: transaction)
                       : await UpdateStandingConfiguration(standingProvisionSchemeMaster.StandingProvisionScheme, connection: connection, transaction: transaction);
                    if (cnt > 0 && standingProvisionSchemeMaster.StandingProvisionSchemeDivisions != null && standingProvisionSchemeMaster.StandingProvisionSchemeDivisions.Count > 0)
                    {
                        List<string>? exUIds = await CheckIfUIDExistsInDB(DbTableName.StandingProvisionSchemeBroadDivision, standingProvisionSchemeMaster.StandingProvisionSchemeDivisions.Select(p => p.UID).ToList());
                        if (exUIds != null && exUIds.Count > 0)
                        {
                            foreach (var item in standingProvisionSchemeMaster.StandingProvisionSchemeDivisions)
                            {
                                bool isExt = exUIds.Contains(item.UID);
                                cnt += isExt ? await UpdateStandingProvisionDivision(item, connection: connection, transaction: transaction) :
                                    await CreateStandingProvisionDivision(item, connection: connection, transaction: transaction);
                            }
                        }
                        else
                        {
                            cnt += await CreateStandingProvisionDivision(standingProvisionSchemeMaster.StandingProvisionSchemeDivisions, connection: connection, transaction: transaction);
                        }
                    }
                    //if (exRecord == null)
                    //{
                    //    if (await CreateApprovalRequest(standingProvisionSchemeMaster))
                    //    {
                    //        standingProvisionSchemeMaster.StandingProvisionScheme.IsApprovalCreated = true;
                    //        cnt += await UpdateStandingConfiguration(standingProvisionSchemeMaster.StandingProvisionScheme, connection: connection, transaction: transaction);
                    //    }
                    //    else
                    //    {
                    //        throw new InvalidOperationException("Approval engine create failed.");
                    //    }
                    //}
                    //else if (standingProvisionSchemeMaster.ApprovalStatusUpdate != null)
                    //{
                    //    if (!(await _approvalEngineHelper.UpdateApprovalStatus(standingProvisionSchemeMaster.ApprovalStatusUpdate)))
                    //    {
                    //        throw new InvalidOperationException("Approval engine update failed.");
                    //    }
                    //}


                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            return cnt;
        }
        private async Task<bool> CreateApprovalRequest(IStandingProvisionSchemeMaster standingProvisionSchemeMaster)
        {
            try
            {
                IAllApprovalRequest approvalRequest = _serviceProvider.GetRequiredService<IAllApprovalRequest>();
                approvalRequest.LinkedItemType = "StandingProvision";
                approvalRequest.LinkedItemUID = standingProvisionSchemeMaster.StandingProvisionScheme.UID;
                ApprovalApiResponse<ApprovalStatus> approvalRequestCreated = await _approvalEngineHelper.CreateApprovalRequest(standingProvisionSchemeMaster.ApprovalRequestItem, approvalRequest);
                return approvalRequestCreated.Success;
            }
            catch (Exception e)
            {
                throw;
            }
        }
        private async Task<int> CreateStandingConfiguration(IStandingProvisionScheme standingConfiguration, IDbConnection connection, IDbTransaction transaction)
        {
            try
            {
                var sql = @"
                        INSERT INTO standing_provision_scheme (
                                        org_uid, job_position_uid, sku_category_code, sku_type_code, star_rating_code, 
                                        sku_tonnage_code,  start_date, end_date, amount, 
                                        description, excluded_models, uid, 
                                        created_by, created_time, modified_by, modified_time, server_add_time, 
                                        server_modified_time,star_capacity_code,sku_series_code,code,sku_product_group,is_approval_created,status
                                    ) VALUES (
                                        @OrgUid, @JobPositionUid, @SkuCategoryCode, @SkuTypeCode, @StarRatingCode, 
                                        @SkuTonnageCode,  @StartDate, @EndDate, @Amount, 
                                        @Description, @ExcludedModels, @Uid, 
                                        @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, 
                                        @ServerModifiedTime,@StarCapacityCode,@SKUSeriesCode,@Code,@SKUProductGroup,@IsApprovalCreated,@Status
                                    );";

                return await ExecuteNonQueryAsync(sql, parameters: standingConfiguration, connection: connection, transaction: transaction);
            }
            catch
            {
                throw;
            }
        }
        private async Task<int> CreateStandingProvisionBranch(object standingProvisionBranch, IDbConnection connection, IDbTransaction transaction)
        {
            try
            {
                var sql = @"
                INSERT INTO standing_provision_scheme_branch (
                   uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, ss, standing_provision_scheme_uid, branch_code
                ) VALUES (
                     @Uid, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @Ss, @StandingProvisionSchemeUID, @BranchCode);";

                return await ExecuteNonQueryAsync(sql, parameters: standingProvisionBranch, connection: connection, transaction: transaction);
            }
            catch
            {
                throw;
            }
        }
        private async Task<int> CreateStandingProvisionDivision(object standingProvisionDivisionOrg, IDbConnection connection, IDbTransaction transaction)
        {
            try
            {
                var sql = @"
                INSERT INTO standing_provision_scheme_division (
                   uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, ss, standing_provision_uid, division_org_uid
                ) VALUES (
                     @Uid, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @Ss, @StandingProvisionUID, @DivisionOrgUID);";

                return await ExecuteNonQueryAsync(sql, parameters: standingProvisionDivisionOrg, connection: connection, transaction: transaction);
            }
            catch
            {
                throw;
            }
        }
        private async Task<int> CreateStandingProvisionBroadClassification(object standingProvisionBC, IDbConnection connection, IDbTransaction transaction)
        {
            try
            {
                var sql = @"
                INSERT INTO standing_provision_scheme_broad_classification (
                  uid, created_by, created_time, modified_by, modified_time, server_add_time,
														server_modified_time, ss, standing_provision_uid, broad_classification_code ) VALUES (
                     @Uid, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @Ss, @StandingProvisionUID, @BroadClassificationCode);"
                ;

                return await ExecuteNonQueryAsync(sql, parameters: standingProvisionBC, connection: connection, transaction: transaction);
            }
            catch
            {
                throw;
            }
        }
        private async Task<int> CreateStandingProvisionApplicableOrg(object standingProvisionAO, IDbConnection connection, IDbTransaction transaction)
        {
            try
            {
                var sql = @"
                INSERT INTO standing_provision_scheme_applicable_org (
                   uid, created_by, created_time, modified_by, modified_time, server_add_time,
														server_modified_time, ss, standing_provision_uid, org_uid ) VALUES (
                     @Uid, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @Ss, @StandingProvisionSchemeUID, @OrgUID);";

                return await ExecuteNonQueryAsync(sql, parameters: standingProvisionAO, connection: connection, transaction: transaction);
            }
            catch
            {
                throw;
            }
        }
        protected async Task<int> UpdateStandingConfiguration(IStandingProvisionScheme awayPeriod, IDbConnection connection = null, IDbTransaction transaction = null)
        {
            var sql = @"
                       UPDATE standing_provision_scheme
                            SET
                                org_uid = @OrgUid,
                                job_position_uid = @JobPositionUid,
                                sku_category_code = @SkuCategoryCode,
                                sku_type_code = @SkuTypeCode,
                                star_rating_code = @StarRatingCode,
                                sku_tonnage_code = @SkuTonnageCode,
                                
                                start_date = @StartDate,
                                end_date = @EndDate,
                                amount = @Amount,
                                description = @Description,
                                excluded_models = @ExcludedModels,
                                created_by = @CreatedBy,
                                created_time = @CreatedTime,
                                modified_by = @ModifiedBy,
                                modified_time = @ModifiedTime,
                                server_add_time = @ServerAddTime,
                                server_modified_time = @ServerModifiedTime,
                                star_capacity_code = @StarCapacityCode,
                                sku_series_code = @SKUSeriesCode,
                                code=@Code,
                                sku_product_group=@SKUProductGroup,
                                status=@Status,
                                is_approval_created=@IsApprovalCreated
                
                            WHERE
                                uid = @Uid;";

            return await ExecuteNonQueryAsync(sql, parameters: awayPeriod, connection: connection, transaction: transaction);
        }
        public async Task<int> UpdateStandingConfiguration(IStandingProvisionScheme standingProvision)
        {
            var sql = @"
                       UPDATE standing_provision_scheme
                            SET
                                org_uid = @OrgUid,
                                job_position_uid = @JobPositionUid,
                                sku_category_code = @SkuCategoryCode,
                                sku_type_code = @SkuTypeCode,
                                star_rating_code = @StarRatingCode,
                                sku_tonnage_code = @SkuTonnageCode,
                                
                                start_date = @StartDate,
                                end_date = @EndDate,
                                amount = @Amount,
                                description = @Description,
                                excluded_models = @ExcludedModels,
                                created_by = @CreatedBy,
                                created_time = @CreatedTime,
                                modified_by = @ModifiedBy,
                                modified_time = @ModifiedTime,
                                server_add_time = @ServerAddTime,
                                server_modified_time = @ServerModifiedTime,
                                star_capacity_code = @StarCapacityCode,
                                sku_series_code = @SKUSeriesCode,
                                code=@Code,
                                sku_product_group=@SKUProductGroup,
                                status=@Status,
                                is_approval_created=@IsApprovalCreated
                
                            WHERE
                                uid = @Uid;";

            return await ExecuteNonQueryAsync(sql, parameters: standingProvision);
        }

        private async Task<int> UpdateStandingProvisionDivision(IStandingProvisionSchemeDivision standingProvisionDivision, IDbConnection connection, IDbTransaction transaction)
        {
            var sql = @"
            UPDATE standing_provision_scheme_division
                SET
                    uid = @Uid,
                    created_by = @CreatedBy,
                    created_time = @CreatedTime,
                    modified_by = @ModifiedBy,
                    modified_time = @ModifiedTime,
                    server_add_time = @ServerAddTime,
                    server_modified_time = @ServerModifiedTime,
                    ss = @Ss,
                    standing_provision_uid = @StandingProvisionUID,
                    division_org_uid = @DivisionOrgUID
                WHERE
                    uid = @Uid;"
            ;

            return await ExecuteNonQueryAsync(sql, parameters: standingProvisionDivision, connection: connection, transaction: transaction);
        }
        private async Task<int> UpdateStandingProvisionBroadClassification(IStandingProvisionSchemeBroadClassification standingProvisionBranch, IDbConnection connection, IDbTransaction transaction)
        {
            var sql = @"
            UPDATE standing_provision_scheme_broad_classification
                SET
                    uid = @Uid,
                    created_by = @CreatedBy,
                    created_time = @CreatedTime,
                    modified_by = @ModifiedBy,
                    modified_time = @ModifiedTime,
                    server_add_time = @ServerAddTime,
                    server_modified_time = @ServerModifiedTime,
                    ss = @Ss,
                    standing_provision_uid=@StandingProvisionUID,
                    broad_classification_code= @BroadClassificationCode
                WHERE
                    uid = @Uid;";

            return await ExecuteNonQueryAsync(sql, parameters: standingProvisionBranch, connection: connection, transaction: transaction);
        }
        private async Task<int> UpdateStandingProvisionApplicableOrg(IStandingProvisionSchemeApplicableOrg standingProvisionApplicableOrg, IDbConnection connection, IDbTransaction transaction)
        {
            var sql = @"
            UPDATE standing_provision_scheme_applicable_org
                SET
                    uid = @Uid,
                    created_by = @CreatedBy,
                    created_time = @CreatedTime,
                    modified_by = @ModifiedBy,
                    modified_time = @ModifiedTime,
                    server_add_time = @ServerAddTime,
                    server_modified_time = @ServerModifiedTime,
                    ss = @Ss,
                   standing_provision_uid=@StandingProvisionSchemeUID, 
                   org_uid=@OrgUID
                WHERE
                    uid = @Uid;";

            return await ExecuteNonQueryAsync(sql, parameters: standingProvisionApplicableOrg, connection: connection, transaction: transaction);
        }
        public async Task<int> DeleteStandingConfiguration(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"DELETE  FROM standing_provision_scheme WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        public async Task<Dictionary<string, StandingSchemeResponse>> GetStandingSchemesByOrgUidAndSKUUid(string orgUid, DateTime orderDate, List<SKUFilter> skuFilterList)
        {
            try
            {
                List<IStandingSchemePO> standingSchemePOsApplicable = await GetApplicableStandingSchemes(orgUid, orderDate);
                if (standingSchemePOsApplicable == null)
                {
                    return null;
                }
                return GetMatchingProductsForSkus(skuFilterList, standingSchemePOsApplicable);
            }
            catch (Exception e)
            {
                throw;
            }
        }
        public async Task<Dictionary<string, StandingSchemeResponse>> GetStandingSchemesByPOUid(string POUid, List<SKUFilter> skuFilterList)
        {
            try
            {
                List<IStandingSchemePO> standingSchemePOsApplicable = await GetStandingSchemesByPOUid(POUid);
                if (standingSchemePOsApplicable == null)
                {
                    return null;
                }
                return GetMatchingProductsForSkus(skuFilterList, standingSchemePOsApplicable);
            }
            catch (Exception e)
            {
                throw;
            }
        }
        private async Task<List<IStandingSchemePO>> GetApplicableStandingSchemes(string orgUid, DateTime orderDate)
        {
            try
            {
                StringBuilder sql = new StringBuilder(
                """
                SELECT sp.uid AS StandingProvisionUID,sp.code AS StandingProvisionCode, sp.sku_category_code AS SkuCategoryCode, 
                sp.sku_type_code AS SkuTypeCode, sp.star_rating_code AS StarRatingCode, 
                sp.sku_tonnage_code AS SkuTonnageCode, sp.star_capacity_code AS StarCapacityCode, 
                sp.sku_series_code AS SkuSeriesCode, sp.sku_product_group AS SkuProductGroup, 
                sp.excluded_models AS ExcludedModelCommaSeparated, sp.amount AS Amount, 
                STRING_AGG(SPD.division_org_uid, ',') AS DivisionOrgUIDsCommaSeparated
                FROM standing_provision_scheme SP
                INNER JOIN scheme_customer_mapping_data SCMD ON SCMD.scheme_type = 'standing'and sp.uid=SCMD.scheme_uid
                AND SCMD.store_uid = @OrgUID
                AND DATEDIFF(dd, SCMD.approved_time, @OrderDate) >= 0 
                AND DATEDIFF(dd, @OrderDate, SCMD.end_date) >= 0
                INNER JOIN standing_provision_scheme_division SPD ON SPD.standing_provision_uid = SP.uid
                GROUP BY sp.uid, sp.code, sp.sku_category_code, sp.sku_type_code, sp.star_rating_code, 
                         sp.sku_tonnage_code, sp.star_capacity_code, sp.sku_series_code, 
                         sp.sku_product_group, sp.excluded_models, sp.amount
                """);
                var parameters = new
                {
                    OrgUid = orgUid,
                    OrderDate = orderDate
                };
                var results = await ExecuteQueryAsync<IStandingSchemePO>(sql.ToString(), parameters);
                foreach (var item in results)
                {
                    // Populate ExcludedModels from ExcludedModelCommaSeparated
                    item.ExcludedModels = !string.IsNullOrEmpty(item.ExcludedModelCommaSeparated)
                        ? new HashSet<string>(item.ExcludedModelCommaSeparated.Split(',').Select(model => model.Trim()))
                        : new HashSet<string>();

                    // Populate Division from ExcludedModelCommaSeparated
                    item.DivisionOrgUIDs = !string.IsNullOrEmpty(item.DivisionOrgUIDsCommaSeparated)
                        ? new HashSet<string>(item.DivisionOrgUIDsCommaSeparated.Split(',').Select(div => div.Trim()))
                        : new HashSet<string>();
                }
                return results;
            }
            catch (Exception e)
            {
                throw;
            }
        }
        private async Task<List<IStandingSchemePO>> GetStandingSchemesByPOUid(string pOUid)
        {
            try
            {
                StringBuilder sql = new StringBuilder(
                """
                with Schemes as(
                select  distinct ph.uid, polp.scheme_code,pl.sku_uid,polp.provision_type,ph.org_uid from purchase_order_header ph 
                inner join purchase_order_line pl on ph.uid=pl.purchase_order_header_uid
                inner join purchase_order_line_provision polp on polp.purchase_order_line_uid=pl.uid 
                and polp.provision_type like '%StandingScheme%' 
                and ph.uid=@POUid
                )
                --select * from Schemes
                  SELECT sp.uid AS StandingProvisionUID,sp.code AS StandingProvisionCode, sp.sku_category_code AS SkuCategoryCode, 
                 sp.sku_type_code AS SkuTypeCode, sp.star_rating_code AS StarRatingCode, 
                 sp.sku_tonnage_code AS SkuTonnageCode, sp.star_capacity_code AS StarCapacityCode, 
                 sp.sku_series_code AS SkuSeriesCode, sp.sku_product_group AS SkuProductGroup, 
                 sp.excluded_models AS ExcludedModelCommaSeparated, sp.amount AS Amount, 
                (select STRING_AGG(division_org_uid,',' )from standing_provision_scheme_division where standing_provision_uid = SP.uid)  AS DivisionOrgUIDsCommaSeparated
                 FROM standing_provision_scheme SP
                 INNER JOIN scheme_customer_mapping_data SCMD ON SCMD.scheme_type = 'standing'and sp.uid=SCMD.scheme_uid
                 INNER JOIN standing_provision_scheme_division SPD ON SPD.standing_provision_uid = SP.uid
                 inner join Schemes S on S.scheme_code= SP.code
                 GROUP BY sp.uid, sp.code, sp.sku_category_code, sp.sku_type_code, sp.star_rating_code, 
                          sp.sku_tonnage_code, sp.star_capacity_code, sp.sku_series_code, 
                          sp.sku_product_group, sp.excluded_models, sp.amount
                """);
                var parameters = new
                {
                    POUid = pOUid,
                   
                };
                var results = await ExecuteQueryAsync<IStandingSchemePO>(sql.ToString(), parameters);
                foreach (var item in results)
                {
                    // Populate ExcludedModels from ExcludedModelCommaSeparated
                    item.ExcludedModels = !string.IsNullOrEmpty(item.ExcludedModelCommaSeparated)
                        ? new HashSet<string>(item.ExcludedModelCommaSeparated.Split(',').Select(model => model.Trim()))
                        : new HashSet<string>();

                    // Populate Division from ExcludedModelCommaSeparated
                    item.DivisionOrgUIDs = !string.IsNullOrEmpty(item.DivisionOrgUIDsCommaSeparated)
                        ? new HashSet<string>(item.DivisionOrgUIDsCommaSeparated.Split(',').Select(div => div.Trim()))
                        : new HashSet<string>();
                }
                return results;
            }
            catch (Exception e)
            {
                throw;
            }
        }
        private Dictionary<string, StandingSchemeResponse> GetMatchingProductsForSkus(List<SKUFilter> skuList, List<IStandingSchemePO> standingDtoList)
        {
            // Dictionary to store the results: SKU -> List of matching StandingDTOs
            var result = new Dictionary<string, StandingSchemeResponse>();

            foreach (var sku in skuList)
            {
                var matchingStandingDtos = new List<IStandingSchemeDTO>();

                foreach (var standingDto in standingDtoList)
                {
                    // Exclude if the SKU matches any excluded model in StandingDTO
                    if (standingDto.ExcludedModels.Contains(sku.SKUUID))
                        continue;

                    // Check if the SKU's DivisionUID matches any DivisionOrgUID in StandingDTO
                    if (!standingDto.DivisionOrgUIDs.Contains(sku.DivisionUID))
                        continue;

                    // Parse SKU attributes from FilterKeys
                    var skuAttributes = sku.FilterKeys
                        .Select(key => key.Split('_'))
                        .Where(parts => parts.Length == 2)
                        .ToDictionary(parts => parts[1], parts => parts[0]);

                    // Check if any attribute matches [For Or Condition]
                    /*
                    bool isMatch = (skuAttributes.TryGetValue("Category", out var category) && category == standingDto.SkuCategoryCode) ||
                                   (skuAttributes.TryGetValue("Product Type", out var type) && type == standingDto.SkuTypeCode) ||
                                   (skuAttributes.TryGetValue("Star Rating", out var rating) && rating == standingDto.StarRatingCode) ||
                                   (skuAttributes.TryGetValue("TONAGE", out var tonnage) && tonnage == standingDto.SkuTonnageCode) ||
                                   (skuAttributes.TryGetValue("Capacity", out var capacity) && capacity == standingDto.StarCapacityCode) ||
                                   (skuAttributes.TryGetValue("Item Series", out var series) && series == standingDto.SkuSeriesCode) ||
                                   (skuAttributes.TryGetValue("PG", out var productGroup) && productGroup == standingDto.SkuProductGroup);
                    */
                    // Check if any attribute matches [For AND Condition]
                    bool isMatch =
                        (standingDto.SkuCategoryCode == null ||
                         (skuAttributes.TryGetValue("Category", out var category) && category == standingDto.SkuCategoryCode)) &&
                        (standingDto.SkuTypeCode == null ||
                         (skuAttributes.TryGetValue("Product Type", out var type) && type == standingDto.SkuTypeCode)) &&
                        (standingDto.StarRatingCode == null ||
                         (skuAttributes.TryGetValue("Star Rating", out var rating) && rating == standingDto.StarRatingCode)) &&
                        (standingDto.SkuTonnageCode == null ||
                         (skuAttributes.TryGetValue("TONAGE", out var tonnage) && tonnage == standingDto.SkuTonnageCode)) &&
                        (standingDto.StarCapacityCode == null ||
                         (skuAttributes.TryGetValue("Capacity", out var capacity) && capacity == standingDto.StarCapacityCode)) &&
                        (standingDto.SkuSeriesCode == null ||
                         (skuAttributes.TryGetValue("Item Series", out var series) && series == standingDto.SkuSeriesCode)) &&
                        (standingDto.SkuProductGroup == null ||
                         (skuAttributes.TryGetValue("PG", out var productGroup) && productGroup == standingDto.SkuProductGroup));


                    // Add to matching list if there's a match
                    if (isMatch)
                    {
                        matchingStandingDtos.Add(
                            new StandingSchemeDTO
                            {
                                SchemeUID = standingDto.StandingProvisionUID,
                                SchemeCode = standingDto.StandingProvisionCode,
                                Amount = standingDto.Amount
                            }
                            );
                    }
                }

                // Add the SKU and its matching StandingDTOs to the result
                result[sku.SKUUID] = new StandingSchemeResponse
                {
                    TotalAmount = matchingStandingDtos.Sum(e => e.Amount),
                    Schemes = matchingStandingDtos
                };
            }

            return result;
        }
        public async Task<int> UpdateSchemeMappingData(string schemeUID)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@scheme_uid", schemeUID);
                return await ExecuteProcedureAsync("usp_scheme_customer_mapping_data_insert_for_standing", parameters);
            }
            catch
            {
                throw;
            }
        }
        public async Task<int> ChangeEndDate(IStandingProvisionScheme standingProvisionScheme)
        {
            string sql = """
                update standing_provision_scheme set has_history=@HasHistory, end_date=@EndDate,
                end_date_remarks=@EndDateRemarks,
                end_date_updated_by_emp_uid=@EndDateUpdatedByEmpUID,
                end_date_updated_on=@EndDateUpdatedOn,
                modified_by=@ModifiedBy,
                modified_time=@ModifiedTime,  
                server_modified_time=@ServerModifiedTime
                where uid=@UID
                """;

            int count = await ExecuteNonQueryAsync(sql, standingProvisionScheme);
            return count;
        }
    }
}
