using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;
using Dapper;
using Winit.Modules.PurchaseOrder.DL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Classes;
using Winit.Modules.PurchaseOrder.Model.Interfaces;

namespace Winit.Modules.PurchaseOrder.DL.Classes;

public class PGSQLStockReceivingDetailDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IStockReceivingDetailDL
{
    public PGSQLStockReceivingDetailDL(IServiceProvider serviceProvider, IConfiguration config)
        : base(serviceProvider, config)
    {
    }

    public async Task<IEnumerable<IStockReceivingDetail>> GetByPurchaseOrderUIDAsync(string purchaseOrderUID)
    {
        try
        {
            string sql = @"
                SELECT
                    ""UID""::text as ""UID"",
                    ""PurchaseOrderUID"",
                    ""PurchaseOrderLineUID""::text as ""PurchaseOrderLineUID"",
                    ""SKUCode"",
                    ""SKUName"",
                    ""OrderedQty"",
                    ""ReceivedQty"",
                    ""AdjustmentReason"",
                    ""AdjustmentQty"",
                    ""ImageURL"",
                    ""IsActive"",
                    ""CreatedDate"",
                    ""CreatedBy""::text as ""CreatedBy"",
                    ""ModifiedDate"",
                    ""ModifiedBy""::text as ""ModifiedBy""
                FROM public.""StockReceivingDetail""
                WHERE ""PurchaseOrderUID"" = @PurchaseOrderUID
                AND ""IsActive"" = true
                ORDER BY ""CreatedDate"" ASC";

            var parameters = new
            {
                PurchaseOrderUID = purchaseOrderUID
            };

            using var connection = new NpgsqlConnection(_connectionString);
            var result = await connection.QueryAsync<StockReceivingDetail>(sql, parameters);
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetByPurchaseOrderUIDAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> SaveStockReceivingDetailsAsync(IEnumerable<IStockReceivingDetail> details)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                foreach (var detail in details)
                {
                    string sql = @"
                        INSERT INTO public.""StockReceivingDetail""
                        (
                            ""UID"",
                            ""PurchaseOrderUID"",
                            ""PurchaseOrderLineUID"",
                            ""SKUCode"",
                            ""SKUName"",
                            ""OrderedQty"",
                            ""ReceivedQty"",
                            ""AdjustmentReason"",
                            ""AdjustmentQty"",
                            ""ImageURL"",
                            ""IsActive"",
                            ""CreatedDate"",
                            ""CreatedBy""
                        )
                        VALUES
                        (
                            @UID,
                            @PurchaseOrderUID,
                            @PurchaseOrderLineUID,
                            @SKUCode,
                            @SKUName,
                            @OrderedQty,
                            @ReceivedQty,
                            @AdjustmentReason,
                            @AdjustmentQty,
                            @ImageURL,
                            @IsActive,
                            @CreatedDate,
                            @CreatedBy
                        )";

                    var uid = string.IsNullOrEmpty(detail.UID) ? Guid.NewGuid() : Guid.Parse(detail.UID);

                    var parameters = new
                    {
                        UID = uid,
                        PurchaseOrderUID = detail.PurchaseOrderUID,
                        PurchaseOrderLineUID = detail.PurchaseOrderLineUID,
                        SKUCode = detail.SKUCode,
                        SKUName = detail.SKUName,
                        OrderedQty = detail.OrderedQty,
                        ReceivedQty = detail.ReceivedQty,
                        AdjustmentReason = detail.AdjustmentReason,
                        AdjustmentQty = detail.AdjustmentQty,
                        ImageURL = detail.ImageURL,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = detail.CreatedBy
                    };

                    await connection.ExecuteAsync(sql, parameters, transaction);
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SaveStockReceivingDetailsAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteByPurchaseOrderUIDAsync(string purchaseOrderUID)
    {
        try
        {
            string sql = @"
                UPDATE public.""StockReceivingDetail""
                SET ""IsActive"" = false,
                    ""ModifiedDate"" = @ModifiedDate
                WHERE ""PurchaseOrderUID"" = @PurchaseOrderUID";

            var parameters = new
            {
                PurchaseOrderUID = purchaseOrderUID,
                ModifiedDate = DateTime.UtcNow
            };

            var rowsAffected = await ExecuteNonQueryAsync(sql, parameters);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in DeleteByPurchaseOrderUIDAsync: {ex.Message}");
            throw;
        }
    }
}
