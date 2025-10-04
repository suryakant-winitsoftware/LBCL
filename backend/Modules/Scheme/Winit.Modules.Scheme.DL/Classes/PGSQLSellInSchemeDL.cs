using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.DL.DBManager;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Winit.Modules.Scheme.DL.Classes
{
    public class PGSQLSellInSchemeDL : PostgresDBManager, ISellInSchemeDL
    {
        public PGSQLSellInSchemeDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<ISellInSchemeHeader>> SelectAllSellInHeader(List<SortCriteria> sortCriterias, int pageNumber,
   int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (SELECT 
                                        org_uid, franchisee_org_uid, max_deal_count, request_type, valid_from, 
                                        end_date, valid_up_to, available_provision2_amount , available_provision3_amount , 
                                        standing_provision_amount, job_position_uid, emp_uid, line_count, status, 
                                        uid, created_by, created_time, modified_by, modified_time, 
                                        server_add_time, server_modified_time
                                    FROM 
                                        sell_in_scheme_header
                                                    )As SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM 
                                    (SELECT 
                                            org_uid, franchisee_org_uid, max_deal_count, request_type, valid_from, 
                                            end_date, valid_up_to, available_provision2_amount , available_provision3_amount , 
                                            standing_provision_amount, job_position_uid, emp_uid, line_count, status, 
                                            uid, created_by, created_time, modified_by, modified_time, 
                                            server_add_time, server_modified_time
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
            { "UID", UID }
        };

                var sql = @"SELECT 
                        org_uid, franchisee_org_uid, max_deal_count, request_type, valid_from, 
                        end_date, valid_up_to, available_provision2_amount , available_provision3_amount , 
                        standing_provision_amount, job_position_uid, emp_uid, line_count, status, 
                        uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                        server_modified_time
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
                        org_uid, franchisee_org_uid, max_deal_count, request_type, valid_from, 
                        end_date, valid_up_to, available_provision2_amount, available_provision3_amount, 
                        standing_provision_amount, job_position_uid, emp_uid, line_count, status, 
                        uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                        server_modified_time
                    ) VALUES (
                        @OrgUid, @FranchiseeOrgUid, @MaxDealCount, @RequestType, @ValidFrom, 
                        @EndDate, @ValidUpTo, @AvailableProvision2Amount, @AvailableProvision3AMOUNT, 
                        @StandingProvisionAmount, @JobPositionUID, @EmpUID, @LineCount, @Status, 
                        @Uid, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, 
                        @ServerModifiedTime);";

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
                    {"UID", UID}
                };
            var sql = @"DELETE FROM sell_in_scheme_header WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        protected async Task<IEnumerable<ISellInSchemeLine>> GetExistingSellInDetailByHeaderUID(string HeaderUID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "HeaderUID", HeaderUID }
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
            { "UID", UID }
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
        public async Task<int> CUSellInHeader(ISellInSchemeDTO sellInDTO)
        {
            int insCount = 0;
            List<string>? exstSellDetail = null;
            if (sellInDTO != null && sellInDTO.SellInSchemeLines != null && sellInDTO.SellInSchemeLines?.Count > 0)
            {
                exstSellDetail = await CheckIfUIDExistsInDB(DbTableName.SellInSchemeHeader, sellInDTO.SellInSchemeLines.Select(p => p.UID).ToList());
            }
            string? exstSellHeader = await CheckIfUIDExistsInDB("sell_in_scheme_header", sellInDTO.SellInHeader.UID);
            insCount = exstSellHeader == null ? await CreateSellInHeader(sellInDTO.SellInHeader) :
               await UpdateSellInHeader(sellInDTO.SellInHeader);


            if (insCount > 0 && sellInDTO.SellInSchemeLines?.Count > 0)
            {
                if (exstSellDetail == null)
                {
                    insCount += await CreateSellInDetail(sellInDTO.SellInSchemeLines);
                }
                else
                {
                    foreach (var item in sellInDTO.SellInSchemeLines)
                    {
                        bool isExst = exstSellDetail.Contains(item.UID);
                        insCount += isExst ? await UpdateSellInDetail(item) : await CreateSellInDetail(item);
                    }
                }
            }



            return insCount;
        }
        public async Task<int> UpdateSellInHeader(ISellInSchemeHeader SellInHeader)
        {
            var sql = @"
               UPDATE sell_in_scheme_header
                    SET
                        org_uid = @OrgUid,
                        franchisee_org_uid = @FranchiseeOrgUid,
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
                        server_modified_time = @ServerModifiedTime
                    WHERE
                        uid = @Uid;";

            return await ExecuteNonQueryAsync(sql, SellInHeader);
        }
        public async Task<PagedResponse<ISellInSchemeLine>> SelectAllSellInDetails(List<SortCriteria> sortCriterias, int pageNumber,
                                                                               int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (SELECT 
                                        sell_in_scheme_header_uid, line_number, sku_uid, is_deleted, committed_qty, 
                                        dp_price, minimum_selling_price, laddering_price, request_price, discount_type, 
                                        discount, invoice_discount, credit_note_discount, final_dealer_price, 
                                        provision_amount_2, provision_amount_3, standing_provision_amount, 
                                        uid, created_by, created_time, modified_by, modified_time, 
                                        server_add_time, server_modified_time
                                    FROM 
                                        sell_in_scheme_line
                                                    )As SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM 
                                    (SELECT 
                                            sell_in_scheme_header_uid, line_number, sku_uid, is_deleted, committed_qty, 
                                            dp_price, minimum_selling_price, laddering_price, request_price, discount_type, 
                                            discount, invoice_discount, credit_note_discount, final_dealer_price, 
                                            provision_amount_2, provision_amount_3, standing_provision_amount, 
                                            uid, created_by, created_time, modified_by, modified_time, 
                                            server_add_time, server_modified_time
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
        public async Task<List<ISellInSchemeLine>> GetSellInDetailByHeaderUID(string HeaderUID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "HeaderUID", HeaderUID }
                };

                var sql = @"SELECT id
                        sell_in_scheme_header_uid, line_number, sku_uid, is_deleted, committed_qty, 
                        dp_price, minimum_selling_price, laddering_price, request_price, discount_type, 
                        discount, invoice_discount, credit_note_discount, final_dealer_price, 
                        provision_amount_2, provision_amount_3, standing_provision_amount, 
                        uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                        server_modified_time
                    FROM 
                        sell_in_scheme_line
                    WHERE sell_in_scheme_header_uid = @HeaderUID";

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
        public async Task<ISellInSchemeLine> GetSellInDetailByUID(string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "UID", UID }
                };

                var sql = @"SELECT 
                        sell_in_scheme_header_uid, line_number, sku_uid, is_deleted, committed_qty, 
                        dp_price, minimum_selling_price, laddering_price, request_price, discount_type, 
                        discount, invoice_discount, credit_note_discount, final_dealer_price, 
                        provision_amount_2, provision_amount_3, standing_provision_amount, 
                        uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                        server_modified_time
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
        public async Task<int> CreateSellInDetail(List<ISellInSchemeLine> sellInDetails)
        {
            try
            {
                var sql = @"
                INSERT INTO sell_in_scheme_line (
                        sell_in_scheme_header_uid, line_number, sku_uid, is_deleted, committed_qty, 
                        dp_price, minimum_selling_price, laddering_price, request_price, discount_type, 
                        discount, invoice_discount, credit_note_discount, final_dealer_price, 
                        provision_amount_2, provision_amount_3, standing_provision_amount, 
                        uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                        server_modified_time
                    ) VALUES (
                        @SellInSchemeHeaderUID, @LineNumber, @SkuUID, @IsDeleted, @CommittedQty, 
                        @DPPrice, @MinimumSellingPrice, @LadderingPrice, @RequestPrice, @DiscountType, 
                        @Discount, @InvoiceDiscount, @CreditNoteDiscount, @FinalDealerPrice, 
                        @ProvisionAmount2, @ProvisionAmount3, @StandingProvisionAmount, 
                        @Uid, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, 
                        @ServerModifiedTime);";

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
                        dp_price, minimum_selling_price, laddering_price, request_price, discount_type, 
                        discount, invoice_discount, credit_note_discount, final_dealer_price, 
                        provision_amount_2, provision_amount_3, standing_provision_amount, 
                        uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                        server_modified_time
                    ) VALUES (
                        @SellInSchemeHeaderUID, @LineNumber, @SkuUID, @IsDeleted, @CommittedQty, 
                        @DPPrice, @MinimumSellingPrice, @LadderingPrice, @RequestPrice, @DiscountType, 
                        @Discount, @InvoiceDiscount, @CreditNoteDiscount, @FinalDealerPrice, 
                        @ProvisionAmount2, @ProvisionAmount3, @StandingProvisionAmount, 
                        @Uid, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, 
                        @ServerModifiedTime);";

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
                        discount_type = @DiscountType,
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
                        server_modified_time = @ServerModifiedTime
                    WHERE
                        uid = @Uid;";

            return await ExecuteNonQueryAsync(sql, sellInDetail);
        }

        public async Task<int> DeleteSellInDetail(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID", UID}
            };
            var sql = @"DELETE FROM sell_in_scheme_line WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<List<ISellInSchemePO>> GetSellInSchemesByOrgUidAndSKUUid(string orgUid, List<string>? sKUUIDs = null)
        {
            throw new NotImplementedException();
        }        
        public async Task<List<IStandingProvision>> GetStandingProvisionAmountByChannelPartnerUID(string channelPartnerUID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "channelPartnerUID", channelPartnerUID }
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
    }
}
