using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Text;
using Winit.Modules.ApprovalEngine.BL.Interfaces;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.Base.DL.DBManager;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common;

namespace Winit.Modules.Scheme.DL.Classes
{
    public class MSSQLSellOutSchemeHeaderDL : SqlServerDBManager, ISellOutSchemeHeaderDL
    {
        private readonly IApprovalEngineHelper _approvalEngineHelper;
        private readonly ISellOutSchemeLineDL _sellOutlineOutSchemeLineDL;
        public MSSQLSellOutSchemeHeaderDL(IApprovalEngineHelper approvalEngineHelper, IServiceProvider serviceProvider, IConfiguration config, ISellOutSchemeLineDL lineOutSchemeLineDL) : base(serviceProvider, config)
        {
            _sellOutlineOutSchemeLineDL = lineOutSchemeLineDL;
            _approvalEngineHelper = approvalEngineHelper;
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
                                                server_modified_time,UID,contribution_level3,code,remarks
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
                                            server_modified_time,UID,contribution_level3,code,remarks
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
                        server_modified_time,UID,contribution_level3,code,remarks
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
                        server_modified_time,UID,contribution_level3,code,remarks
                    ) VALUES (
                        @OrgUid, @FranchiseeOrgUid, @ContributionLevel1, @ContributionLevel2, 
                        @TotalCreditNote, @AvailableProvision2Amount, @AvailableProvision3Amount, 
                        @StandingProvisionAmount, @JobPositionUid, @EmpUid, @LineCount, @Status, 
                        @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, 
                        @ServerModifiedTime,@UID,@ContributionLevel3,@Code,@Remarks);";

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
                        server_modified_time = @ServerModifiedTime,
                        contribution_level3 = @ContributionLevel3,code=@Code,
                        remarks=@Remarks
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

        //public async Task<bool> CreateSeLLOut(ISellOutMasterScheme sellOutMasterScheme)
        //{
        //    try
        //    {
        //        using IDbConnection connection = CreateConnection();
        //        connection.Open();
        //        using IDbTransaction transaction = connection.BeginTransaction();
        //        if (await CreateSellOutHeader(sellOutMasterScheme.SellOutSchemeHeader!, connection, transaction) != 1)
        //        {
        //            transaction.Rollback();
        //            return false;
        //        }
        //        if (await _sellOutlineOutSchemeLineDL.CreateSellOutLines
        //            (sellOutMasterScheme.SellOutSchemeLine!, connection, transaction) != sellOutMasterScheme.SellOutSchemeLine!.Count)
        //        {
        //            transaction.Rollback();
        //            return false;
        //        }

