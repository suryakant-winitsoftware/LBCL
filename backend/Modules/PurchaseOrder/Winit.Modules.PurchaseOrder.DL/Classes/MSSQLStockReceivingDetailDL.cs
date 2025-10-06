using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using Winit.Modules.PurchaseOrder.DL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Classes;
using Winit.Modules.PurchaseOrder.Model.Interfaces;

namespace Winit.Modules.PurchaseOrder.DL.Classes;

public class MSSQLStockReceivingDetailDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IStockReceivingDetailDL
{
    public MSSQLStockReceivingDetailDL(IServiceProvider serviceProvider, IConfiguration config)
        : base(serviceProvider, config)
    {
    }

    public async Task<IEnumerable<IStockReceivingDetail>> GetByPurchaseOrderUIDAsync(string purchaseOrderUID)
    {
        try
        {
            string sql = @"
                SELECT
                    CAST([UID] AS VARCHAR(50)) as [UID],
                    [PurchaseOrderUID],
                    CAST([PurchaseOrderLineUID] AS VARCHAR(50)) as [PurchaseOrderLineUID],
                    [SKUCode],
                    [SKUName],
                    [OrderedQty],
                    [ReceivedQty],
                    [AdjustmentReason],
                    [AdjustmentQty],
                    [ImageURL],
                    [IsActive],
                    [CreatedDate],
                    CAST([CreatedBy] AS VARCHAR(250)) as [CreatedBy],
                    [ModifiedDate],
                    CAST([ModifiedBy] AS VARCHAR(250)) as [ModifiedBy]
                FROM [dbo].[StockReceivingDetail]
                WHERE [PurchaseOrderUID] = @PurchaseOrderUID
                AND [IsActive] = 1
                ORDER BY [CreatedDate] ASC";

            var parameters = new
            {
                PurchaseOrderUID = purchaseOrderUID
            };

            using var connection = new SqlConnection(_connectionString);
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
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                foreach (var detail in details)
                {
                    string sql = @"
                        INSERT INTO [dbo].[StockReceivingDetail]
                        (
                            [UID],
                            [PurchaseOrderUID],
                            [PurchaseOrderLineUID],
                            [SKUCode],
                            [SKUName],
                            [OrderedQty],
                            [ReceivedQty],
                            [AdjustmentReason],
                            [AdjustmentQty],
                            [ImageURL],
                            [IsActive],
                            [CreatedDate],
                            [CreatedBy]
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

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
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
                UPDATE [dbo].[StockReceivingDetail]
                SET [IsActive] = 0,
                    [ModifiedDate] = @ModifiedDate
                WHERE [PurchaseOrderUID] = @PurchaseOrderUID";

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
