using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Winit.Modules.ApprovalEngine.BL.Interfaces;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.Base.DL.DBManager;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common;
namespace Winit.Modules.Scheme.DL.Classes
{
    public class MSSQLSellInSchemeDL : SqlServerDBManager, ISellInSchemeDL
    {
        private readonly IApprovalEngineHelper _approvalEngineHelper;
        public MSSQLSellInSchemeDL(IApprovalEngineHelper approvalEngineHelper, IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
            _approvalEngineHelper = approvalEngineHelper;
        }
        public async Task<PagedResponse<ISellInSchemeHeader>> SelectAllSellInHeader(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (SELECT 
                                        org_uid,  max_deal_count, request_type, valid_from, 
                                        end_date, valid_up_to, available_provision2_amount , available_provision3_amount , 
                                        standing_provision_amount, job_position_uid, emp_uid, line_count, status, 
                                        uid, created_by, created_time, modified_by, modified_time, 
                                        server_add_time, server_modified_time,Code,is_till_month_end
                                    FROM 
                                        sell_in_scheme_header
                                                    )As SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM 
                                    (SELECT 
                                            org_uid,  max_deal_count, request_type, valid_from, 
                                            end_date, valid_up_to, available_provision2_amount , available_provision3_amount , 
                                            standing_provision_amount, job_position_uid, emp_uid, line_count, status, 
                                            uid, created_by, created_time, modified_by, modified_time, 
                                            server_add_time, server_modified_time,Code,is_till_month_end
                                        FROM 
                                            sell_in_scheme_header
                                                    )As SubQuery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<ISellInSchemeHeader>(filterCriterias, sbFilterCriteria, parameters);
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
                IEnumerable<ISellInSchemeHeader> SellInHeader = await ExecuteQueryAsync<ISellInSchemeHeader>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<ISellInSchemeHeader> pagedResponse = new PagedResponse<ISellInSchemeHeader>
                {
                    PagedData = SellInHeader,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<ISellInSchemeHeader> GetSellInHeaderByUID(string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {
                        "UID", UID
                    }
                };

                var sql = @"SELECT 
                        org_uid,  max_deal_count, request_type, valid_from, 
                        end_date, valid_up_to, available_provision2_amount , available_provision3_amount , 
                        standing_provision_amount, job_position_uid, emp_uid, line_count, status, 
                        uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                        server_modified_time,Code,is_till_month_end,is_approval_created
                    FROM 
                        sell_in_scheme_header
                    WHERE uid = @UID";

                return await ExecuteSingleAsync<ISellInSchemeHeader>(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> CreateSellInHeader(ISellInSchemeHeader SellInHeader)
        {
            try
            {
                var sql = @"
                INSERT INTO sell_in_scheme_header (
                        org_uid, max_deal_count, request_type, valid_from, 
                        end_date, valid_up_to, available_provision2_amount, available_provision3_amount, 
                        standing_provision_amount, job_position_uid, emp_uid, line_count, status, 
                        uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                        server_modified_time,code,is_till_month_end,is_approval_created
                    ) VALUES (

                        @OrgUid, @MaxDealCount, @RequestType, @ValidFrom, 
                        @EndDate, @ValidUpTo, @AvailableProvision2Amount, @AvailableProvision3AMOUNT, 
                        @StandingProvisionAmount, @JobPositionUID, @EmpUID, @LineCount, @Status, 
                        @Uid, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, 
                        @ServerModifiedTime,@Code,@IsTillMonthEnd,@IsApprovalCreated);";

                return await ExecuteNonQueryAsync(sql, SellInHeader);
            }
            catch
            {
                throw;
            }
        }
        public async Task<int> DeleteSellInHeader(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {
                    "UID", UID
                }
            };
            var sql = @"DELETE FROM sell_in_scheme_header WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        public async Task<int> UpdateSellInHeader(ISellInSchemeHeader SellInHeader)
        {
            var sql = @"
               UPDATE sell_in_scheme_header
                    SET
                        org_uid = @OrgUid,
                        
                        max_deal_count = @MaxDealCount,
                        request_type = @RequestType,
                        valid_from = @ValidFrom,
                        end_date = @EndDate,
                        valid_up_to = @ValidUpTo,
                        available_provision2_amount = @AvailableProvision2Amount,
                        available_provision3_amount  = @AvailableProvision3Amount,
                        standing_provision_amount = @StandingProvisionAmount,
                        job_position_uid = @JobPositionUID,
                        emp_uid = @EmpUID,
                        line_count = @LineCount,
                        status = @Status,
                        created_by = @CreatedBy,
                        created_time = @CreatedTime,
                        modified_by = @ModifiedBy,
                        modified_time = @ModifiedTime,
                        server_add_time = @ServerAddTime,
                        server_modified_time = @ServerModifiedTime,
                        code = @Code,
                        is_till_month_end=@IsTillMonthEnd,
                        is_approval_created=@IsApprovalCreated
                    WHERE
                        uid = @Uid;";

            return await ExecuteNonQueryAsync(sql, SellInHeader);
        }
        public async Task<int> UpdateSellInHeaderAfterFinalApproval(ISellInSchemeHeader SellInHeader)
        {
            try
            {
                var sql = @"
               UPDATE sell_in_scheme_header
                    SET
                        approved_time=@ApprovedTime,
                        approved_by=@ApprovedBy
                    WHERE
                        uid = @Uid;";

                return await ExecuteNonQueryAsync(sql, SellInHeader);
            }
            catch (Exception) { throw; }
        }
        public async Task<PagedResponse<ISellInSchemeLine>> SelectAllSellInDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (SELECT 
                                        sell_in_scheme_header_uid, line_number, sku_uid, is_deleted, committed_qty, 
                                        dp_price, minimum_selling_price, laddering_price, request_price, invoice_discount_type, 
                                        discount, invoice_discount, credit_note_discount, final_dealer_price, 
                                        provision_amount_2, provision_amount_3, standing_provision_amount, 
                                        uid, created_by, created_time, modified_by, modified_time, 
                                        server_add_time, server_modified_time,credit_note_discount_type,start_date,end_date
                                    FROM 
                                        sell_in_scheme_line
                                                    )As SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM 
                                    (SELECT 
                                            sell_in_scheme_header_uid, line_number, sku_uid, is_deleted, committed_qty, 
                                            dp_price, minimum_selling_price, laddering_price, request_price, invoice_discount_type, 
                                            discount, invoice_discount, credit_note_discount, final_dealer_price, 
                                            provision_amount_2, provision_amount_3, standing_provision_amount, 
                                            uid, created_by, created_time, modified_by, modified_time, 
                                            server_add_time, server_modified_time,credit_note_discount_type,start_date,end_date
                                        FROM 
                                            sell_in_scheme_line
                                                    )As SubQuery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<ISellInSchemeLine>(filterCriterias, sbFilterCriteria, parameters);
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
                IEnumerable<ISellInSchemeLine> SellInDetails = await ExecuteQueryAsync<ISellInSchemeLine>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<ISellInSchemeLine> pagedResponse = new PagedResponse<ISellInSchemeLine>
                {
                    PagedData = SellInDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<ISellInSchemeLine> GetSellInDetailByUID(string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {
                        "UID", UID
                    }
                };

                var sql = @"SELECT 
                        sell_in_scheme_header_uid, line_number, sku_uid, is_deleted, committed_qty, 
                        dp_price, minimum_selling_price, laddering_price, request_price, invoice_discount_type, 
                        discount, invoice_discount, credit_note_discount, final_dealer_price, 
                        provision_amount_2, provision_amount_3, standing_provision_amount, 
                        uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                        server_modified_time,credit_note_discount_type,start_date,end_date
                    FROM 
                        sell_in_scheme_line
                    WHERE uid = @UID";

                return await ExecuteSingleAsync<ISellInSchemeLine>(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<ISellInSchemeLine>> GetSellInDetailByHeaderUID(string HeaderUID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {
                        "HeaderUID", HeaderUID
                    }
                };

                var sql = @"select '['+sku.code+'] ' + sku.name as SKUName,sku.code as SKUCode, SQ.* from(SELECT id,
                        sell_in_scheme_header_uid, line_number, sku_uid, is_deleted, committed_qty, 
                        dp_price, minimum_selling_price, laddering_price, request_price, invoice_discount_type, 
                        discount, invoice_discount, credit_note_discount, final_dealer_price, 
                        provision_amount_2, provision_amount_3, standing_provision_amount, 
                        uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                        server_modified_time,credit_note_discount_type
                    FROM 
                        sell_in_scheme_line)as SQ
						inner join sku on SQ.sku_uid=sku.uid
                    WHERE SQ.sell_in_scheme_header_uid = @HeaderUID";

                var data = await ExecuteQueryAsync<ISellInSchemeLine>(sql, parameters);
                if (data != null)
                {
                    return data.ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }
        protected async Task<IEnumerable<ISellInSchemeLine>> GetExistingSellInDetailByHeaderUID(string HeaderUID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {
                        "HeaderUID", HeaderUID
                    }
                };

                var sql = @"SELECT 
                        uid
                    FROM 
                        sell_in_scheme_line
                    WHERE sell_in_scheme_header_uid = @HeaderUID";

                return await ExecuteQueryAsync<ISellInSchemeLine>(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<ISellInSchemeHeader> GetExistingSellInHeaderByUID(string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {
                        "UID", UID
                    }
                };

                var sql = @"SELECT uid
                    FROM 
                        sell_in_scheme_header
                    WHERE uid = @UID";

                return await ExecuteSingleAsync<ISellInSchemeHeader>(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        //modification
        public async Task<int> CUSellInHeader(ISellInSchemeDTO sellInDTO)
        {
            int insCount = 0;
            //using (var transaction=)
            //{
            var exstSellHeader = await GetExistingSellInHeaderByUID(sellInDTO.SellInHeader.UID);
            var exstSellDetail = await GetExistingSellInDetailByHeaderUID(sellInDTO.SellInHeader.UID);
            insCount = exstSellHeader == null ? await CreateSellInHeader(sellInDTO.SellInHeader) :
                await UpdateSellInHeader(sellInDTO.SellInHeader);
            if (insCount > 0 && sellInDTO.SellInSchemeLines.Count > 0)
            {
                if (exstSellDetail == null || !exstSellDetail.Any())
                {
                    insCount += await CreateSellInDetail(sellInDTO.SellInSchemeLines);

                }
                else
                {
                    foreach (var item in sellInDTO.SellInSchemeLines)
                    {
                        bool isExst = exstSellDetail.Any(p => p.UID == item.UID);
                        insCount += isExst ? await UpdateSellInDetail(item) : await CreateSellInDetail(item);
                    }
                }
            }
            if (insCount > 0)
            {
                insCount += await CreateUpdateApprovalEngine(insCount, exstSellHeader, sellInDTO);

            }

            return insCount;
        }

        // written by Aziz
        private async Task<int> CreateUpdateApprovalEngine(int insCount, ISellInSchemeHeader exstSellHeader, ISellInSchemeDTO sellInDTO)
        {
            try
            {
                if (exstSellHeader == null)
                {
                    if (await CreateApprovalRequest(sellInDTO))
                    {
                        sellInDTO.SellInHeader.IsApprovalCreated = true;
                        return await UpdateSellInHeader(sellInDTO.SellInHeader);
                    }
                    else
                    {
                        throw new InvalidOperationException("Approval status create failed.");
                    }
                }
                else if (sellInDTO.ApprovalStatusUpdate != null)
                {
                    if (await _approvalEngineHelper.UpdateApprovalStatus(sellInDTO.ApprovalStatusUpdate))
                    {
                        return 1;
                    }
                    else
                    {
                        throw new InvalidOperationException("Approval engine update failed.");
                    }
                }
                else
                {
                    throw new Exception("Approval Engine create and update not required");
                }
            }
            catch (Exception e)
            {
                throw;// Rethrowing the exception to preserve stack trace
            }
        }

        private async Task<bool> CreateApprovalRequest(ISellInSchemeDTO sellInDTO)
        {
            try
            {
                IAllApprovalRequest approvalRequest = _serviceProvider.GetRequiredService<IAllApprovalRequest>();
                approvalRequest.LinkedItemType = "Scheme_SellIn";
                approvalRequest.LinkedItemUID = sellInDTO.SellInHeader.UID;
                ApprovalApiResponse<ApprovalStatus> approvalRequestCreated = await _approvalEngineHelper.CreateApprovalRequest(sellInDTO.ApprovalRequestItem, approvalRequest);
                return approvalRequestCreated.Success;
            }
            catch (Exception e)
            {
                throw;
            }
        }
        public async Task<int> CreateSellInDetail(List<ISellInSchemeLine> sellInDetails)
        {
            try
            {
                var sql = @"
                INSERT INTO sell_in_scheme_line (
                        sell_in_scheme_header_uid, line_number, sku_uid, is_deleted, committed_qty, 
                        dp_price, minimum_selling_price, laddering_price, request_price, invoice_discount_type, 
                        discount, invoice_discount, credit_note_discount, final_dealer_price, 
                        provision_amount_2, provision_amount_3, standing_provision_amount, 
                        uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                        server_modified_time,credit_note_discount_type
                    ) VALUES (
                        @SellInSchemeHeaderUID, @LineNumber, @SkuUID, @IsDeleted, @CommittedQty, 
                        @DPPrice, @MinimumSellingPrice, @LadderingPrice, @RequestPrice, @InvoiceDiscountType, 
                        @Discount, @InvoiceDiscount, @CreditNoteDiscount, @FinalDealerPrice, 
                        @ProvisionAmount2, @ProvisionAmount3, @StandingProvisionAmount, 
                        @Uid, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, 
                        @ServerModifiedTime,@CreditNoteDiscountType);";

                return await ExecuteNonQueryAsync(sql, sellInDetails);
            }
            catch
            {
                throw;
            }
        }
        public async Task<int> CreateSellInDetail(ISellInSchemeLine sellInDetail)
        {
            try
            {
                var sql = @"
                INSERT INTO sell_in_scheme_line (
                        sell_in_scheme_header_uid, line_number, sku_uid, is_deleted, committed_qty, 
                        dp_price, minimum_selling_price, laddering_price, request_price, invoice_discount_type, 
                        discount, invoice_discount, credit_note_discount, final_dealer_price, 
                        provision_amount_2, provision_amount_3, standing_provision_amount, 
                        uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                        server_modified_time,credit_note_discount_type
                    ) VALUES (
                        @SellInSchemeHeaderUID, @LineNumber, @SkuUID, @IsDeleted, @CommittedQty, 
                        @DPPrice, @MinimumSellingPrice, @LadderingPrice, @RequestPrice, @InvoiceDiscountType, 
                        @Discount, @InvoiceDiscount, @CreditNoteDiscount, @FinalDealerPrice, 
                        @ProvisionAmount2, @ProvisionAmount3, @StandingProvisionAmount, 
                        @Uid, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, 
                        @ServerModifiedTime,@CreditNoteDiscountType);";

                return await ExecuteNonQueryAsync(sql, sellInDetail);
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> UpdateSellInDetail(ISellInSchemeLine sellInDetail)
        {
            var sql = @"
               UPDATE sell_in_scheme_line
                    SET
                        sell_in_scheme_header_uid = @SellInSchemeHeaderUID,
                        line_number = @LineNumber,
                        sku_uid = @SkuUID,
                        is_deleted = @IsDeleted,
                        committed_qty = @CommittedQty,
                        dp_price = @DPPrice,
                        minimum_selling_price = @MinimumSellingPrice,
                        laddering_price = @LadderingPrice,
                        request_price = @RequestPrice,
                        invoice_discount_type = @InvoiceDiscountType,
                        discount = @Discount,
                        invoice_discount = @InvoiceDiscount,
                        credit_note_discount = @CreditNoteDiscount,
                        final_dealer_price = @FinalDealerPrice,
                        provision_amount_2 = @ProvisionAmount2,
                        provision_amount_3 = @ProvisionAmount3,
                        standing_provision_amount = @StandingProvisionAmount,
                        created_by = @CreatedBy,
                        created_time = @CreatedTime,
                        modified_by = @ModifiedBy,
                        modified_time = @ModifiedTime,
                        server_add_time = @ServerAddTime,
                        server_modified_time = @ServerModifiedTime,
                        credit_note_discount_type = @CreditNoteDiscountType
                       
                    WHERE
                        uid = @Uid;";

            return await ExecuteNonQueryAsync(sql, sellInDetail);
        }

        public async Task<int> DeleteSellInDetail(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {
                    "UID", UID
                }
            };
            var sql = @"DELETE FROM sell_in_scheme_line WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        public async Task<int> UpdateSellInMappingData(string schemeUID)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@scheme_uid", schemeUID);
                return await ExecuteProcedureAsync("UpdateSellInMappingData", parameters);
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<IStandingProvision>> GetStandingProvisionAmountByChannelPartnerUID(string channelPartnerUID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {
                        "ChannelPartnerUID", channelPartnerUID
                    }
                };

                var sql = @"select  e.sku_uid,SUM(sp.amount) amount 
                                                            from standing_provision_scheme sp
                                                            inner join sku_ext_data e on 
                                                            (e.category=SP.sku_category_code OR ISNULL(SP.sku_category_code,'')='') 
															and (e.product_type=sp.sku_type_code OR ISNULL(sp.sku_type_code,'')='' )
                                                            and (e.star_rating=sp.star_rating_code or ISNULL(sp.star_rating_code ,'')='') 
                                                            and (e.tonnage=sp.sku_tonnage_code or Isnull(sp.sku_tonnage_code,'')='')
															 and (e.item_series=sp.sku_series_code or Isnull(sp.sku_series_code,'')='')
															  and (e.capacity=sp.star_capacity_code or Isnull(sp.star_capacity_code,'')='')
															   and (e.type=sp.sku_product_group or Isnull(sp.sku_product_group,'')='')
                                                            and cast(getdate() as date)  between sp.start_date and sp.end_date 
                                                            and Exists ( 
                                                            select s.uid, sp1.* from standing_provision_scheme sp1
                                                            inner join store S on 1=1 
                                                            inner join address a on a.linked_item_uid=s.uid and a.linked_item_type='Store' and s.uid=@ChannelPartnerUID 
                                                            left join standing_provision_scheme_branch sb on sb.standing_provision_scheme_uid=sp1.uid and sb.branch_code=a.branch_uid
                                                            left join standing_provision_scheme_applicable_org SO on SO.standing_provision_uid=sp1.uid and so.org_uid=s.uid
                                                            left join standing_provision_scheme_broad_classification sbc on sbc.standing_provision_uid=sp1.uid and sbc.broad_classification_code=s.broad_classification
                                                            where (sb.standing_provision_scheme_uid  is not null  or so.standing_provision_uid is not null or sbc.standing_provision_uid is not null)
                                                            and sp.uid =sp1.uid)
                                                            and not exists(select * from dbo.Split(sp.excluded_models,',') s where s.Value=e.sku_uid)
															inner join sku on e.sku_uid =sku.uid 
															inner join standing_provision_scheme_division spd on spd.division_org_uid=sku.supplier_org_uid and spd.standing_provision_uid=sp.uid
                                                            group by   e.sku_uid";

                var data = await ExecuteQueryAsync<IStandingProvision>(sql, parameters);
                if (data != null)
                {
                    return data.ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }
        public async Task<ISellInSchemeDTO> GetSellInSchemeByOrgUid(string OrgUid, DateTime OrderDate)
        {
            ISellInSchemeDTO sellInSchemeDTO = new SellInSchemeDTO();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {
                        "OrgUid", OrgUid
                    },
                    {
                        "OrderDate", OrderDate
                    },
                };

                var headerSql = @$"select * from sell_in_scheme_header where status='{SchemeConstants.Approved}' and  @OrderDate between valid_from and  end_date";

                sellInSchemeDTO.SellInSchemeHeaders = await ExecuteQueryAsync<ISellInSchemeHeader>(headerSql, parameters);
                if (sellInSchemeDTO.SellInSchemeHeaders != null && sellInSchemeDTO.SellInSchemeHeaders.Count > 0)
                {
                    var lineSql = $"""
                                   select SL.* from sell_in_scheme_line SL 
                                           inner join sell_in_scheme_header  SH on SH.uid=SL.sell_in_scheme_header_uid and 
                                           @OrderDate between valid_from and  end_date
                                           where  SH.status='{SchemeConstants.Approved}'
                                   """;

                    sellInSchemeDTO.SellInSchemeLines = await ExecuteQueryAsync<ISellInSchemeLine>(lineSql, parameters);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return sellInSchemeDTO;
        }

        public async Task<List<ISellInSchemePO>> GetSellInSchemesByOrgUidAndSKUUid(string orgUid, List<string>? sKUUIDs = null)
        {
            try
            {
                StringBuilder sql = new StringBuilder(
                """
                   SELECT scheme_uid,SchemeCode, sku_uid, invoice_discount, cnp1, cnp2, cnp3, invoice_discount_type, credit_note_discount_type,
                sell_in_scheme_line_uid as SellInSchemeLineUID , ended_by_sell_in_scheme_line_uid As EndedBySellInSchemeLineUID
                FROM (
                SELECT ROW_NUMBER() OVER (Partition BY simd.org_uid, simd.sku_uid ORDER BY simd.approved_time DESC) AS RowNum,
                simd.* , SH.code as SchemeCode,
                sisl.invoice_discount_type, sisl.credit_note_discount_type
                FROM  sell_in_scheme_header SH(Nolock)
                inner join sell_in_mapping_data simd(Nolock) on Sh.uid=simd.scheme_uid
                inner join sell_in_scheme_line sisl(Nolock) on sisl.uid = simd.sell_in_scheme_line_uid  and simd.org_uid = @OrgUID 
                AND DATEDIFF(mi, simd.start_date, getdate()) >= 0 AND DATEDIFF(mi, getdate(), simd.end_date) > 0
                ) T WHERE T.RowNum = 1 
                """);
                var parameters = new
                {
                    OrgUid = orgUid,
                    SKUUIds = sKUUIDs
                };
                if (sKUUIDs != null && sKUUIDs.Any())
                {
                    sql.Append($" AND sku_uid IN @SKUUIds ");
                }
                return await ExecuteQueryAsync<ISellInSchemePO>(sql.ToString(), parameters);
            }
            catch (Exception e)
            {
                throw;
            }
        }
         public async Task<List<ISellInSchemePO>> GetExistSellInSchemesByPOUid(string POHeaderUID)
        {
            try
            {
                StringBuilder sql = new StringBuilder(
                """
                  
                with Schemes as(
                select  distinct polp.scheme_code,pl.sku_uid from purchase_order_header ph 
                inner join purchase_order_line pl on ph.uid=pl.purchase_order_header_uid
                inner join purchase_order_line_provision polp on polp.purchase_order_line_uid=pl.uid 
                and polp.provision_type like 'SellIn%' and ph.uid=@POHeaderUID
                )

                 SELECT distinct scheme_uid,SchemeCode, sku_uid, invoice_discount, cnp1, cnp2, cnp3, invoice_discount_type, credit_note_discount_type,
                sell_in_scheme_line_uid as SellInSchemeLineUID , ended_by_sell_in_scheme_line_uid As EndedBySellInSchemeLineUID
                FROM (
                SELECT ROW_NUMBER() OVER (Partition BY simd.org_uid, simd.sku_uid ORDER BY simd.approved_time DESC) AS RowNum,
                simd.* , SH.code as SchemeCode,
                sisl.invoice_discount_type, sisl.credit_note_discount_type
                FROM  sell_in_scheme_header SH
                inner join sell_in_mapping_data simd on Sh.uid=simd.scheme_uid
                inner join sell_in_scheme_line sisl on sisl.uid = simd.sell_in_scheme_line_uid  
                inner join Schemes s on s.scheme_code=sh.code and simd.sku_uid=s.sku_uid
                ) T WHERE T.RowNum = 1 
                """);
                var parameters = new 
                {
                     POHeaderUID = POHeaderUID ,
                };
              
                return await ExecuteQueryAsync<ISellInSchemePO>(sql.ToString(), parameters);
            }
            catch (Exception e)
            {
                throw;
            }
        }



    }
}
