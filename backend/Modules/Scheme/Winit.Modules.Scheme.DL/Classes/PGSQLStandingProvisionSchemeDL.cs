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
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.DL.Classes
{
    public class PGSQLStandingProvisionSchemeDL : PostgresDBManager, IStandingProvisionSchemeDL
    {
        public PGSQLStandingProvisionSchemeDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<IStandingProvisionScheme>> SelectAllStandingConfiguration(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (SELECT 
                                                        org_uid, job_position_uid, sku_category_code, sku_type_code, star_rating_code, 
                                                        sku_tonnage_code, broad_classification_code, start_date, end_date, amount, 
                                                        description, excluded_models,  uid, 
                                                        created_by, created_time, modified_by, modified_time, server_add_time, 
                                                        server_modified_time
                                                    FROM 
                                                        standing_provision_scheme
                                                            )As SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM 
                                            (SELECT 
                                                    org_uid, job_position_uid, sku_category_code, sku_type_code, star_rating_code, 
                                                    sku_tonnage_code, broad_classification_code, start_date, end_date, amount, 
                                                    description, excluded_models,  uid, 
                                                    created_by, created_time, modified_by, modified_time, server_add_time, 
                                                    server_modified_time
                                                FROM 
                                                    standing_provision_scheme
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
            IStandingProvisionSchemeMaster standingProvisionSchemeMaster=new StandingProvisionSchemeMaster();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "UID", UID }
                };

                var sql = @"SELECT 
                                org_uid, job_position_uid, sku_category_code, sku_type_code, star_rating_code, 
                                sku_tonnage_code, broad_classification_code, start_date, end_date, amount, 
                                description, excluded_models,  uid, 
                                created_by, created_time, modified_by, modified_time, server_add_time, 
                                server_modified_time
                            FROM 
                                standing_provision_scheme
                              WHERE UID = @UID";
                var sql2 = @"SELECT 
                                    id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, ss, standing_provision_scheme_uid, branch_uid
                                FROM 
                                    public.standing_provision_scheme_branch where standing_provision_scheme_uid=@UID";

                standingProvisionSchemeMaster.StandingProvisionScheme= await ExecuteSingleAsync<IStandingProvisionScheme>(sql, parameters);
                //standingProvisionSchemeMaster.StandingProvisionSchemeBranches = await ExecuteQueryAsync<IStandingProvisionSchemeBranch>(sql2.ToString(), parameters);

                return standingProvisionSchemeMaster;

            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<IStandingProvisionSchemeMaster> GetExistingStandingConfigurationMasterByUID(string UID)
        {
            IStandingProvisionSchemeMaster standingProvisionSchemeMaster=new StandingProvisionSchemeMaster();
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
                var sql2 = @"SELECT 
                                     uid
                                FROM 
                                    public.standing_provision_scheme_branch where standing_provision_scheme_uid=@UID";

                standingProvisionSchemeMaster.StandingProvisionScheme= await ExecuteSingleAsync<IStandingProvisionScheme>(sql, parameters);
                //standingProvisionSchemeMaster.StandingProvisionSchemeBranches = await ExecuteQueryAsync<IStandingProvisionSchemeBranch>(sql2.ToString(), parameters);

                return standingProvisionSchemeMaster;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> CUStandingProvisionScheme(IStandingProvisionSchemeMaster standingProvisionSchemeMaster)
        {
            var exRecord = await GetExistingStandingConfigurationMasterByUID(standingProvisionSchemeMaster?.StandingProvisionScheme?.UID);

            int cnt= exRecord.StandingProvisionScheme==null?await CreateStandingConfiguration(standingProvisionSchemeMaster.StandingProvisionScheme)
                :await UpdateStandingConfiguration(standingProvisionSchemeMaster.StandingProvisionScheme);

            // if(cnt>0&&standingProvisionSchemeMaster.StandingProvisionSchemeBranches!=null&& standingProvisionSchemeMaster.StandingProvisionSchemeBranches.Count > 0)
            //{
            //    foreach(var item in standingProvisionSchemeMaster.StandingProvisionSchemeBranches)
            //    {
            //        bool isExt = exRecord.StandingProvisionSchemeBranches.Any(p => p.UID == item.UID);
            //        cnt += isExt ? await  UpdateStandingProvisionBranch(item):await CreateStandingProvisionBranch(item);
            //    }
            //}
             
                                                     
            return cnt;
        }
        private async Task<int> CreateStandingConfiguration(IStandingProvisionScheme standingConfiguration)
        {
            try
            {
                var sql = @"
                        INSERT INTO standing_provision_scheme (
                                        org_uid, job_position_uid, sku_category_code, sku_type_code, star_rating_code, 
                                        sku_tonnage_code, broad_classification_code, start_date, end_date, amount, 
                                        description, excluded_models, uid, 
                                        created_by, created_time, modified_by, modified_time, server_add_time, 
                                        server_modified_time
                                    ) VALUES (
                                        @OrgUid, @JobPositionUid, @SkuCategoryCode, @SkuTypeCode, @StarRatingCode, 
                                        @SkuTonnageCode, @BroadClassificationCode, @StartDate, @EndDate, @Amount, 
                                        @Description, @ExcludedModels::json, @Uid, 
                                        @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, 
                                        @ServerModifiedTime
                                    );";

                return await ExecuteNonQueryAsync(sql, standingConfiguration);
            }
            catch
            {
                throw;
            }
        }
        private async Task<int> CreateStandingProvisionBranch(IStandingProvisionSchemeBranch standingProvisionBranch)
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
        public async Task<int> UpdateStandingConfiguration(IStandingProvisionScheme awayPeriod)
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
                                broad_classification_code = @BroadClassificationCode,
                                start_date = @StartDate,
                                end_date = @EndDate,
                                amount = @Amount,
                                description = @Description,
                                excluded_models = @ExcludedModels::json,
                                created_by = @CreatedBy,
                                created_time = @CreatedTime,
                                modified_by = @ModifiedBy,
                                modified_time = @ModifiedTime,
                                server_add_time = @ServerAddTime,
                                server_modified_time = @ServerModifiedTime
                            WHERE
                                uid = @Uid;";

            return await ExecuteNonQueryAsync(sql, awayPeriod);
        }
        private async Task<int> UpdateStandingProvisionBranch(IStandingProvisionSchemeBranch standingProvisionBranch)
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
        public async Task<int> DeleteStandingConfiguration(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"DELETE  FROM standing_provision_scheme WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        public Task<Dictionary<string, StandingSchemeResponse>> GetStandingSchemesByOrgUidAndSKUUid(string orgUid, DateTime orderDate, List<SKUFilter> skuFilterList)
        {
            throw new NotImplementedException();
        }
    }
}
