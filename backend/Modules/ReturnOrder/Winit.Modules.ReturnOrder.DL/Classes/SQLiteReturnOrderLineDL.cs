
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
    public class SQLiteReturnOrderLineDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, IReturnOrderLineDL
    {
        public SQLiteReturnOrderLineDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider)
        {
        }
        public async Task<PagedResponse<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine>> SelectAllReturnOrderLineDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                return_order_uid, line_number, sku_uid, sku_code, sku_type, base_price, unit_price, fake_unit_price, base_uom, uom, multiplier, 
                qty, qty_bu, approved_qty, returned_qty, total_amount, total_discount, total_excise_duty, total_tax, net_amount, sku_price_uid,
                sku_price_list_uid, reason_code, reason_text, expiry_date, batch_number, sales_order_uid, sales_order_line_uid, remarks, volume,
                volume_unit, promotion_uid, net_fake_amount, po_number
	                FROM return_order_line");
                var sqlCount = new StringBuilder();
                if (isCountRequired)    
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM return_order_line");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);
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
                    if (sortCriterias != null && sortCriterias.Count > 0)
                    {
                        sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                    else
                    {
                        sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }

                IEnumerable<Model.Interfaces.IReturnOrderLine> returnOrderLines = await 
                    ExecuteQueryAsync<Model.Interfaces.IReturnOrderLine>(sql.ToString(), parameters);
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
            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, 
            server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, return_order_uid AS ReturnOrderUID, line_number AS LineNumber,
            skuuuid AS SKUUID, sku_code AS SKUCode, sku_type AS SKUType, base_price AS BasePrice, unit_price AS UnitPrice, fake_unit_price AS FakeUnitPrice,
            base_uom AS BaseUOM, uom AS UoM, multiplier AS Multiplier, qty AS Qty, qty_bu AS QtyBU, approved_qty AS ApprovedQty, returned_qty AS ReturnedQty,
            total_amount AS TotalAmount, total_discount AS TotalDiscount, total_excise_duty AS TotalExciseDuty, total_tax AS TotalTax, net_amount AS NetAmount,
            sku_price_uid AS SKUPriceUID, sku_price_list_uid AS SKUPriceListUID, reason_code AS ReasonCode, reason_text AS ReasonText, expiry_date AS ExpiryDate,
            batch_number AS BatchNumber, sales_order_uid AS SalesOrderUID, sales_order_line_uid AS SalesOrderLineUID, remarks AS Remarks, volume AS Volume,
            volume_unit AS VolumeUnit, promotion_uid AS PromotionUID, net_fake_amount AS NetFakeAmount, po_number AS PONumber FROM return_order_line";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IReturnOrderLine>().GetType();
            Model.Interfaces.IReturnOrderLine ReturnOrderLineList = await ExecuteSingleAsync<Model.Interfaces.IReturnOrderLine>(sql, parameters, type);
            return ReturnOrderLineList;
        }
        public async Task<int> CreateReturnOrderLine(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine returnOrderLine)
        {
            try
            {
                var sql = @"INSERT INTO ReturnOrderLine (uid AS UID,created_by AS CreatedBy,created_time AS CreatedTime,modified_by AS ModifiedBy,modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime,return_order_uid AS ReturnOrderUID,line_number AS LineNumber,
                            sku_uid AS SKUUID,sku_uid AS SKUCode,sku_type AS SKUType,base_price AS BasePrice,unit_price AS UnitPrice, fake_unit_price AS FakeUnitPrice,
                            base_uom AS BaseUOM,uom AS UoM,multiplier AS Multiplier,qty AS Qty,qty_bu AS QtyBU,approved_qty AS ApprovedQty,returned_qty AS ReturnedQty,
                            total_amount AS TotalAmount,total_discount AS TotalDiscount,total_excise_duty AS TotalExciseDuty,total_tax AS TotalTax,net_worth AS NetAmount,
                            sku_price_uid AS SKUPriceUID,sku_price_list_uid AS SKUPriceListUID,reason_code AS ReasonCode,reason_text AS ReasonText,expiry_date AS ExpiryDate, 
                            batch_number AS BatchNumber,sales_order_uid AS SalesOrderUID,sales_order_line_uid AS SalesOrderLineUID,remarks AS Remarks,volume AS Volume,volume_unit AS VolumeUnit,
                            promotion_uid AS PromotionUID,net_fake_amount AS NetFakeAmount,po_numebr AS PONumber)
                            VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, 
                            @ServerModifiedTime, @ReturnOrderUID, @LineNumber, @SKUUID, @SKUCode, @SKUType, @BasePrice, @UnitPrice, @FakeUnitPrice, @BaseUOM, 
                            @UoM, @Multiplier, @Qty, @QtyBU, @ApprovedQty, @ReturnedQty, @TotalAmount, @TotalDiscount, @TotalExciseDuty, @TotalTax, @NetAmount, 
                            @SKUPriceUID, @SKUPriceListUID, @ReasonCode, @ReasonText, @ExpiryDate, @BatchNumber, @SalesOrderUID, @SalesOrderLineUID, @Remarks, 
                            @Volume, @VolumeUnit, @PromotionUID, @NetFakeAmount,@PONumber)";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "@UID", returnOrderLine.UID },
                    { "@CreatedBy", returnOrderLine.CreatedBy },
                    { "@CreatedTime", returnOrderLine.CreatedTime },
                    { "@ModifiedBy", returnOrderLine.ModifiedBy },
                    { "@ModifiedTime", returnOrderLine.ModifiedTime },
                    { "@ServerAddTime", returnOrderLine.ServerAddTime },
                    { "@ServerModifiedTime", returnOrderLine.ServerModifiedTime },
                    { "@ReturnOrderUID", returnOrderLine.ReturnOrderUID },
                    { "@LineNumber", returnOrderLine.LineNumber },
                    { "@SKUUID", returnOrderLine.SKUUID },
                    { "@SKUCode", returnOrderLine.SKUCode },
                    { "@SKUType", returnOrderLine.SKUType },
                    { "@BasePrice", returnOrderLine.BasePrice },
                    { "@UnitPrice", returnOrderLine.UnitPrice },
                    { "@FakeUnitPrice", returnOrderLine.FakeUnitPrice },
                    { "@BaseUOM", returnOrderLine.BaseUOM },
                    { "@UoM", returnOrderLine.UoM },
                    { "@Multiplier", returnOrderLine.Multiplier },
                    { "@Qty", returnOrderLine.Qty },
                    { "@QtyBU", returnOrderLine.QtyBU },
                    { "@ApprovedQty", returnOrderLine.ApprovedQty },
                    { "@ReturnedQty", returnOrderLine.ReturnedQty },
                    { "@TotalAmount", returnOrderLine.TotalAmount },
                    { "@TotalDiscount", returnOrderLine.TotalDiscount },
                    { "@TotalExciseDuty", returnOrderLine.TotalExciseDuty },
                    { "@TotalTax", returnOrderLine.TotalTax },
                    { "@NetAmount", returnOrderLine.NetAmount },
                    { "@SKUPriceUID", returnOrderLine.SKUPriceUID },
                    { "@SKUPriceListUID", returnOrderLine.SKUPriceListUID },
                    { "@ReasonCode", returnOrderLine.ReasonCode },
                    { "@ReasonText", returnOrderLine.ReasonText },
                    { "@ExpiryDate", returnOrderLine.ExpiryDate },
                    { "@BatchNumber", returnOrderLine.BatchNumber },
                    { "@SalesOrderUID", returnOrderLine.SalesOrderUID },
                    { "@SalesOrderLineUID", returnOrderLine.SalesOrderLineUID },
                    { "@Remarks", returnOrderLine.Remarks },
                    { "@Volume", returnOrderLine.Volume },
                    { "@VolumeUnit", returnOrderLine.VolumeUnit },
                    { "@PromotionUID", returnOrderLine.PromotionUID },
                    { "@NetFakeAmount", returnOrderLine.NetFakeAmount },
                    { "@PONumber", returnOrderLine.PONumber },
                };
                return await ExecuteNonQueryAsync(sql, parameters);
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
                var sql = @"UPDATE ReturnOrderLine SET 
                            modified_by AS ModifiedBy = @ModifiedBy, 
                            modified_time AS ModifiedTime = @ModifiedTime, 
                            server_modified_time ASServerModifiedTime = @ServerModifiedTime, 
                            return_order_uid AS ReturnOrderUID = @ReturnOrderUID, 
                            line_number AS LineNumber = @LineNumber, 
                            sku_uid AS SKUUID = @SKUUID, 
                            sku_code AS SKUCode = @SKUCode, 
                            sku_type AS SKUType = @SKUType, 
                            base_price AS BasePrice = @BasePrice, 
                            unit_price AS UnitPrice = @UnitPrice, 
                            fake_unit_price AS FakeUnitPrice = @FakeUnitPrice, 
                            base_uom AS BaseUOM = @BaseUOM, 
                            uom AS UoM = @UoM, 
                            multiplier AS Multiplier = @Multiplier, 
                            qty  AS Qty = @Qty, 
                            qty_bu AS QtyBU = @QtyBU, 
                            approved_qty AS ApprovedQty = @ApprovedQty, 
                            returned_qty AS ReturnedQty = @ReturnedQty, 
                            total_amount AS TotalAmount = @TotalAmount, 
                            total_discount AS TotalDiscount = @TotalDiscount, 
                            total_excise_duty AS TotalExciseDuty = @TotalExciseDuty, 
                            total_tax AS TotalTax = @TotalTax, 
                            net_amount AS NetAmount = @NetAmount, 
                            sku_price_uid AS SKUPriceUID = @SKUPriceUID, 
                            sku_price_list_uid AS SKUPriceListUID = @SKUPriceListUID, 
                            reason_code AS ReasonCode = @ReasonCode, 
                            reason_text AS ReasonText = @ReasonText, 
                            expiry_date AS ExpiryDate = @ExpiryDate, 
                            batch_number AS BatchNumber = @BatchNumber, 
                            sales_order_uid AS SalesOrderUID = @SalesOrderUID, 
                            sales_order_line_uid AS SalesOrderLineUID = @SalesOrderLineUID, 
                            remarks AS Remarks = @Remarks, 
                            volume AS Volume = @Volume, 
                            volume_unit AS VolumeUnit = @VolumeUnit, 
                            promotion_uid AS PromotionUID = @PromotionUID, 
                            net_fake_amount AS NetFakeAmount = @NetFakeAmount,
                            po_number AS PONumber = @PONumber
                             WHERE uid = @UID;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "@UID", returnOrderLine.UID },
                    { "@CreatedBy", returnOrderLine.CreatedBy },
                    { "@CreatedTime", returnOrderLine.CreatedTime },
                    { "@ModifiedBy", returnOrderLine.ModifiedBy },
                    { "@ModifiedTime", returnOrderLine.ModifiedTime },
                    { "@ServerAddTime", returnOrderLine.ServerAddTime },
                    { "@ServerModifiedTime", returnOrderLine.ServerModifiedTime },
                    { "@ReturnOrderUID", returnOrderLine.ReturnOrderUID },
                    { "@LineNumber", returnOrderLine.LineNumber },
                    { "@SKUUID", returnOrderLine.SKUUID },
                    { "@SKUCode", returnOrderLine.SKUCode },
                    { "@SKUType", returnOrderLine.SKUType },
                    { "@BasePrice", returnOrderLine.BasePrice },
                    { "@UnitPrice", returnOrderLine.UnitPrice },
                    { "@FakeUnitPrice", returnOrderLine.FakeUnitPrice },
                    { "@BaseUOM", returnOrderLine.BaseUOM },
                    { "@UoM", returnOrderLine.UoM },
                    { "@Multiplier", returnOrderLine.Multiplier },
                    { "@Qty", returnOrderLine.Qty },
                    { "@QtyBU", returnOrderLine.QtyBU },
                    { "@ApprovedQty", returnOrderLine.ApprovedQty },
                    { "@ReturnedQty", returnOrderLine.ReturnedQty },
                    { "@TotalAmount", returnOrderLine.TotalAmount },
                    { "@TotalDiscount", returnOrderLine.TotalDiscount },
                    { "@TotalExciseDuty", returnOrderLine.TotalExciseDuty },
                    { "@TotalTax", returnOrderLine.TotalTax },
                    { "@NetAmount", returnOrderLine.NetAmount },
                    { "@SKUPriceUID", returnOrderLine.SKUPriceUID },
                    { "@SKUPriceListUID", returnOrderLine.SKUPriceListUID },
                    { "@ReasonCode", returnOrderLine.ReasonCode },
                    { "@ReasonText", returnOrderLine.ReasonText },
                    { "@ExpiryDate", returnOrderLine.ExpiryDate },
                    { "@BatchNumber", returnOrderLine.BatchNumber },
                    { "@SalesOrderUID", returnOrderLine.SalesOrderUID },
                    { "@SalesOrderLineUID", returnOrderLine.SalesOrderLineUID },
                    { "@Remarks", returnOrderLine.Remarks },
                    { "@Volume", returnOrderLine.Volume },
                    { "@VolumeUnit", returnOrderLine.VolumeUnit },
                    { "@PromotionUID", returnOrderLine.PromotionUID },
                    { "@NetFakeAmount", returnOrderLine.NetFakeAmount },
                    { "@PONumber", returnOrderLine.PONumber },
                };
                return await ExecuteNonQueryAsync(sql, parameters);

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
