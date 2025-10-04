
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ReturnOrder.DL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ReturnOrder.DL.Classes
{
    public class MSSQLReturnOrderLineDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IReturnOrderLineDL
    {
        public MSSQLReturnOrderLineDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine>> SelectAllReturnOrderLineDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT 
                                            id AS Id,
                                            uid AS Uid,
                                            created_by AS CreatedBy,
                                            created_time AS CreatedTime,
                                            modified_by AS ModifiedBy,
                                            modified_time AS ModifiedTime,
                                            server_add_time AS ServerAddTime,
                                            server_modified_time AS ServerModifiedTime,
                                            return_order_uid AS ReturnOrderUid,
                                            line_number AS LineNumber,
                                            sku_uid AS SkuUid,
                                            sku_code AS SkuCode,
                                            sku_type AS SkuType,
                                            base_price AS BasePrice,
                                            unit_price AS UnitPrice,
                                            fake_unit_price AS FakeUnitPrice,
                                            base_uom AS BaseUom,
                                            uom AS Uom,
                                            multiplier AS Multiplier,
                                            qty AS Qty,
                                            qty_bu AS QtyBu,
                                            approved_qty AS ApprovedQty,
                                            returned_qty AS ReturnedQty,
                                            total_amount AS TotalAmount,
                                            total_discount AS TotalDiscount,
                                            total_excise_duty AS TotalExciseDuty,
                                            total_tax AS TotalTax,
                                            net_amount AS NetAmount,
                                            sku_price_uid AS SkuPriceUid,
                                            sku_price_list_uid AS SkuPriceListUid,
                                            reason_code AS ReasonCode,
                                            reason_text AS ReasonText,
                                            expiry_date AS ExpiryDate,
                                            batch_number AS BatchNumber,
                                            sales_order_uid AS SalesOrderUid,
                                            sales_order_line_uid AS SalesOrderLineUid,
                                            remarks AS Remarks,
                                            volume AS Volume,
                                            volume_unit AS VolumeUnit,
                                            promotion_uid AS PromotionUid,
                                            net_fake_amount AS NetFakeAmount
                                        FROM Return_Order_Line");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM Return_Order_Line");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine>(filterCriterias, sbFilterCriteria, parameters); ;

                    sql.Append(sbFilterCriteria);
                    // If count required then add filters to count
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
                    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }

                //Data
                IEnumerable<Model.Interfaces.IReturnOrderLine> returnOrderLines = await ExecuteQueryAsync<Model.Interfaces.IReturnOrderLine>(sql.ToString(), parameters);
                //Count
                int totalCount = 0;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine> pagedResponse = new PagedResponse<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine>
                {
                    PagedData = returnOrderLines,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine> SelectReturnOrderLineByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"SELECT 
                            id AS Id,
                            uid AS Uid,
                            created_by AS CreatedBy,
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            return_order_uid AS ReturnOrderUid,
                            line_number AS LineNumber,
                            sku_uid AS SkuUid,
                            sku_code AS SkuCode,
                            sku_type AS SkuType,
                            base_price AS BasePrice,
                            unit_price AS UnitPrice,
                            fake_unit_price AS FakeUnitPrice,
                            base_uom AS BaseUom,
                            uom AS Uom,
                            multiplier AS Multiplier,
                            qty AS Qty,
                            qty_bu AS QtyBu,
                            approved_qty AS ApprovedQty,
                            returned_qty AS ReturnedQty,
                            total_amount AS TotalAmount,
                            total_discount AS TotalDiscount,
                            total_excise_duty AS TotalExciseDuty,
                            total_tax AS TotalTax,
                            net_amount AS NetAmount,
                            sku_price_uid AS SkuPriceUid,
                            sku_price_list_uid AS SkuPriceListUid,
                            reason_code AS ReasonCode,
                            reason_text AS ReasonText,
                            expiry_date AS ExpiryDate,
                            batch_number AS BatchNumber,
                            sales_order_uid AS SalesOrderUid,
                            sales_order_line_uid AS SalesOrderLineUid,
                            remarks AS Remarks,
                            volume AS Volume,
                            volume_unit AS VolumeUnit,
                            promotion_uid AS PromotionUid,
                            net_fake_amount AS NetFakeAmount
                        FROM return_order_line where uid=@UID";
            Model.Interfaces.IReturnOrderLine ReturnOrderLineList = await ExecuteSingleAsync<Model.Interfaces.IReturnOrderLine>(sql, parameters);
            return ReturnOrderLineList;
        }
        public async Task<int> CreateReturnOrderLine(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine returnOrderLine)
        {
            try
            {
                var sql = @"INSERT INTO return_order_line ( uid, created_by, created_time, modified_by, modified_time, server_add_time,
                            server_modified_time, return_order_uid, line_number, sku_uuid, sku_code, sku_type, base_price, unit_price, 
                            fake_unit_price, base_uom, uom, multiplier, qty, qty_bu, approved_qty, returned_qty, total_amount,
                            total_discount, total_excise_duty, total_tax, net_amount, sku_price_uid, sku_price_list_uid, reason_code, 
                            reason_text, expiry_date, batch_number, sales_order_uid, sales_order_line_uid, remarks, volume, volume_unit,
                            promotion_uid, net_fake_amount) VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, 
                            @ServerModifiedTime, @ReturnOrderUID, @LineNumber, @SKUUID, @SKUCode, @SKUType, @BasePrice, @UnitPrice, @FakeUnitPrice, @BaseUOM, 
                            @UoM, @Multiplier, @Qty, @QtyBU, @ApprovedQty, @ReturnedQty, @TotalAmount, @TotalDiscount, @TotalExciseDuty, @TotalTax, @NetAmount, 
                            @SKUPriceUID, @SKUPriceListUID, @ReasonCode, @ReasonText, @ExpiryDate, @BatchNumber, @SalesOrderUID, @SalesOrderLineUID, @Remarks, 
                            @Volume, @VolumeUnit, @PromotionUID, @NetFakeAmount)";
                
                return await ExecuteNonQueryAsync(sql, returnOrderLine);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateReturnOrderLine(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine returnOrderLine)
        {
            try
            {
                var sql = @"UPDATE return_order_line SET 
                            modified_by = @ModifiedBy, 
                            modified_time = @ModifiedTime, 
                            server_modified_time = @ServerModifiedTime, 
                            return_order_uid = @ReturnOrderUID, 
                            line_number = @LineNumber, 
                            sku_uuid = @SKUUID, 
                            sku_code = @SKUCode, 
                            sku_type = @SKUType, 
                            base_price = @BasePrice, 
                            unit_price = @UnitPrice, 
                            fake_unit_price = @FakeUnitPrice, 
                            base_uom = @BaseUOM, 
                            uom = @UoM, 
                            multiplier = @Multiplier, 
                            qty = @Qty, 
                            qty_bu = @QtyBU, 
                            approved_qty = @ApprovedQty, 
                            returned_qty = @ReturnedQty, 
                            total_amount = @TotalAmount, 
                            total_discount = @TotalDiscount, 
                            total_excise_duty = @TotalExciseDuty, 
                            total_tax = @TotalTax, 
                            net_amount = @NetAmount, 
                            sku_price_uid = @SKUPriceUID, 
                            sku_price_list_uid = @SKUPriceListUID, 
                            reason_code = @ReasonCode, 
                            reason_text = @ReasonText, 
                            expiry_date = @ExpiryDate, 
                            batch_number = @BatchNumber, 
                            sales_order_uid = @SalesOrderUID, 
                            sales_order_line_uid = @SalesOrderLineUID, 
                            remarks = @Remarks, 
                            volume = @Volume, 
                            volume_unit = @VolumeUnit, 
                            promotion_uid = @PromotionUID, 
                            net_fake_amount = @NetFakeAmount
                             WHERE uid = @UID;";
                
                return await ExecuteNonQueryAsync(sql, returnOrderLine);

            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteReturnOrderLine(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE  FROM return_order_line WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
    }
}