        //        transaction.Commit();
        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        public async Task<bool> CrudSellOutMaster(ISellOutMasterScheme sellOutMasterScheme)
        {
            using IDbConnection connection = CreateConnection();
            connection.Open();
            using IDbTransaction transaction = connection.BeginTransaction();
            try
            {
                var existingSellOutHeader = await GetSellOutSchemeHeaderByUID(sellOutMasterScheme.SellOutSchemeHeader!.UID);

                int headerResult = existingSellOutHeader == null
                    ? await CreateSellOutHeader(sellOutMasterScheme.SellOutSchemeHeader!, connection, transaction)
                    : await UpdateSellOutSchemeHeader(sellOutMasterScheme.SellOutSchemeHeader!, connection, transaction);


                if (headerResult != 1)
                {
                    transaction.Rollback();
                    return false;
                }
                var existingSellOutLines = await _sellOutlineOutSchemeLineDL.GetSellOutSchemeLinesByUIDs(sellOutMasterScheme.SellOutSchemeLines.Select(line => line.UID).ToList());

                var linesToUpdate = sellOutMasterScheme.SellOutSchemeLines!
                    .Where(line => existingSellOutLines.Any(existingLine => existingLine.UID == line.UID))
                    .ToList();
                var linesToCreate = sellOutMasterScheme.SellOutSchemeLines!
                    .Where(line => !existingSellOutLines.Any(existingLine => existingLine.UID == line.UID))
                    .ToList();

                if (linesToUpdate.Any())
                {
                    int updateResult = await _sellOutlineOutSchemeLineDL.UpdateSellOutSchemeLines(linesToUpdate, connection, transaction);
                    if (updateResult != linesToUpdate.Count)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }

                if (linesToCreate.Any())
                {
                    int createResult = await _sellOutlineOutSchemeLineDL.CreateSellOutLines(linesToCreate, connection, transaction);
                    if (createResult != linesToCreate.Count)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
                if (existingSellOutHeader == null)
                {
                    if (await CreateApprovalRequest(sellOutMasterScheme))
                    {
                        int retVal = await UpdateSellOutSchemeHeader(sellOutMasterScheme.SellOutSchemeHeader!, connection, transaction);
                        if (retVal !=1)
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }
                    else
                    {
                        transaction.Rollback();
                        throw new InvalidOperationException("Approval engine create failed.");
                    }
                }
                else
                {
                    if (sellOutMasterScheme.ApprovalStatusUpdate != null)
                    {
                        if (!(await _approvalEngineHelper.UpdateApprovalStatus(sellOutMasterScheme.ApprovalStatusUpdate)))
                        {
                            transaction.Rollback();
                            throw new InvalidOperationException("Approval engine update failed.");
                        }
                    }
                }

                transaction.Commit();
                return true;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
        private async Task<bool> CreateApprovalRequest(ISellOutMasterScheme sellOutMasterScheme)
        {
            try
            {
                IAllApprovalRequest approvalRequest = _serviceProvider.GetRequiredService<IAllApprovalRequest>();
                approvalRequest.LinkedItemType = "Scheme_SellOut";
                approvalRequest.LinkedItemUID =sellOutMasterScheme.SellOutSchemeHeader!.UID;
                ApprovalApiResponse<ApprovalStatus> approvalRequestCreated = await _approvalEngineHelper.CreateApprovalRequest(sellOutMasterScheme.ApprovalRequestItem, approvalRequest);
                return approvalRequestCreated.Success;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<int> CreateSellOutHeader(ISellOutSchemeHeader sellOutSchemeHeader,
       IDbConnection? dbConnection = null, IDbTransaction? dbTransaction = null)
        {
            try
            {
                string sql = """
                        INSERT INTO sell_out_scheme_header (
                            uid, created_by, created_time, modified_by, modified_time, 
                            server_add_time, server_modified_time, ss, org_uid, franchisee_org_uid, 
                            contribution_level1, contribution_level2, total_credit_note, 
                            available_provision2_amount, available_provision3_amount, 
                            standing_provision_amount, job_position_uid, emp_uid, 
                            line_count, status,contribution_level3,code,remarks,is_approval_created
                        ) VALUES (
                            @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, 
                            @ServerAddTime, @ServerModifiedTime, @SS, @OrgUID, @FranchiseeOrgUID, 
                            @ContributionLevel1, @ContributionLevel2, @TotalCreditNote, 
                            @AvailableProvision2Amount, @AvailableProvision3Amount, 
                            @StandingProvisionAmount, @JobPositionUID, @EmpUID, 
                            @LineCount, @Status,@ContributionLevel3,@Code,@Remarks,@IsApprovalCreated 
                        );
            """;

                return await ExecuteNonQueryAsync(sql, dbConnection, dbTransaction, sellOutSchemeHeader);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<int> UpdateSellOutSchemeHeader(ISellOutSchemeHeader sellOutSchemeHeader,
      IDbConnection? dbConnection = null, IDbTransaction? dbTransaction = null)
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
                        server_modified_time = @ServerModifiedTime,
                        contribution_level3 = @ContributionLevel3,
                        code=@Code,
                        remarks=@Remarks,
                        is_approval_created=@IsApprovalCreated 
                    WHERE
                        uid = @Uid;";

            return await ExecuteNonQueryAsync(sql, dbConnection, dbTransaction, sellOutSchemeHeader);
        }
        public async Task<ISellOutMasterScheme> GetSellOutMasterByUID(string UID)
        {
            try
            {
                ISellOutMasterScheme sellOutMasterScheme = _serviceProvider.GetRequiredService<ISellOutMasterScheme>();
                sellOutMasterScheme.SellOutSchemeHeader = await GetSellOutSchemeHeaderByUID(UID);
                sellOutMasterScheme.SellOutSchemeLines = await GetSellOutSchemeLinesByUIDs(UID);

                return sellOutMasterScheme;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<List<ISellOutSchemeLine>> GetSellOutSchemeLinesByUIDs(string headerUID)
        {
            try
            {

                string sql = $@"
            SELECT 
                sell_out_SCHEME_header_uid, line_number, sku_uid, is_deleted, 
                qty, qty_scanned, reason, unit_price, unit_credit_note_amount, 
                total_credit_note_amount, serial_nos, uid, 
                created_by, created_time, modified_by, modified_time, server_add_time, 
                server_modified_time
            FROM 
                sell_out_scheme_line
            WHERE sell_out_SCHEME_header_uid = @UIDs";

                var parameters = new Dictionary<string, object>
                {
                    { "UIDs", headerUID }
                };

                return (await ExecuteQueryAsync<ISellOutSchemeLine>(sql, parameters)).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }


    }
}
